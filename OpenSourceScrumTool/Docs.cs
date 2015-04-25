using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using OpenSourceScrumTool.Models;
using OpenSourceScrumTool.Models.DataModels;

namespace OpenSourceScrumTool
{
    public class Docs
    {
        public static List<ChangeLog> Changelogs = new List<ChangeLog>()
        {
            new ChangeLog()
            {
                Version = 0.01,
                MajorFeature = "Created project with basic management features.",
                Changes = new List<string>()
                {
                    "Create and edit Projects",
                    "Create and edit Features",
                    "Create and edit Tasks",
                    "Drag re-order Projects, Features & Tasks"
                }
            },
            new ChangeLog()
            {
                Version = 0.02,
                MajorFeature = "User logins!",
                Changes = new List<string>()
                {
                    "Added ASP.NET Identities",
                    "Associated users with projects",
                    "Changed projects page to show owned projects",
                    "Added external login providers",
                    "Added Kanban board for tasks management"
                }
            },
            new ChangeLog()
            {
                Version = 0.03,
                MajorFeature = "User Delegation",
                Changes = new List<string>()
                {
                    "Added dialog to delegate access to other users",
                    "Changed projects page to show all accessible projects"
                }
            },
            new ChangeLog()
            {
                Version = 0.04,
                MajorFeature = "SPRINTS!!!",
                Changes = new List<string>()
                {
                    "Added Sprint management",
                    "Added Sprint filters on Features list",
                    "Added Logo",
                    "Fixed enter closing dialog",
                    "Reduced password complexity policy"
                }
            },
            new ChangeLog()
            {
                Version = 0.05,
                MajorFeature = "General improvements",
                Changes = new List<string>()
                {
                    "Displays status on Feature",
                    "Filter Features by Status",
                    "Display info on filter applied",
                    "Added this change log",
                    "Added remember last used task view mode (List/Board)"
                }
            },
            new ChangeLog()
            {
                Version = 0.06,
                MajorFeature = "Permission implementation!",
                Changes = new List<string>()
                {
                    "Added permissions to Projects, Features, Tasks & Sprints Controllers",
                    "Hides buttons for options users don't have permissions for",
                    "Prevented users from using drag items when they don't have permission to save changes",
                    "Changed Project order to be user specific"
                }
            },
            new ChangeLog()
            {
                Version = 0.07,
                MajorFeature = "Mobile & Graphics",
                Changes = new List<string>()
                {
                    "Fixed use on mobile devices",
                    "Improved responsive css",
                    "Sortable works on touch devices",
                    "Menus work on touch devices",
                    "Added progress bar on projects & tasks",
                    "Added colours to easily identify task status'",
                    "Updated login page"
                }
            },
            new ChangeLog()
            {
                Version = 0.08,
                MajorFeature = "Minor Fixes",
                Changes = new List<string>()
                {
                    "Restored Remember me checkbox",
                    "Forced Done tasks to 0 hours remaining",
                    "Fixed issue with viewing Profile details",
                    "Fixed sprint management",
                    "Fixed viewing features sometimes saying access denied"
                }
            },
            new ChangeLog()
            {
                Version = 0.09,
                MajorFeature = "Started Live Updating",
                Changes = new List<string>()
                {
                    "Added Knockout library",
                    "Added SignalR Hub & Connection",
                    "Changed Project Details view to use Knockout MVVM to render",
                    "Added framework for monitoring changes to entity models",
                    "Added push update for Project Details"
                }
            },
            new ChangeLog()
            {
                Version = 0.091,
                MajorFeature = "More Live Updating",
                Changes = new List<string>()
                {
                    "Added Live Update to Feature Details",
                    "Added Live Update to Projects list",
                    "Added tinyMCE"
                }
            },
            new ChangeLog()
            {
                Version = 0.092,
                MajorFeature = "Live Updating on Tasks",
                Changes = new List<string>()
                {
                    "Added Live Update to Features list & board"
                }
            },
            new ChangeLog()
            {
                Version = 0.1,
                MajorFeature = "Live Updating Finished",
                Changes = new List<string>()
                {
                    "Fixed several issues caused by live updating",
                    "Finished check if model has changed when starting to monitor (prevents outdated info unless update)",
                    "Fixed issue with TinyMCE",
                    "Change description areas to render html from TinyMCE",
                    "Fixed disable time remaining when tasks are done",
                    "Fixed task and feature order in live update",
                    "Added SignalR reconnect & disconnect handling"
                }
            },
            new ChangeLog()
            {
                Version = 0.11,
                MajorFeature = "Tiny Tweaks",
                Changes = new List<string>()
                {
                    "Project progress bars are now based on feature Weights"
                }
            },
            new ChangeLog()
            {
                Version = 0.12,
                MajorFeature = "Single user edits",
                Changes = new List<string>()
                {
                    "You will now be warned if someone else has an edit window open"
                }
            },
            new ChangeLog()
            {
                Version = 0.2,
                MajorFeature = "TEAMS!!!",
                Changes = new List<string>()
                {
                    "You now assign known users by the teams page",
                    "Group colleagues using Teams",
                    "Assign Teams to Projects"
                }
            },
            new ChangeLog()
            {
                Version = 0.3,
                MajorFeature = "SPRINTS!!!",
                Changes = new List<string>()
                {
                    "New Sprint page",
                    "-> Get details for work to do this sprint",
                    "Sprint Planning",
                    "-> Easier management of features in sprints",
                    "Features list filters now applied with knockout so happen instantly."
                }
            },
            new ChangeLog()
            {
                Version = 1.0,
                MajorFeature = "First Release",
                Changes = new List<string>()
                {
                    "Added full name to users",
                    "Added full name to Create User page",
                    "Added edit full name in manage user",
                    "Fixed scrum board data passing (html escaping)",
                    "Fixed Add/Delete projects live update in projects list",
                    "Added fallback for project progress bar if features weights set to 0",
                    "Fixed issue when scrum board has multiple projects (update item data failed)",
                    "Added Live locking to Scrum Board"
                }
            }
        };

        public static List<KnownIssue> KnownIssues = new List<KnownIssue>()
        {
            new KnownIssue() { 
                Issue = "No feedback is provided when using the manage sprint dialog."
            },
            new KnownIssue() { 
                Issue = "External logins sometimes don't work."
            }
        };
    }
}