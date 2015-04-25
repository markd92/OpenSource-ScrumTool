using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OpenSourceScrumTool.Extensions;
using OpenSourceScrumTool.Models;
using OpenSourceScrumTool.Models.DataModels;
using OpenSourceScrumTool.Models.ViewModels;

namespace OpenSourceScrumTool.Controllers
{
    [Authorize]
    public class FeaturesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeaturesController()
        {
            _db = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_db));
        }

        // GET: Feature
        public async Task<ActionResult> Index(int? id)
        {
            //Check if valid input
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No Project Id presented.");

            //Find project from database
            var project = await _db.Projects.FindAsync(id);
            if (project == null)
                return HttpNotFound("Project not found.");

            //TODO: Ensure user is allowed access to this project

            //Assemble view model
            var model = new FeatureIndexViewModel(project);
            return View(model);
        }

        // GET: Feature/Create
        public ActionResult Create(int? id)
        {
            //Check request was valid
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No Project Id presented.");

            //Ensure project exists
            var project = _db.Projects.Find(id);
            if (project == null)
                return HttpNotFound("Project not found.");

            //TODO: Ensure user is allowed access to this project

            //Create view model
            var model = new FeatureCreateViewModel()
            {
                ProjectId = project.Id
            };
            return View("_Create", model);
        }

        // POST: Feature/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(FeatureCreateViewModel model)
        {
            //Check if model is valid
            if (!ModelState.IsValid)
                return PartialView("_Create", model);

            //Find project in database
            var project = _db.Projects.Find(model.ProjectId);
            if (project == null)
                return HttpNotFound("Project not found");

            //TODO: Ensure user is allowed access to this project

            //Create feature from model
            var feature = new Feature()
            {
                Project = project,
                Title = model.Title,
                Description = model.Description,
                Weight = model.Weight,
                Priority = project.Features.Any() ? project.Features.OrderByDescending(p => p.Priority).First().Priority + 1 : 1,
                DateCreated = DateTime.Now
            };

            //Save feature in database
            try
            {
                _db.Features.Add(feature);
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                model.Error = exception.Message;
                return PartialView("_Create", model);
            }
        }

        // GET: Feature/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            //Ensure valid input
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //Find feature in database
            var feature = await _db.Features.FindAsync(id);
            if (feature == null)
                return HttpNotFound("Feature not found.");

            //TODO: Ensure user is allowed access to this project

            //Create and return base model
            var model = new FeatureEditViewModel()
            {
                Id = feature.Id,
                Description = feature.Description,
                Title = feature.Title,
                Weight = feature.Weight
            };
            return PartialView("_Edit", model);
        }

        // POST: Feature/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(FeatureEditViewModel model)
        {
            //Ensure model is valid
            if (!ModelState.IsValid)
                return PartialView("_Edit", model);
            
            //Find feature in database
            var feature = await _db.Features.FindAsync(model.Id);
            if (feature == null)
                return HttpNotFound("Feature not found.");

            //TODO: Ensure user is allowed access to this project

            try
            {
                //Update database entry
                feature.Title = model.Title;
                feature.Description = model.Description;
                feature.Weight = model.Weight;

                //Store changes
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                model.Error = exception.Message;
                return PartialView("_Edit", model);
            }
        }

        // GET: Feature/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            //Ensure valid input
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            //Find feature in database
            var feature = await _db.Features.FindAsync(id);
            if (feature == null)
                return HttpNotFound("Feature not found.");

            //TODO: Ensure user is allowed access to this project

            //Create and return model
            var model = new FeatureDeleteViewModel()
            {
                Title = feature.Title,
                Description = feature.Description,
                DateCreated = feature.DateCreated,
                Weight = feature.Weight
            };
            return PartialView("_Delete", model);
        }

        // POST: Feature/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            //Find the project in the database
            var feature = await _db.Features.FindAsync(id);
            if (feature == null)
                return HttpNotFound("Feature not found.");

            //TODO: Ensure user is allowed access to this project

            //Attempt to delete feature from database
            try
            {
                _db.Features.Remove(feature);
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                //If fails, return model containing the error
                var model = new FeatureDeleteViewModel()
                {
                    Title = feature.Title,
                    Description = feature.Description,
                    DateCreated = feature.DateCreated,
                    Weight = feature.Weight,
                    Error = exception.Message
                };
                return PartialView("_Delete", model);
            }
        }

        public ActionResult AddToSprint(int sprintId, int featureId)
        {
            //Find feature in database
            var feature = _db.Features.Find(featureId);
            if (feature == null)
                return HttpNotFound("Feature not found.");

            //TODO: Ensure user is allowed access to this project

            //Find the sprint in the database
            var sprint = _db.Sprints.Find(sprintId);
            if (sprint == null)
                return HttpNotFound("Sprint not found.");

            //Add the feature to the sprint
            sprint.Features.Add(feature);
            _db.SaveChanges();
            return RedirectToAction("Index", new { id = feature.ProjectId });
        }

        public ActionResult RemoveFromSprint(int sprintId, int featureId)
        {
            //Find the feature in database
            var feature = _db.Features.Find(featureId);
            if (feature == null)
                return HttpNotFound("Feature not found.");

            //TODO: Ensure user is allowed access to this project

            //Find the sprint in the database
            var sprint = _db.Sprints.Find(sprintId);
            if (sprint == null)
                return HttpNotFound("Sprint not found.");

            //Double check the sprint actually has the feature in it
            if (!sprint.Features.Contains(feature))
                Debug.WriteLine("RemoveFromSprint -> sprint does not contain this feature.");

            //Remove it the feature from the sprint, and save
            sprint.Features.Remove(feature);
            _db.SaveChanges();

            //Redirect the user to the index method
            return RedirectToAction("Index", new { id = feature.ProjectId, sprintId = sprintId });
        }

        [ChildActionOnly]
        public ActionResult DetailsPartial(int featureId)
        {
            //Ensure feature exists
            var feature = _db.Features.Find(featureId);
            if (feature == null)
                return HttpNotFound("Feature not found.");

            //TODO: Ensure user is allowed access to this project

            //Create view model
            var model = new FeatureDetailsViewModel(feature);
            return View("_FeatureDetails", model);
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
