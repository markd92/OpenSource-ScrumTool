using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
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
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController()
        {
            _db = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_db));
        }

        // GET: Tasks
        public async Task<ActionResult> Index(int? id, EnumTaskView? view)
        {
            if (view != null)
            {
                //Store preferred view style as list
                UpdateUserPreferredView((EnumTaskView) view);
            }
            else
            {
                view = _userManager.FindById(User.Identity.GetUserId()).PreferedView;
            }

            //Check valid input
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No Feature Id presented.");

            //Find feature in database
            var feature = await _db.Features.FindAsync(id);
            if (feature == null)
                return HttpNotFound("Feature not found.");

            //TODO: Ensure user is allowed access to this project

            //Create and return view model
            var model = new TaskIndexViewModel(feature);

            switch (view)
            {
                case EnumTaskView.Board:
                    return View("Board", model);
                case EnumTaskView.List:
                default:
                    return View("Index", model);
            }
        }

        // GET: Tasks/Create
        public ActionResult Create(int? id)
        {
            //Ensure valid input
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No Feature Id presented.");

            //Find feature in database
            var feature = _db.Features.Find(id);
            if (feature == null)
                return HttpNotFound("Feature not found.");

            //TODO: Ensure user is allowed access to this project

            //Create view model and return
            var model = new TaskCreateViewModel()
            {
                FeatureId = feature.Id
            };
            return View("_Create", model);
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TaskCreateViewModel model)
        {
            //Ensure valid input
            if (!ModelState.IsValid) 
                return View("_Create", model);

            //Find feature
            var feature = _db.Features.Find(model.FeatureId);
            if (feature == null)
                return HttpNotFound("Feature not found.");

            //TODO: Ensure user is allowed access to this project
            
            //Create new task
            var task = new ScrumTask()
            {
                Title = model.Title,
                Description = model.Description,
                State = model.State,
                TimeRemaining = model.TimeRemaining,
                Priority = feature.Tasks.Count + 1,
            };

            try
            {
                //Save new feature
                feature.Tasks.Add(task);
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                model.Error = exception.Message;
                return View("_Create", model);
            }
        }

        // GET: Tasks/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            //Validate input
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //Find task in database
            var task = await _db.Tasks.FindAsync(id);
            if (task == null)
                return HttpNotFound("Task not found.");

            //TODO: Ensure user is allowed access to this project

            //Create and return view model
            var model = new TaskEditViewModel()
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                State = task.State,
                TimeRemaining = task.TimeRemaining
            };
            return View("_Edit", model);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(TaskEditViewModel model)
        {
            //Ensure valid input
            if (!ModelState.IsValid) 
                return View("_Edit", model);

            //Find task in database
            var task = await _db.Tasks.FindAsync(model.Id);
            if (task == null)
                return new HttpNotFoundResult("Task not found.");

            //TODO: Ensure user is allowed access to this project

            //Update task
            task.Title = model.Title;
            task.Description = model.Description;
            task.State = model.State;
            task.TimeRemaining = model.TimeRemaining;

            //Save changes
            try
            {
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                model.Error = exception.Message;
                return View("_Edit", model);
            }
        }

        // GET: Tasks/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            //Ensure valid input
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //Find task in database
            var task = await _db.Tasks.FindAsync(id);
            if (task == null)
                return HttpNotFound("Task not found.");

            //TODO: Ensure user is allowed access to this project

            //Create and return view model
            var model = new TaskDeleteViewModel()
            {
                Title = task.Title,
                Description = task.Description,
                State = task.State,
                TimeRemaining = task.TimeRemaining,
                FeatureTitle = task.Feature.Title
            };
            return View("_Delete", model);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            //Find task in database
            var task = await _db.Tasks.FindAsync(id);
            if (task == null)
                return HttpNotFound("Task not found.");

            //TODO: Ensure user is allowed access to this project

            //Remove task from database
            try
            {
                _db.Tasks.Remove(task);
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                //Create and return view model
                var model = new TaskDeleteViewModel()
                {
                    Title = task.Title,
                    Description = task.Description,
                    State = task.State,
                    TimeRemaining = task.TimeRemaining,
                    FeatureTitle = task.Feature.Title,
                    Error = exception.Message
                };
                return View("_Delete", model);
            }
        }

        /// <summary>
        /// Updates the user's preferred view mode
        /// </summary>
        /// <param name="preferredView">The preferred view.</param>
        private void UpdateUserPreferredView(EnumTaskView preferredView)
        {
            //Find user
            var currentUser = _userManager.FindById(User.Identity.GetUserId());
            //Change their view mode
            currentUser.PreferedView = preferredView;            
            //Store changes
            _userManager.Update(currentUser);
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
