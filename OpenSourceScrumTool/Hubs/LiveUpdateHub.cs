using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OpenSourceScrumTool.Extensions;
using OpenSourceScrumTool.Models;
using OpenSourceScrumTool.Models.DataModels;
using OpenSourceScrumTool.Models.ViewModels;
using System.Threading.Tasks;

using projectList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.DataModels.Project>>;
using featureList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.DataModels.Feature>>;
using taskList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.DataModels.ScrumTask>>;
using sprintList = System.Collections.Generic.List<OpenSourceScrumTool.Models.DataModels.Sprint>;
using userList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.ApplicationUser>>;
using colleaguesList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.DataModels.UserColleagues>>;
using teamList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.DataModels.Team>>;

namespace OpenSourceScrumTool.Hubs
{
    public enum ViewModelDataType
    {
        Project,
        Feature,
        Task,
        Sprint,
        Colleague,
        Team
    }

    public enum ActionType
    {
        Details,
        Index,
        Create,
        Edit,
        Delete,
        ManageUsers,
        Order,
        EditStatus
    }

    [Authorize]
    public class LiveUpdateHub : Hub
    {
        private static readonly List<string> OnlineUsers = new List<string>();
        private static readonly Dictionary<string, string> CurrentEditingUsers = new Dictionary<string, string>();
        private static readonly Dictionary<string, List<string>> EditQueue = new Dictionary<string, List<string>>();
        private static readonly ReaderWriterLockSlim QueueLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        #region SignalR Events
        public override Task OnConnected()
        {
            OnlineUsers.Add(Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            OnlineUsers.Remove(Context.ConnectionId);

            QueueLock.EnterWriteLock();
            try
            {
                //Remove any locks from this user & from any queue
                if (CurrentEditingUsers.ContainsValue(Context.ConnectionId))
                {
                    var heldLocks = CurrentEditingUsers
                        .Where(pair => pair.Value == Context.ConnectionId)
                        .Select(vlock => vlock.Key)
                        .ToArray();
                    for (var i = 0; i < heldLocks.Length; i++)
                    {
                        CurrentEditingUsers.Remove(heldLocks[i]);
                        PassLockOn(heldLocks[i]);
                    }
                }
                foreach (var queue in EditQueue)
                {
                    var removeList = queue.Value.Where(s => s == Context.ConnectionId).ToArray();
                    foreach (var remove in removeList)
                        queue.Value.Remove(remove);
                }
            }
            finally
            {
                QueueLock.ExitWriteLock();
            }

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }
        #endregion

        public bool StartMonitoring_ProjectDetailsViewModel(ProjectDetailsViewModel model)
        {
            //Add this user as watching this project update
            Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(ViewModelDataType.Project, ActionType.Details, model.Id));

            Task.Factory.StartNew(() =>
            {
                //Check if the model has changed, call update if it has
                using (var db = new ApplicationDbContext())
                {
                    var project = db.Projects.Find(model.Id);
                    var newModel = new ProjectDetailsViewModel(project);
                    if (!model.PublicInstancePropertiesEqual(newModel))
                    {
                        LiveUpdateSingleton.Instance.EntityUpdated(new projectList() { new Tuple<EntityState, Project>(EntityState.Modified, project) });
                    }
                }
            });
            return true;
        }

        public bool StartMonitoring_ProjectIndexViewModel(ProjectIndexViewModel model)
        {
            //Find the user's Id
            string userId = HttpContext.Current.GetOwinContext().Authentication.User.Identity.GetUserId();
            //Add this user as watching for Project Adds
            Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(userId));
            //Add this user as watching updates on each project
            foreach (var project in model.Projects)
            {
                Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(ViewModelDataType.Project, ActionType.Index, project.Id));
            }

            //We can't check if a project should have been added or access revoked/deleted because we don't know the user
            //But we can check if it has changed
            Task.Factory.StartNew(() =>
            {
                //Check if the model has changed, call update if it has
                using (var db = new ApplicationDbContext())
                {
                    //Check features in list
                    foreach (var oldProject in model.Projects)
                    {
                        var project = db.Projects.Find(oldProject.Id);
                        if (project == null)
                        {
                            //Data has been removed!
                            continue;
                        }

                        var newProjectItem = new ProjectItemViewModel(project);
                        if (!oldProject.PublicInstancePropertiesEqual(newProjectItem))
                        {
                            //Data has changed
                            LiveUpdateSingleton.Instance.EntityUpdated(new projectList() { new Tuple<EntityState, Project>(EntityState.Modified, project) });
                        }
                    }
                }
            });
            return true;
        }

        public bool StartMonitoring_FeatureDetailsViewModel(FeatureDetailsViewModel model)
        {
            //Add this user as watching this feature update
            Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(ViewModelDataType.Feature, ActionType.Details, model.Id));

            Task.Factory.StartNew(() =>
            {
                //Check if the model has changed, call update if it has
                using (var db = new ApplicationDbContext())
                {
                    var feature = db.Features.Find(model.Id);
                    var newModel = new FeatureDetailsViewModel(feature);
                    if (!model.PublicInstancePropertiesEqual(newModel))
                    {
                        LiveUpdateSingleton.Instance.EntityUpdated(new featureList() { new Tuple<EntityState, Feature>(EntityState.Modified, feature) });
                    }
                }
            });
            return true;
        }

        public bool StartMonitoring_FeatureIndexViewModel(FeatureIndexViewModel model)
        {
            //Add this user as watching features for this project id
            Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(ViewModelDataType.Feature, ActionType.Index, model.ProjectId));

            Task.Factory.StartNew(() =>
            {
                //Check if the model has changed, call update if it has
                using (var db = new ApplicationDbContext())
                {
                    var project = db.Projects.Find(model.ProjectId);
                    var newModel = new FeatureIndexViewModel(project);
                    //Check features in list
                    foreach (var oldFeature in model.Features)
                    {
                        var newFeature = newModel.Features.Find(feature => feature.Id == oldFeature.Id);
                        if (newFeature == null)
                        {
                            //Data has been removed!
                            LiveUpdateSingleton.Instance.EntityUpdated(new featureList() { new Tuple<EntityState, Feature>(EntityState.Deleted, new Feature() { Id = oldFeature.Id, ProjectId = model.ProjectId }) });
                        }
                        else if (!oldFeature.PublicInstancePropertiesEqual(newFeature))
                        {
                            //Data has changed
                            var feature = db.Features.Find(oldFeature.Id);
                            LiveUpdateSingleton.Instance.EntityUpdated(new featureList() { new Tuple<EntityState, Feature>(EntityState.Modified, feature) });
                        }
                    }
                    foreach (var newFeature in newModel.Features)
                    {
                        var oldFeature = model.Features.Find(feature => feature.Id == newFeature.Id);
                        if (oldFeature == null)
                        {
                            //Data has been added!
                            var feature = db.Features.Find(newFeature.Id);
                            LiveUpdateSingleton.Instance.EntityUpdated(new featureList() { new Tuple<EntityState, Feature>(EntityState.Added, feature) });
                        }
                    }
                }
            });
            return true;
        }

        public bool StartMonitoring_TaskIndexViewModel(TaskIndexViewModel model)
        {
            //Add this user as watching tasks for this feature id
            Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(ViewModelDataType.Task, ActionType.Index, model.FeatureId));

            Task.Factory.StartNew(() =>
            {
                //Check if the model has changed, call update if it has
                using (var db = new ApplicationDbContext())
                {
                    var feature = db.Features.Find(model.FeatureId);
                    var newModel = new TaskIndexViewModel(feature);
                    //Check task in list
                    foreach (var oldTask in model.Tasks)
                    {
                        var newTask = newModel.Tasks.Find(task => task.Id == oldTask.Id);
                        if (newTask == null)
                        {
                            //Data has been removed!
                            LiveUpdateSingleton.Instance.EntityUpdated(new taskList() { new Tuple<EntityState, Models.DataModels.ScrumTask>(EntityState.Deleted, new Models.DataModels.ScrumTask() { Id = oldTask.Id, FeatureId = model.FeatureId }) });
                        }
                        else if (!oldTask.PublicInstancePropertiesEqual(newTask))
                        {
                            //Data has changed
                            var task = db.Tasks.Find(oldTask.Id);
                            LiveUpdateSingleton.Instance.EntityUpdated(new taskList() { new Tuple<EntityState, Models.DataModels.ScrumTask>(EntityState.Modified, task) });
                        }
                    }
                    foreach (var newTask in newModel.Tasks)
                    {
                        var oldTask = model.Tasks.Find(task => task.Id == newTask.Id);
                        if (oldTask == null)
                        {
                            //Data has been added!
                            var task = db.Tasks.Find(newTask.Id);
                            LiveUpdateSingleton.Instance.EntityUpdated(new taskList() { new Tuple<EntityState, Models.DataModels.ScrumTask>(EntityState.Added, task) });
                        }
                    }
                }
            });
            return true;
        }

        public bool StartMonitoring_ColleagueViewModel(ColleaguesViewModel model)
        {
            //Find the user's Id
            string userId = HttpContext.Current.GetOwinContext().Authentication.User.Identity.GetUserId();
            //Add this user as watching for Colleague Adds
            Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(userId));
            //Add this user as watching updates on each colleague
            Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(ViewModelDataType.Colleague, ActionType.Index, sId: userId));
            foreach (var colleague in model.Colleagues)
            {
                Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(ViewModelDataType.Colleague, ActionType.Details, sId: colleague.Email));
            }

            //Check if the model has changed
            Task.Factory.StartNew(() =>
            {
                //Check if the model has changed, call update if it has
                using (var db = new ApplicationDbContext())
                {
                    using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db)))
                    {
                        var thisUser = userManager.FindById(userId);
                        foreach (var userColleagues in thisUser.Colleagues)
                        {
                            //Find this in model
                            if (model.Colleagues.Any(item => item.Email == userColleagues.Colleague.Email))
                            {
                                var oldColleague =
                                    model.Colleagues.First(item => item.Email == userColleagues.Colleague.Email);

                                var newModel = new ColleagueItemViewModel(userColleagues.Colleague);
                                //In new and old
                                if (!oldColleague.PublicInstancePropertiesEqual(newModel))
                                {
                                    //Data has changed
                                    //Push change
                                    LiveUpdateSingleton.Instance.EntityUpdated(new colleaguesList()
                                    {
                                        new Tuple<EntityState, UserColleagues>(EntityState.Modified, userColleagues)
                                    });
                                }

                                //Remove from model list
                                model.Colleagues.Remove(oldColleague);
                            }
                            else
                            {
                                //Data has been added from database
                                //Push add
                                LiveUpdateSingleton.Instance.EntityUpdated(new colleaguesList()
                                {
                                    new Tuple<EntityState, UserColleagues>(EntityState.Added, userColleagues)
                                });
                            }
                        }
                    }

                    if (model.Colleagues.Any())
                    {
                        //Items in the list have been deleted on the server
                        foreach (var removedColleague in model.Colleagues)
                        {
                            //Push delete
                            var detailsGroup =
                                        Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Colleague,
                                            ActionType.Details, sId: removedColleague.Email));
                            detailsGroup.RemoveData(ViewModelDataType.Colleague.ToString(),
                                ActionType.Index.ToString(), removedColleague.Email);
                        }
                    }
                }
            });
            return true;
        }

        public bool StartMonitoring_TeamViewModel(TeamsViewModel model)
        {
            //Find the user's Id
            string userId = HttpContext.Current.GetOwinContext().Authentication.User.Identity.GetUserId();
            //Add this user as watching for Colleague Adds
            Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(userId));
            //Add this user as watching updates on each colleague
            Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(ViewModelDataType.Team, ActionType.Index, sId: userId));
            foreach (var team in model.Teams)
            {
                Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(ViewModelDataType.Team, ActionType.Details, team.Id));
            }

            //Check if the model has changed
            Task.Factory.StartNew(() =>
            {
                //Check if the model has changed, call update if it has
                using (var db = new ApplicationDbContext())
                {
                    using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db)))
                    {
                        var thisUser = userManager.FindById(userId);
                        foreach (var userTeams in thisUser.Teams)
                        {
                            //Find this in model
                            if (model.Teams.Any(item => item.Id == userTeams.Team_Id))
                            {
                                var oldTeam =
                                    model.Teams.First(item => item.Id == userTeams.Team_Id);

                                var newModel = new TeamItemViewModel(userTeams.Team);
                                //In new and old
                                if (!oldTeam.PublicInstancePropertiesEqual(newModel))
                                {
                                    //Data has changed
                                    //Push change
                                    LiveUpdateSingleton.Instance.EntityUpdated(new teamList()
                                    {
                                        new Tuple<EntityState, Team>(EntityState.Modified, userTeams.Team)
                                    });
                                }

                                //Remove from model list
                                model.Teams.Remove(oldTeam);
                            }
                            else
                            {
                                //Data has been added from database
                                //Push add
                                LiveUpdateSingleton.Instance.EntityUpdated(new teamList()
                                {
                                    new Tuple<EntityState, Team>(EntityState.Added, userTeams.Team)
                                });
                            }
                        }
                    }

                    if (model.Teams.Any())
                    {
                        //Items in the list have been deleted on the server
                        foreach (var removedTeam in model.Teams)
                        {
                            //Push delete
                            var detailsGroup =
                                        Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Team,
                                            ActionType.Details, removedTeam.Id));
                            detailsGroup.RemoveData(ViewModelDataType.Team.ToString(),
                                ActionType.Index.ToString(), removedTeam.Id);
                        }
                    }
                }
            });

            return true;
        }

        public bool StartMonitoring_SprintIndexViewModel(SprintIndexViewModel model)
        {
            //Find the user's Id
            string userId = HttpContext.Current.GetOwinContext().Authentication.User.Identity.GetUserId();
            //Add this user as watching for Colleague Adds
            Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(userId));
            //Add this user as watching updates on each colleague
            Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(ViewModelDataType.Sprint, ActionType.Index, sId: userId));
            foreach (var proj in model.Projects)
            {
                Groups.Add(Context.ConnectionId, LiveUpdateHelper.GroupName(ViewModelDataType.Sprint, ActionType.Details, proj.ProjectId));
            }

            //TODO: check if the model has changed, god knows how to do this when it's so big...
            return true;
        }

        public bool Edit_RequestLock(string viewType, int id)
        {
            QueueLock.EnterWriteLock();
            try
            {
                string lockName = joinLockName(viewType, id);
                if (CurrentEditingUsers.ContainsKey(lockName))
                {
                    //if the queue doesn't exist yet, create one
                    if (!EditQueue.ContainsKey(lockName))
                        EditQueue.Add(lockName, new List<string>());

                    //Add the user to the queue
                    EditQueue[lockName].Add(Context.ConnectionId);

                    //Return a failed request attempt
                    return false;
                }
                else
                {
                    //Store this user as currently having the lock
                    CurrentEditingUsers.Add(lockName, Context.ConnectionId);
                    return true;
                }
            }
            finally
            {
                QueueLock.ExitWriteLock();
            }
        }

        public void Edit_StopLock(string viewType, int id)
        {
            QueueLock.EnterWriteLock();
            try
            {
                string lockName = joinLockName(viewType, id);

                //Remove user from queue
                if (EditQueue.ContainsKey(lockName) && EditQueue[lockName] != null)
                {
                    var removeList = EditQueue[lockName].Where(s => s == Context.ConnectionId).ToArray();
                    foreach (var remove in removeList)
                        EditQueue[lockName].Remove(remove);
                }

                //Remove current lock if they have it
                if (CurrentEditingUsers.ContainsKey(lockName)
                    && CurrentEditingUsers[lockName] == Context.ConnectionId)
                {
                    CurrentEditingUsers.Remove(lockName);
                    PassLockOn(lockName);
                }
            }
            finally
            {
                QueueLock.ExitWriteLock();
            }
        }

        public void Edit_StopAllLocks()
        {
            QueueLock.EnterWriteLock();
            try
            {
                var heldLocks = CurrentEditingUsers
                    .Where(pair => pair.Value == Context.ConnectionId)
                    .Select(vlock => vlock.Key)
                    .ToArray();
                for (var i = 0; i < heldLocks.Length; i++)
                {
                    CurrentEditingUsers.Remove(heldLocks[i]);
                    PassLockOn(heldLocks[i]);
                }
            }
            finally
            {
                QueueLock.ExitWriteLock();
            }
        }

        public bool Edit_TakeControl(string viewType, int id)
        {
            QueueLock.EnterWriteLock();
            try
            {
                string lockName = joinLockName(viewType, id);
                if (CurrentEditingUsers.ContainsKey(lockName))
                {
                    //Queue the old user
                    var prevUser = CurrentEditingUsers[lockName];
                    EditQueue[lockName].Add(prevUser);

                    //Tell that user they have been removed
                    Clients.Client(prevUser).edit_RemoveAccess();

                    //Remove the old access
                    CurrentEditingUsers.Remove(lockName);
                }

                //Store this user as currently having the lock
                CurrentEditingUsers.Add(lockName, Context.ConnectionId);
                return true;
            }
            finally
            {
                QueueLock.ExitWriteLock();
            }
        }

        private void PassLockOn(string lockName)
        {
            QueueLock.EnterWriteLock();
            try
            {
                //Check if anyone else wanted control
                if (EditQueue.ContainsKey(lockName))
                {
                    bool userFound = false;
                    while (EditQueue[lockName].Any() && !userFound)
                    {
                        string nextUser = EditQueue[lockName].First();
                        if (nextUser != null)
                            EditQueue[lockName].RemoveAt(0);

                        if (OnlineUsers.Contains(nextUser))
                        {
                            userFound = true;
                            Clients.Client(nextUser).edit_UnlockDialog();
                            CurrentEditingUsers.Add(lockName, nextUser);
                        }
                    }
                }
            }
            finally
            {
                QueueLock.ExitWriteLock();
            }
        }

        private string joinLockName(string viewType, int id)
        {
            return string.Format("{0}-{1}", viewType, id);
        }
    }

    public class LiveUpdateSingleton
    {
        #region Singleton Setup
        public static LiveUpdateSingleton Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        // Singleton instance
        private readonly static Lazy<LiveUpdateSingleton> _instance = new Lazy<LiveUpdateSingleton>(
            () => new LiveUpdateSingleton(GlobalHost.ConnectionManager.GetHubContext<LiveUpdateHub>().Clients));

        private IHubConnectionContext<dynamic> Clients { get; set; }

        private LiveUpdateSingleton(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
        }
        #endregion

        public void EntityUpdated(projectList updatedEntities)
        {
            using (var db = new ApplicationDbContext())
            {
                Trace.WriteLine("Updated Project");
                foreach (var tuple in updatedEntities)
                {
                    switch (tuple.Item1)
                    {
                        case EntityState.Modified:
                            {
                                var dbProject = db.Projects.Find(tuple.Item2.Id);
                                if (dbProject == null)
                                    continue; //Project has been deleted

                                //Update Project Details box
                                var detailsViewModel = new ProjectDetailsViewModel(dbProject);
                                Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Project,
                                    ActionType.Details, dbProject.Id))
                                    .UpdateData(ViewModelDataType.Project.ToString(),
                                        ActionType.Details.ToString(), detailsViewModel);

                                //Update Projects List
                                var projectItemViewModel = new ProjectItemViewModel(dbProject);
                                Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Project,
                                    ActionType.Index, dbProject.Id))
                                    .UpdateItemData(ViewModelDataType.Project.ToString(),
                                        ActionType.Index.ToString(), projectItemViewModel);

                                //Update Sprint Board
                                foreach (var team in dbProject.Teams)
                                {
                                    var thisSprint = dbProject.GetThisSprint();
                                    var thisProjSprint = new ProjectSprintViewModel()
                                    {
                                        ProjectId = dbProject.Id,
                                        ProjectName = dbProject.Title,
                                        TeamName = team.Team.TeamName,
                                        SprintItteration = thisSprint.Iteration,
                                        SprintStartDate = thisSprint.StartDate,
                                        SprintEndDate = thisSprint.EndDate,
                                        Features = new List<SprintFeatures>()
                                    };
                                    //Add features to view model
                                    foreach (var feat in thisSprint.Features.OrderBy(f => f.Priority))
                                    {
                                        thisProjSprint.Features.Add(new SprintFeatures()
                                        {
                                            FeatureId = feat.Id,
                                            FeatureName = feat.Title,
                                            FeatureDescription = feat.Description,
                                            TasksNotStarted =
                                                feat.Tasks.Where(t => t.State == EnumTaskState.NotStarted)
                                                    .OrderBy(p => p.Priority)
                                                    .Select(t => new TaskItemViewModel(t))
                                                    .ToList(),
                                            TasksInProgress =
                                                feat.Tasks.Where(t => t.State == EnumTaskState.InProgress)
                                                    .OrderBy(p => p.Priority)
                                                    .Select(t => new TaskItemViewModel(t))
                                                    .ToList(),
                                            TasksDone =
                                                feat.Tasks.Where(t => t.State == EnumTaskState.Done)
                                                    .OrderBy(p => p.Priority)
                                                    .Select(t => new TaskItemViewModel(t))
                                                    .ToList()
                                        });
                                    }

                                    //Push to group
                                    Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Sprint, ActionType.Details, dbProject.Id))
                                    .UpdateItemData(ViewModelDataType.Sprint.ToString(), ActionType.Index.ToString(), thisProjSprint);
                                }
                            }
                            break;
                        case EntityState.Added:
                            {
                                //Find owner and push add
                                var dbProject = db.Projects.Find(tuple.Item2.Id);
                                if (dbProject == null)
                                    continue; //Project has been deleted

                                var projectItemViewModel = new ProjectItemViewModel(dbProject);
                                var group = Clients.Group(LiveUpdateHelper.GroupName(dbProject.User_Id));
                                group.AddData(ViewModelDataType.Project.ToString(),
                                    ActionType.Index.ToString(), projectItemViewModel);
                            }
                            break;
                        case EntityState.Deleted:
                            {
                                //Find owner and push delete
                                if (tuple.Item2 != null)
                                {
                                    var group = Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Project, ActionType.Index, tuple.Item2.Id));
                                    group.RemoveData(ViewModelDataType.Project.ToString(),
                                        ActionType.Index.ToString(), tuple.Item2.Id);
                                }
                            }
                            break;
                    }
                }
            }
        }

        public void EntityUpdated(featureList updatedEntities)
        {
            using (var db = new ApplicationDbContext())
            {
                Trace.WriteLine("Updated Feature");
                foreach (var entry in updatedEntities)
                {
                    var dbFeature = db.Features.Find(entry.Item2.Id);

                    var indexGroup =
                        Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Feature,
                            ActionType.Index, entry.Item2.ProjectId));
                    var detailsGroup =
                        Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Feature,
                            ActionType.Details, entry.Item2.Id));

                    switch (entry.Item1)
                    {
                        case EntityState.Added:
                            //Add feature to list
                            var featureItemViewModelA = new FeatureItemViewModel(dbFeature);
                            indexGroup.AddData(ViewModelDataType.Feature.ToString(),
                                ActionType.Index.ToString(), featureItemViewModelA);
                            break;
                        case EntityState.Deleted:
                            //Remove Feature from list
                            indexGroup.RemoveData(ViewModelDataType.Feature.ToString(),
                                ActionType.Index.ToString(), entry.Item2.Id);
                            break;
                        case EntityState.Modified:
                            //Update Features Details box
                            var detailsViewModel = new FeatureDetailsViewModel(dbFeature);
                            detailsGroup.UpdateData(ViewModelDataType.Feature.ToString(),
                                ActionType.Details.ToString(), detailsViewModel);

                            //Update Features List
                            var featureItemViewModelM = new FeatureItemViewModel(dbFeature);
                            indexGroup.UpdateItemData(ViewModelDataType.Feature.ToString(),
                                ActionType.Index.ToString(), featureItemViewModelM);
                            break;
                    }
                }
            }
        }

        public void EntityUpdated(taskList updatedEntities)
        {
            using (var db = new ApplicationDbContext())
            {
                Trace.WriteLine("Updated Tasks");
                foreach (var entry in updatedEntities)
                {
                    var dbTask = db.Tasks.Find(entry.Item2.Id);

                    var indexGroup =
                        Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Task,
                            ActionType.Index, entry.Item2.FeatureId));

                    switch (entry.Item1)
                    {
                        case EntityState.Added:
                            //Add Task to list
                            var taskItemViewModelA = new TaskItemViewModel(dbTask);
                            indexGroup.AddData(ViewModelDataType.Task.ToString(),
                                ActionType.Index.ToString(), taskItemViewModelA);
                            break;
                        case EntityState.Deleted:
                            //Remove Task from list
                            indexGroup.RemoveData(ViewModelDataType.Task.ToString(),
                                ActionType.Index.ToString(), entry.Item2.Id);
                            break;
                        case EntityState.Modified:
                            //Update Tasks List
                            var taskItemViewModelM = new TaskItemViewModel(dbTask);
                            indexGroup.UpdateItemData(ViewModelDataType.Task.ToString(),
                                ActionType.Index.ToString(), taskItemViewModelM);
                            break;
                    }
                }
            }
        }

        public void EntityUpdated(sprintList updatedEntities)
        {
            Trace.WriteLine("Updated Sprint");
            //TODO: Something else here???
        }

        public void EntityUpdated(userList updatedEntities)
        {
            //TODO: Handle user update
        }

        public void EntityUpdated(colleaguesList updatedEntities)
        {
            using (var db = new ApplicationDbContext())
            {
                using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db)))
                {
                    Trace.WriteLine("Updated Colleague");
                    foreach (var entry in updatedEntities)
                    {
                        var dbColleague = userManager.FindByIdAsync(entry.Item2.ColleagueId);

                        switch (entry.Item1)
                        {
                            case EntityState.Added:
                                {
                                    //Add Colleague to list
                                    var colleagueItemA = new ColleagueItemViewModel(dbColleague.Result);
                                    var indexGroup =
                                        Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Colleague,
                                            ActionType.Index, sId: entry.Item2.UserId));
                                    indexGroup.AddData(ViewModelDataType.Colleague.ToString(),
                                        ActionType.Index.ToString(), colleagueItemA);
                                }
                                break;
                            case EntityState.Deleted:
                                {
                                    //Remove Colleague from list (by email)
                                    var detailsGroup =
                                        Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Colleague,
                                            ActionType.Details, sId: dbColleague.Result.Email));
                                    detailsGroup.RemoveData(ViewModelDataType.Colleague.ToString(),
                                        ActionType.Index.ToString(), dbColleague.Result.Email);
                                }
                                break;
                            case EntityState.Modified:
                                //Not sure when this will happen, so lets ignore it...
                                Trace.WriteLine("UserColleague has been modified and i don't know why...");
                                break;
                        }
                    }
                }
            }
        }

        public void EntityUpdated(teamList updatedEntities)
        {
            Trace.WriteLine("Updated Team");
            foreach (var entry in updatedEntities)
            {
                switch (entry.Item1)
                {
                    case EntityState.Added:
                        {
                            using (var db = new ApplicationDbContext())
                            {
                                var dbTeam = db.Teams.FindAsync(entry.Item2.Id);
                                //Add Team to list
                                var teamItem = new TeamItemViewModel(dbTeam.Result);
                                //This shouldn't ever be null as we set it when we create a team (get owner's Id)
                                var ownerId = dbTeam.Result.Members.Select(member => member.User).First().Id;
                                //Index by UserId
                                var indexGroup =
                                    Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Team,
                                        ActionType.Index, sId: ownerId));
                                indexGroup.AddData(ViewModelDataType.Team.ToString(),
                                    ActionType.Index.ToString(), teamItem);
                            }
                        }
                        break;
                    case EntityState.Deleted:
                        {
                            //Remove Team from list
                            //Details by TeamId
                            var detailsGroup =
                                Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Team,
                                    ActionType.Details, entry.Item2.Id));
                            detailsGroup.RemoveData(ViewModelDataType.Team.ToString(),
                                ActionType.Index.ToString(), entry.Item2.Id);
                        }
                        break;
                    case EntityState.Modified:
                        {
                            using (var db = new ApplicationDbContext())
                            {
                                var dbTeam = db.Teams.FindAsync(entry.Item2.Id);

                                //Team may have also been deleted
                                //This happens when you delete a team (all the users trigger a modified on the team)
                                if (dbTeam.Result == null)
                                    continue;

                                //Add Team to list
                                var teamItem = new TeamItemViewModel(dbTeam.Result);
                                //Details by TeamId
                                var detailsGroup =
                                    Clients.Group(LiveUpdateHelper.GroupName(ViewModelDataType.Team,
                                        ActionType.Details, dbTeam.Result.Id));
                                detailsGroup.UpdateItemData(ViewModelDataType.Team.ToString(),
                                    ActionType.Index.ToString(), teamItem);
                            }
                        }
                        break;
                }

            }
        }
    }

    public static class LiveUpdateHelper
    {
        public static string GroupName(ViewModelDataType model, ActionType mode, int? id = null, string sId = null)
        {
            return string.Format("{0}-{1}-{2}", model, mode, id != null ? id.ToString() : sId);
        }
        public static string GroupName(string userId)
        {
            return string.Format("User-{0}", userId);
        }
    }
}
