using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OpenSourceScrumTool.Extensions;
using OpenSourceScrumTool.Models;
using OpenSourceScrumTool.Models.DataModels;
using OpenSourceScrumTool.Models.ViewModels;

namespace OpenSourceScrumTool.Controllers
{
    [Authorize]
    public class SprintController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public SprintController()
        {
            _db = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_db));
        }

        private List<SprintSettingsTableItemViewModel> GetTableItemsList(Project project)
        {
            return project.Sprints.Select(s => new SprintSettingsTableItemViewModel()
            {
                Id = s.Id,
                Iteration = s.Iteration,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                FeaturesCount = s.Features.Count
            }).ToList();
        }

        // GET: Sprint
        public async Task<ActionResult> Index(/* int? id */)
        {
            //Find user
            var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (currentUser == null)
                return RedirectToAction("LogOff", "Account"); //If the user is here without a found user then it must be an old cookie

            

            //Generate model
            var model = new SprintIndexViewModel()
            {
                UserName = currentUser.Fullname ?? currentUser.Email,
                Projects = new List<ProjectSprintViewModel>()
            };

            //Hack in this sprint...
            //if (id != null)
            //{
            //    //Find current sprint
            //    var thisSprint = await _db.Sprints.FindAsync(id);

            //    var thisProjSprint = new ProjectSprintViewModel()
            //    {
            //        ProjectId = thisSprint.Project_Id,
            //        ProjectName = thisSprint.Project.Title,
            //        TeamName = thisSprint.Project.Teams.First().Team.TeamName,
            //        SprintItteration = thisSprint.Iteration,
            //        SprintStartDate = thisSprint.StartDate,
            //        SprintEndDate = thisSprint.EndDate,
            //        Features = new List<SprintFeatures>()
            //    };

            //    //Add features to view model
            //    foreach (var feat in thisSprint.Features.OrderBy(f => f.Priority))
            //    {
            //        thisProjSprint.Features.Add(new SprintFeatures()
            //        {
            //            FeatureId = feat.Id,
            //            FeatureName = feat.Title,
            //            FeatureDescription = feat.Description,
            //            TasksNotStarted = feat.Tasks.Where(t => t.State == EnumTaskState.NotStarted).OrderBy(p => p.Priority).Select(t => new TaskItemViewModel(t)).ToList(),
            //            TasksInProgress = feat.Tasks.Where(t => t.State == EnumTaskState.InProgress).OrderBy(p => p.Priority).Select(t => new TaskItemViewModel(t)).ToList(),
            //            TasksDone = feat.Tasks.Where(t => t.State == EnumTaskState.Done).OrderBy(p => p.Priority).Select(t => new TaskItemViewModel(t)).ToList()
            //        });
            //    }
            //    model.Projects.Add(thisProjSprint);
            //    return View(model);
            //}

            //Add projects
            var teams = currentUser.Teams.Select(member => member.Team);
            foreach (var team in teams)
            {
                foreach (var proj in team.Projects)
                {
                    //Check we haven't already added this project
                    if(model.Projects.Any(i => i.ProjectId == proj.ProjectId))
                        continue;

                    //Find current sprint
                    var thisSprint = proj.Project.GetThisSprint();

                    //Skip project if we can't find the current sprint
                    if(thisSprint == null)
                        continue;

                    var thisProjSprint = new ProjectSprintViewModel()
                    {
                        ProjectId = proj.ProjectId,
                        ProjectName = proj.Project.Title,
                        TeamName = team.TeamName,
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
                            TasksNotStarted = feat.Tasks.Where(t => t.State == EnumTaskState.NotStarted).OrderBy(p => p.Priority).Select(t => new TaskItemViewModel(t)).ToList(),
                            TasksInProgress = feat.Tasks.Where(t => t.State == EnumTaskState.InProgress).OrderBy(p => p.Priority).Select(t => new TaskItemViewModel(t)).ToList(),
                            TasksDone = feat.Tasks.Where(t => t.State == EnumTaskState.Done).OrderBy(p => p.Priority).Select(t => new TaskItemViewModel(t)).ToList()
                        });
                    }

                    model.Projects.Add(thisProjSprint);
                }
            }

            return View(model);
        }

        // GET: Sprint/SprintSettings/5
        public async Task<ActionResult> SprintSettings(int? id)
        {
            //Ensure valid input
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //Find project in database
            var project = await _db.Projects.FindAsync(id);
            if (project == null)
                return HttpNotFound("Project not found.");

            //TODO: Ensure user is allowed access to this project

            //Create and return model
            var model = new SprintSettingsViewModel()
            {
                Id = project.Id,
                FormViewModel = new SprintSettingsFormViewModel()
                {
                    Id = project.Id,
                    SprintDuration = project.SprintDuration
                },
                TableViewModel = new SprintSettingsTableViewModel()
                {
                    Id = project.Id,
                    Sprints = GetTableItemsList(project)
                }
            };
            return PartialView("_SprintSettings", model);
        }

        // POST: Sprint/SprintSettings/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SprintSettings(SprintSettingsFormViewModel model)
        {
            //Find project in database
            var project = await _db.Projects.FindAsync(model.Id);
            if (project == null)
                return HttpNotFound("Project not found.");

            //TODO: Ensure user is allowed access to this project

            //Ensure model is valid
            if (!ModelState.IsValid)
            {
                var sprintSettingsViewModel = new SprintSettingsViewModel()
                {
                    Id = project.Id,
                    FormViewModel = new SprintSettingsFormViewModel()
                    {
                        Id = project.Id,
                        SprintDuration = project.SprintDuration
                    },
                    TableViewModel = new SprintSettingsTableViewModel()
                    {
                        Id = project.Id,
                        Sprints = GetTableItemsList(project)
                    }
                };
                return PartialView("_SprintSettings", sprintSettingsViewModel);
            }

            //Update entity
            project.SprintDuration = model.SprintDuration;

            //Update future sprints with new schedule
            var futureSprints = project.Sprints.Where(sprint => sprint.StartDate > DateTime.Now);
            var lastSprint = project.GetThisSprint();
            foreach (var futureSprint in futureSprints)
            {
                futureSprint.StartDate = lastSprint.EndDate;
                futureSprint.EndDate = futureSprint.StartDate.AddDays(7 * model.SprintDuration);
                lastSprint = futureSprint;
            }

            try
            {
                //Save all changes made
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                ViewBag.Error = exception.Message;
            }
            return await SprintSettings(model.Id);
        }

        // POST: Sprint/CreateSprint/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateSprint(int? id)
        {
            //Ensure valid input
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Project Id not provided.");

            //Find project
            var project = await _db.Projects.FindAsync(id);
            if (project == null)
                return HttpNotFound("Project not found.");

            //TODO: Ensure user is allowed access to this project

            //Calculate end date
            var startDate = DateTime.Now;
            if (project.Sprints.Any())
                startDate = project.Sprints.OrderByDescending(sprint => sprint.EndDate).First().EndDate;

            //Calculate the iteration number
            var iteration = 1;
            if (project.Sprints.Any())
                iteration = (project.Sprints.OrderByDescending(sprint => sprint.Iteration).First().Iteration) + 1;

            //Add the new sprint and return result
            try
            {
                project.Sprints.Add(new Sprint()
                {
                    StartDate = startDate,
                    EndDate = startDate.AddDays(project.SprintDuration * 7),
                    Iteration = iteration
                });

                await _db.SaveChangesAsync();
                return new HttpStatusCodeResult(HttpStatusCode.Accepted, "Sprint Added");
            }
            catch (Exception exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, exception.Message);
            }
        }

        // GET: Sprint/EditSprint/5
        public async Task<ActionResult> EditSprint(int? id)
        {
            //Ensure valid input
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //Find the sprint in the database
            var sprint = await _db.Sprints.FindAsync(id);
            if (sprint == null)
                return HttpNotFound("Sprint not found.");

            //TODO: Ensure user is allowed access to this project

            //Calculate the earliest start date for this sprint
            var prevSprints = sprint.Project.Sprints.Where(sprint1 => sprint1.Iteration == sprint.Iteration - 1);
            var prevSprintsArray = prevSprints as Sprint[] ?? prevSprints.ToArray();
            ViewBag.notBefore = prevSprintsArray.Any() ? prevSprintsArray.First().EndDate.ToString("MM-dd-yyyy") : null;

            //Create view model and return
            var model = new SprintEditViewModel()
            {
                Id = sprint.Id,
                SprintDuration = sprint.Project.SprintDuration,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate
            };
            return PartialView("_SprintEdit", model);
        }

        // POST: Sprint/EditSprint/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSprint(SprintEditViewModel model)
        {
            //Ensure model is valid
            if (!ModelState.IsValid)
                return PartialView("_SprintEdit", model);

            //Find database sprint
            var sprint = await _db.Sprints.FindAsync(model.Id);
            if (sprint == null)
                return HttpNotFound("Sprint not found.");

            //TODO: Ensure user is allowed access to this project

            //Validate sprint dates
            if (model.EndDate < DateTime.Now)
            {
                model.Error = "You cannot set the end date to the past.";
                return PartialView("_SprintEdit", model);
            }

            //Attempt to store the edit
            try
            {
                //Update the database
                sprint.StartDate = model.StartDate;
                sprint.EndDate = model.EndDate;
                await _db.SaveChangesAsync();

                //Update future sprints with new schedule
                var futureSprints = sprint.Project.Sprints.Where(s => s.Iteration > sprint.Iteration);
                var lastSprint = sprint;
                foreach (var futureSprint in futureSprints)
                {
                    futureSprint.StartDate = lastSprint.EndDate;
                    futureSprint.EndDate = futureSprint.StartDate.AddDays(7 * sprint.Project.SprintDuration);
                    lastSprint = futureSprint;
                }
                await _db.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                //Return error
                model.Error = exception.Message;
                return PartialView("_SprintEdit", model);
            }

            //Create view model and return
            var sprintSettingViewModel = new SprintSettingsViewModel()
            {
                Id = sprint.Project.Id,
                Info = "Sprint Updated",
                FormViewModel = new SprintSettingsFormViewModel()
                {
                    Id = sprint.Project.Id,
                    SprintDuration = sprint.Project.SprintDuration
                },
                TableViewModel = new SprintSettingsTableViewModel()
                {
                    Id = sprint.Project.Id,
                    Sprints = GetTableItemsList(sprint.Project)
                }
            };
            return PartialView("_SprintSettings", sprintSettingViewModel);
        }

        // GET: Sprint/SprintsListArea/5
        public async Task<ActionResult> SprintsListArea(int? id)
        {
            //Ensure valid input
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //Find project
            var project = await _db.Projects.FindAsync(id);
            if (project == null)
                return HttpNotFound("Project not found.");

            //TODO: Ensure user is allowed access to this project

            //Create and return model
            var model = new SprintSettingsTableViewModel()
            {
                Id = project.Id,
                Sprints = GetTableItemsList(project)
            };
            return PartialView("_SprintSettingsTable", model);
        }
    }
}