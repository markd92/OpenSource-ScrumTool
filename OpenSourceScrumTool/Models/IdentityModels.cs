using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;
using System.Threading;
using OpenSourceScrumTool.Hubs;
using OpenSourceScrumTool.Models.DataModels;

using projectList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.DataModels.Project>>;
using featureList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.DataModels.Feature>>;
using taskList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.DataModels.ScrumTask>>;
using sprintList = System.Collections.Generic.List<OpenSourceScrumTool.Models.DataModels.Sprint>;
using userList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.ApplicationUser>>;
using colleaguesList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.DataModels.UserColleagues>>;
using teamList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.DataModels.Team>>;
using teamMemberList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.DataModels.TeamMember>>;
using teamProjectList = System.Collections.Generic.List<System.Tuple<System.Data.Entity.EntityState, OpenSourceScrumTool.Models.DataModels.TeamProject>>;

namespace OpenSourceScrumTool.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public string Fullname { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<UserColleagues> Colleagues { get; set; }

        [InverseProperty("Colleague")]
        public virtual ICollection<UserColleagues> ColleaguesToMe { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<TeamMember> Teams { get; set; }

        [InverseProperty("Owner")]
        public virtual ICollection<Project> Projects { get; set; }

        [DefaultValue(EnumTaskView.List)]
        public EnumTaskView PreferedView { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("ScrumEntities", throwIfV1Schema: false)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Call base as we need IdentityDbContext to build it's relationships
            base.OnModelCreating(modelBuilder);
            //configure model with fluent API 
            //We don't need to use fluent API as we now use InverseProperties
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public override int SaveChanges()
        {
            var changes = DetectLiveChanges();
            var task = base.SaveChanges();
            PushLiveChages(changes);
            return task;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            var changes = DetectLiveChanges();
            var task = base.SaveChangesAsync(cancellationToken);
            task.ContinueWith(delegate { PushLiveChages(changes); }, cancellationToken);
            return task;
        }

        public override Task<int> SaveChangesAsync()
        {
            var changes = DetectLiveChanges();
            var task = base.SaveChangesAsync();
            task.ContinueWith(delegate { PushLiveChages(changes); });
            return task;
        }

        private ServiceContainer DetectLiveChanges()
        {
            //Find changes made
            ChangeTracker.DetectChanges();
            projectList changedProjects =
                ChangeTracker.Entries<Project>()
                    .Where(i => i.State != EntityState.Unchanged)
                    .Select(i => new Tuple<EntityState, Project>(i.State, i.Entity))
                    .ToList();
            featureList changedFeatures =
                ChangeTracker.Entries<Feature>()
                    .Where(i => i.State != EntityState.Unchanged)
                    .Select(i => new Tuple<EntityState, Feature>(i.State, i.Entity))
                    .ToList();
            taskList changedTasks =
                ChangeTracker.Entries<ScrumTask>()
                    .Where(i => i.State != EntityState.Unchanged)
                    .Select(i => new Tuple<EntityState, ScrumTask>(i.State, i.Entity))
                    .ToList();
            sprintList changedSprints =
                ChangeTracker.Entries<Sprint>()
                    .Where(i => i.State != EntityState.Unchanged && i.State != EntityState.Deleted)
                    .Select(i => i.Entity)
                    .ToList();
            userList changedUsers =
                ChangeTracker.Entries<ApplicationUser>()
                    .Where(i => i.State != EntityState.Unchanged)
                    .Select(i => new Tuple<EntityState, ApplicationUser>(i.State, i.Entity))
                    .ToList();
            colleaguesList changedColleagues =
                ChangeTracker.Entries<UserColleagues>()
                    .Where(i => i.State != EntityState.Unchanged)
                    .Select(i => new Tuple<EntityState, UserColleagues>(i.State, i.Entity))
                    .ToList();
            teamList changedTeams =
                ChangeTracker.Entries<Team>()
                    .Where(i => i.State != EntityState.Unchanged)
                    .Select(i => new Tuple<EntityState, Team>(i.State, i.Entity))
                    .ToList();
            teamMemberList changedTeamMembers =
                ChangeTracker.Entries<TeamMember>()
                    .Where(i => i.State != EntityState.Unchanged)
                    .Select(i => new Tuple<EntityState, TeamMember>(i.State, i.Entity))
                    .ToList();
            teamProjectList changedTeamProjects =
                ChangeTracker.Entries<TeamProject>()
                    .Where(i => i.State != EntityState.Unchanged)
                    .Select(i => new Tuple<EntityState, TeamProject>(i.State, i.Entity))
                    .ToList();


            //Propagate task and feature changes up to projects as these could affect the Feature/Project status and counts
            changedFeatures.AddRange(changedTasks.Select(i => new Tuple<EntityState, Feature>(EntityState.Modified, this.Features.Find(i.Item2.FeatureId))).Distinct());
            changedProjects.AddRange(changedFeatures.Select(i => new Tuple<EntityState, Project>(EntityState.Modified, this.Projects.Find(i.Item2.ProjectId))).Distinct());

            //Propagate changed team users to teams objects
            changedTeams.AddRange(changedTeamMembers.Select(i => new Tuple<EntityState, Team>(EntityState.Modified, this.Teams.Find(i.Item2.Team_Id))).Distinct());

            //Propagate changed team projects to teams objects
            changedTeams.AddRange(changedTeamProjects.Select(i => new Tuple<EntityState, Team>(EntityState.Modified, this.Teams.Find(i.Item2.TeamId))).Distinct());

            //Collect results
            var changesContainer = new ServiceContainer();
            changesContainer.AddService(typeof(projectList), changedProjects);
            changesContainer.AddService(typeof(featureList), changedFeatures);
            changesContainer.AddService(typeof(taskList), changedTasks);
            changesContainer.AddService(typeof(sprintList), changedSprints);
            changesContainer.AddService(typeof(userList), changedUsers);
            changesContainer.AddService(typeof(colleaguesList), changedColleagues);
            changesContainer.AddService(typeof(teamList), changedTeams);
            changesContainer.AddService(typeof(teamMemberList), changedTeamMembers); //Not used at present
            return changesContainer;
        }



        private void PushLiveChages(ServiceContainer changesContainer)
        {
            //Forward events on
            if (changesContainer.GetService(typeof(projectList)) != null)
                Task.Factory.StartNew(() => { LiveUpdateSingleton.Instance.EntityUpdated((projectList)changesContainer.GetService(typeof(projectList))); });

            if (changesContainer.GetService(typeof(featureList)) != null)
                Task.Factory.StartNew(() => { LiveUpdateSingleton.Instance.EntityUpdated((featureList)changesContainer.GetService(typeof(featureList))); });

            if (changesContainer.GetService(typeof(taskList)) != null)
                Task.Factory.StartNew(() => { LiveUpdateSingleton.Instance.EntityUpdated((taskList)changesContainer.GetService(typeof(taskList))); });

            if (changesContainer.GetService(typeof(sprintList)) != null)
                Task.Factory.StartNew(() => { LiveUpdateSingleton.Instance.EntityUpdated((sprintList)changesContainer.GetService(typeof(sprintList))); });

            if (changesContainer.GetService(typeof(userList)) != null)
                Task.Factory.StartNew(() => { LiveUpdateSingleton.Instance.EntityUpdated((userList)changesContainer.GetService(typeof(userList))); });

            if (changesContainer.GetService(typeof(colleaguesList)) != null)
                Task.Factory.StartNew(() => { LiveUpdateSingleton.Instance.EntityUpdated((colleaguesList)changesContainer.GetService(typeof(colleaguesList))); });

            if (changesContainer.GetService(typeof(teamList)) != null)
                Task.Factory.StartNew(() => { LiveUpdateSingleton.Instance.EntityUpdated((teamList)changesContainer.GetService(typeof(teamList))); });
        }

        public DbSet<Feature> Features { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ScrumTask> Tasks { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamsMembers { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<UserColleagues> Colleagues { get; set; }
        public DbSet<TeamProject> TeamProjects { get; set; }
    }
}