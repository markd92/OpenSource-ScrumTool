using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OpenSourceScrumTool.Models;
using OpenSourceScrumTool.Models.ViewModels;

namespace OpenSourceScrumTool.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Manage projects using SCRUM!";

            return View();
        }

        public ActionResult KnownIssues()
        {
            return View(new HomeKnownIssuesViewModel() { Issues = Docs.KnownIssues });
        }

        public ActionResult ChangeLog()
        {
            return View(new HomeChangeLogViewModels() { Changes = Docs.Changelogs });
        }
    }
}