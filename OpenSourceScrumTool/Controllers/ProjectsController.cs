using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OpenSourceScrumTool.Extensions;
using OpenSourceScrumTool.Hubs;
using OpenSourceScrumTool.Models;
using OpenSourceScrumTool.Models.DataModels;
using OpenSourceScrumTool.Models.ViewModels;

namespace OpenSourceScrumTool.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController()
        {
            _db = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_db));
        }

        // GET: Projects
        public async Task<ActionResult> Index()
        {
            var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (currentUser == null)
                return RedirectToAction("LogOff", "Account"); //If the user is here without a found user then it must be an old cookie

            //Get accessible projects
            var myProjects = currentUser.Projects.ToList();
            var teamProjects = currentUser.Teams.Select(member => member.Team.Projects).SelectMany(collection => collection).Select(project => project.Project).ToList();

            //Combine the two lists
            var combined = myProjects.Concat(teamProjects).Distinct();

            //Create model from projects
            var model = new ProjectIndexViewModel(combined.ToList());

            return View(model);
        }

        public ActionResult Create()
        {
            var model = new ProjectCreateViewModel()
            {
                SprintDuration = Project.DefaultSprintDuration
            };
            return PartialView("_Create", model);
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProjectCreateViewModel model)
        {
            if (!ModelState.IsValid) 
                return PartialView("_Create", model);

            try
            {
                //Create project
                var project = new Project()
                {
                    Title = model.Title,
                    Description = model.Description,
                    SprintDuration = model.SprintDuration
                };
                _db.Projects.Add(project);

                //Find and associate with user
                var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
                currentUser.Projects.Add(project);
                await _db.SaveChangesAsync();

                //Add initial sprint
                project.Sprints.Add(new Sprint()
                {
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(project.SprintDuration * 7),
                    Iteration = 1
                });
                await _db.SaveChangesAsync();

                //Return success message
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                model.Error = exception.Message;
                return PartialView("_Create", model);
            }
        }


        // GET: Projects/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Project project = await _db.Projects.FindAsync(id);
            if (project == null)
                return HttpNotFound("Project not found.");

            //TODO: Ensure user is allowed access to this project

            var model = new ProjectEditViewModel()
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                SprintDuration = project.SprintDuration
            };

            return PartialView("_Edit", model);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ProjectEditViewModel model)
        {
            if (!ModelState.IsValid) return PartialView("_Edit", model);

            var project = _db.Projects.Find(model.Id);
            if (project == null)
                return HttpNotFound("Project not found.");

            //TODO: Ensure user is allowed access to this project

            try
            {
                project.SprintDuration = model.SprintDuration;
                project.Title = model.Title;
                project.Description = model.Description;
                await _db.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                model.Error = exception.Message;
                return PartialView("_Edit", model);
            }

            return Json(new { success = true });
        }

        // GET: Projects/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid id");

            Project project = await _db.Projects.FindAsync(id);
            if (project == null)
                return HttpNotFound("Project not found.");

            //Ensure user is allowed access to this project
            var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (currentUser == null)
                return RedirectToAction("LogOff", "Account"); //If the user is here without a found user then it must be an old cookie

            //Check if user owns this project
            if (!currentUser.Projects.Contains(project))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest,
                    "You do not have permission to delete this project");

            var model = new ProjectDeleteViewModel()
            {
                Title = project.Title,
                Description = project.Description
            };

            return PartialView("_Delete", model);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Project project = await _db.Projects.FindAsync(id);
            if (project == null)
                return HttpNotFound("Project not found.");

            //Ensure user is allowed access to this project
            var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (currentUser == null)
                return RedirectToAction("LogOff", "Account"); //If the user is here without a found user then it must be an old cookie

            //Check if user owns this project
            if (!currentUser.Projects.Contains(project))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest,
                    "You do not have permission to delete this project");

            try
            {
                _db.Sprints.RemoveRange(project.Sprints);
                _db.Projects.Remove(project);
                
                await _db.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                var model = new ProjectDeleteViewModel()
                {
                    Title = project.Title,
                    Description = project.Description,
                    Error = exception.Message
                };
                return PartialView("_Delete", model);
            }

            return Json(new { success = true });
        }

        [ChildActionOnly]
        public ActionResult DetailsPartial(int projectId)
        {
            //Ensure project exists
            var project = _db.Projects.Find(projectId);
            if (project == null)
                return HttpNotFound("Feature not found.");

            //TODO: Ensure user is allowed access to this project

            //Create view model
            var model = new ProjectDetailsViewModel(project);
            return View("_ProjectDetails", model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
