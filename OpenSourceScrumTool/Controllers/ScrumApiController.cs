using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using OpenSourceScrumTool.Models;
using OpenSourceScrumTool.Models.DataModels;

namespace OpenSourceScrumTool.Controllers
{
    [Authorize]
    public class ScrumApiController : ApiController
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ScrumApiController()
        {
            _db = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_db));
        }

        [HttpPost]
        public async Task<bool> SaveProjectOrder(int[] projectIds)
        {
            if (projectIds == null || projectIds.Length <= 0) return false;

            var user = _userManager.FindById(User.Identity.GetUserId());

            for (var i = 0; i < projectIds.Length; i++)
            {
                //TODO: Store order somewhere
            }
            await _db.SaveChangesAsync();
            return true;
        }

        [HttpPost]
        public async Task<bool> SaveFeatureOrder(int[] featureIds)
        {
            if (featureIds == null || featureIds.Length <= 0) return false;

            for (var i = 0; i < featureIds.Length; i++)
            {
                var feature = _db.Features.Find(featureIds[i]);
                if (feature != null)
                    feature.Priority = i;
            }
            await _db.SaveChangesAsync();
            return true;
        }

        [HttpPost]
        public async Task<bool> SaveTaskOrder(int[] taskIds)
        {
            if (taskIds == null || taskIds.Length <= 0) return false;

            for (var i = 0; i < taskIds.Length; i++)
            {
                var task = _db.Tasks.Find(taskIds[i]);
                if (task != null)
                    task.Priority = i;
            }
            await _db.SaveChangesAsync();
            return true;
        }

        [HttpPost]
        public async Task<bool> SaveTaskOrderState(ScrumTask[] scrumTasks)
        {
            if (scrumTasks == null || scrumTasks.Length <= 0) return false;

            foreach (var t in scrumTasks)
            {
                var task = _db.Tasks.Find(t.Id);
                if (task == null) continue;

                task.State = t.State;
                if (task.State == EnumTaskState.Done)
                    task.TimeRemaining = 0;
                task.Priority = t.Priority;
            }
            await _db.SaveChangesAsync();
            return true;
        }

        [HttpPost]
        [ResponseType(typeof(SprintSetupResponse))]
        public async Task<HttpResponseMessage> SetupSprint(SprintSetupData data)
        {
            var project = await _db.Projects.FindAsync(data.projectId);
            if (project == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Cannot find a project with this Id");

            //Next sprint if not this sprint
            Sprint sprint = null;
            if (project.Sprints.Any())
            {
                if (data.thisSprint)
                    sprint = project.GetThisSprint();
                else
                {
                    sprint = project.Sprints.OrderByDescending(s => s.StartDate).FirstOrDefault(s => s.StartDate > DateTime.Now);
                }
            }

            //If sprint doesn't exist, create one
            if (sprint == null)
            {
                //Calculate end date
                var startDate = DateTime.Now;
                if (project.Sprints.Any())
                    startDate = project.Sprints.OrderByDescending(s => s.EndDate).First().EndDate;

                //Calculate the iteration number
                var iteration = 1;
                if (project.Sprints.Any())
                    iteration = (project.Sprints.OrderByDescending(s => s.Iteration).First().Iteration) + 1;

                //Add the new sprint and return result
                try
                {
                    sprint = new Sprint()
                    {
                        StartDate = startDate,
                        EndDate = startDate.AddDays(project.SprintDuration * 7),
                        Iteration = iteration
                    };
                    project.Sprints.Add(sprint);

                    await _db.SaveChangesAsync();
                }
                catch (Exception exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, exception.Message);
                }
            }

            //Check if we are allowed to overwrite this sprint
            if (!data.forceNextSpring && sprint.Features.Any())
                return Request.CreateResponse(HttpStatusCode.OK, new SprintSetupResponse() { NextSprintAlreadySet = true });

            //If there are features in the sprint, clear them
            sprint.Features.Clear();

            //Save the data
            try
            {
                //Add features to sprint
                var features = _db.Features.Where(f => data.featureIds.Contains(f.Id));
                sprint.Features = features.ToList();
                await _db.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, exception.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new SprintSetupResponse(){ SprintSet = true });
        }
    }

    public class SprintSetupData
    {
        public int projectId { get; set; }
        public int[] featureIds { get; set; }
        public bool thisSprint { get; set; }
        public bool forceNextSpring { get; set; }
    }

    public class SprintSetupResponse
    {
        public bool SprintSet { get; set; }
        public bool NextSprintAlreadySet { get; set; }
    }
}
