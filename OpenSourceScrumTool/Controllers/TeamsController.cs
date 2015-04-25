using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OpenSourceScrumTool.Models;
using OpenSourceScrumTool.Models.DataModels;
using OpenSourceScrumTool.Models.ViewModels;

namespace OpenSourceScrumTool.Controllers
{
    [Authorize]
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamsController()
        {
            _db = new ApplicationDbContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_db));
        }

        // GET: Teams
        public async Task<ActionResult> Index()
        {
            var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (currentUser == null)
                return RedirectToAction("LogOff", "Account"); //If the user is here without a found user then it must be an old cookie

            var model = new TeamsIndexViewModel(currentUser.Colleagues.Select(colleagues => colleagues.Colleague).ToList(), currentUser.Teams.ToList());

            return View(model);
        }

        // GET: Teams/AddTeam
        public ActionResult AddTeam()
        {
            return PartialView("_AddTeam", new TeamCreateViewModel());
        }

        // POST: Teams/AddTeam
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddTeam(TeamCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_AddTeam", model);

            try
            {
                //Create Team
                var team = new Team()
                {
                    TeamName = model.TeamName.Trim(),
                    Velocity = model.Velocity,
                    Members = new List<TeamMember>()
                    {
                        new TeamMember()
                        {
                            Role = TeamMemberRole.Creator,
                            User = await _userManager.FindByIdAsync(User.Identity.GetUserId())
                        }
                    }
                };
                _db.Teams.Add(team);
                await _db.SaveChangesAsync();

                //Return success message
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                model.Error = exception.Message;
                return PartialView("_AddTeam", model);
            }
        }

        // Get: Teams/EditTeam
        public ActionResult EditTeam(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid Team Id");

            //Find team
            var team = _db.Teams.Find(id);
            if (team == null)
                return new HttpNotFoundResult("Cannot find specified team");

            //Check the user has permission
            var currentUser = _userManager.FindById(User.Identity.GetUserId());
            var teamUser = team.Members.First(m => m.User == currentUser);
            if (teamUser == null || teamUser.Role != TeamMemberRole.Creator)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have permission to perform this action");

            var model = new TeamEditViewModel(team);
            return PartialView("_EditTeam", model);
        }

        // GET: Teams/AddColleague
        public ActionResult AddUser()
        {
            return PartialView("_AddUser", new ColleagueCreateViewModel());
        }

        // POST: Teams/AddColleague
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUser(ColleagueCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_AddUser", model);

            //Try find the user with specified email address
            var colleague = _userManager.FindByEmailAsync(model.Email).Result;

            //If the user cannot be found, display this message
            if (colleague == null)
            {
                model.Error = "Cannot find this user, please ensure they are registered.";
                return PartialView("_AddUser", model);
            }

            //If the user is already a colleague, display this message
            var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (currentUser.Colleagues.Select(colleagues => colleagues.Colleague).Contains(colleague))
            {
                model.Error = "This user is already one of your colleagues.";
                return PartialView("_AddUser", model);
            }

            //If you've entered your own email address
            if (currentUser == colleague)
            {
                model.Error = "There is no need to add yourself...";
                return PartialView("_AddUser", model);
            }

            try
            {
                _db.Colleagues.Add(new UserColleagues()
                {
                    User = currentUser,
                    Colleague = colleague
                });
                await _db.SaveChangesAsync();

                //If add works, return json okay
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                model.Error = exception.Message;
            }

            //If something went wrong, return the model (containing the error)
            return PartialView("_AddUser", model);
        }

        // GET: Teams/RemoveUser
        public async Task<ActionResult> RemoveUser(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
                return new HttpNotFoundResult("Cannot find a user with that Id");

            return PartialView("_RemoveUser", new ColleagueRemoveViewModel(user));
        }

        // POST: Teams/RemoveUser/5
        [HttpPost, ActionName("RemoveUser")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmRemoveUser(string userEmail)
        {
            var colleague = await _userManager.FindByEmailAsync(userEmail);
            if (colleague == null)
                return new HttpNotFoundResult("Cannot find a user with that Id");

            //Create model in case error
            var model = new ColleagueRemoveViewModel(colleague);

            //If the user is already a colleague, return as if success
            var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (!currentUser.Colleagues.Select(colleagues => colleagues.Colleague).Contains(colleague))
            {
                return Json(new { success = true });
            }

            try
            {
                //Remove as colleague
                _db.Colleagues.RemoveRange(currentUser.Colleagues.Where(colleagues => colleagues.Colleague == colleague));

                //Remove from groups
                var myGroups = currentUser.Teams.Where(member => member.Role == TeamMemberRole.Creator).Select(member => member.Team_Id).ToList();
                foreach (var groupId in myGroups)
                {
                    var thisGroup = await _db.Teams.FindAsync(groupId);
                    if (thisGroup.Members.Select(member => member.User).Contains(colleague))
                    {
                        var items = _db.TeamsMembers.Where(member => member.Team_Id == thisGroup.Id && member.User_Id == colleague.Id);
                        _db.TeamsMembers.RemoveRange(items);
                    }
                }
                await _db.SaveChangesAsync();

                //If add works, return json okay
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                model.Error = exception.Message;
            }

            return PartialView("_RemoveUser", model);
        }

        // GET: Teams/AddTeamUser
        public async Task<ActionResult> AddTeamUser(int teamId)
        {
            var team = await _db.Teams.FindAsync(teamId);
            if (team == null)
                return new HttpNotFoundResult("Cannot find a team with that Id");

            //Check if the current user is the team creator
            var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (!currentUser.Teams.Select(member => member.Team).Contains(team) ||
                team.Members.Where(member => member.Role == TeamMemberRole.Creator)
                    .Select(member => member.User)
                    .First() != currentUser)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You cannot manage this team as you are not the creator");
            }

            //Find all colleagues not in this team already
            var colleagues = currentUser.Colleagues.ToList();
            colleagues = colleagues.Where(user => !team.Members.Select(member => member.User).Contains(user.Colleague)).ToList();

            var model = new AddTeamUserViewModel(colleagues.Select(u => u.Colleague).ToList(), team);
            return PartialView("_AddTeamUser", model);
        }

        // POST: Teams/AddTeamUser
        [HttpPost, ActionName("AddTeamUser")]
        public async Task<ActionResult> AddTeamUser(int teamId, string userEmail)
        {
            var team = await _db.Teams.FindAsync(teamId);
            if (team == null)
                //return new HttpNotFoundResult("Cannot find a team with that Id");
                return Json(new { success = false, error = "Cannot find a team with that Id" });

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
                //return new HttpNotFoundResult("Cannot find a user with that Id");
                return Json(new { success = false, error = "Cannot find a user with that Id" });

            //Check if the current user is the team creator
            var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (!currentUser.Teams.Select(member => member.Team).Contains(team) ||
                team.Members.Where(member => member.Role == TeamMemberRole.Creator)
                    .Select(member => member.User)
                    .First() != currentUser)
                //return new HttpUnauthorizedResult("You cannot manage this team as you are not the creator");
                return Json(new { success = false, error = "You cannot manage this team as you are not the creator" });

            //Check if user is colleague
            if (!currentUser.Colleagues.Select(colleagues => colleagues.Colleague).Contains(user))
                //return new HttpUnauthorizedResult("This user is not one of your colleagues, you cannot add them to a team.");
                return Json(new { success = false, error = "This user is not one of your colleagues, you cannot add them to a team." });

            //Check if user is already a team member
            if(team.Members.Select(member => member.User).Contains(user))
                //Pretend it worked as we've got the desired outcome...
                return Json(new { success = true });

            try
            {
                //Remove user from team
                team.Members.Add(new TeamMember()
                {
                    Role = TeamMemberRole.Developer,
                    User = user
                });

                await _db.SaveChangesAsync();

                //If add works, return json okay
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                return Json(new { success = false, error = exception.Message });
            }
        }

        // GET: Teams/RemoveTeamUser
        public async Task<ActionResult> RemoveTeamUser(int teamId, string userId)
        {
            var team = await _db.Teams.FindAsync(teamId);
            if (team == null)
                return new HttpNotFoundResult("Cannot find a team with that Id");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new HttpNotFoundResult("Cannot find a user with that Id");

            //Check the user has permission
            var currentUser = _userManager.FindById(User.Identity.GetUserId());
            var teamUser = team.Members.First(m => m.User == currentUser);
            if (teamUser == null || teamUser.Role != TeamMemberRole.Creator)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have permission to perform this action");

            return PartialView("_RemoveTeamUser", new TeamUserRemoveViewModel(user, team));
        }

        // POST: Teams/RemoveTeamUser/5
        [HttpPost, ActionName("RemoveTeamUser")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmRemoveTeamUser(int teamId, string userId)
        {
            var team = await _db.Teams.FindAsync(teamId);
            if (team == null)
                return new HttpNotFoundResult("Cannot find a team with that Id");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new HttpNotFoundResult("Cannot find a user with that Id");

            //Check the user has permission
            var currentUser = _userManager.FindById(User.Identity.GetUserId());
            var teamUser = team.Members.First(m => m.User == currentUser);
            if (teamUser == null || teamUser.Role != TeamMemberRole.Creator)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have permission to perform this action");

            //Create model in case error
            var model = new TeamUserRemoveViewModel(user, team);

            //If the user isn't part of the team then return success
            if (!team.Members.Select(member => member.User).Contains(user))
            {
                return RedirectToAction("EditTeam", new { id = teamId });
                //return Json(new { success = true });
            }

            //Check they are not the owner
            if (team.Members.First(member => member.User == user).Role == TeamMemberRole.Creator)
            {
                model.Error = "Cannot remove the team creator";
                return PartialView("_RemoveTeamUser", model);
            }

            try
            {
                //Remove user from team
                var teamMember = team.Members.First(member => member.User == user);
                _db.Entry(teamMember).State = EntityState.Deleted;

                await _db.SaveChangesAsync();

                //If add works, return json okay
                return RedirectToAction("EditTeam", new { id = teamId });
                //return Json(new { success = true });
            }
            catch (Exception exception)
            {
                model.Error = exception.Message;
            }

            return PartialView("_RemoveTeamUser", model);
        }

        // POST: Teams/UpdateTeamName/5
        [HttpPost, ActionName("UpdateTeamName")]
        public async Task<ActionResult> UpdateTeamName(int teamId, string teamName)
        {
            var team = await _db.Teams.FindAsync(teamId);
            if (team == null)
                //return new HttpNotFoundResult("Cannot find a team with that Id");
                return Json(new { success = false, error = "Cannot find a team with that Id" });

            //Check if the current user is the team creator
            var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (!currentUser.Teams.Select(member => member.Team).Contains(team) ||
                team.Members.Where(member => member.Role == TeamMemberRole.Creator)
                    .Select(member => member.User)
                    .First() != currentUser)
                //return new HttpUnauthorizedResult("You cannot manage this team as you are not the creator");
                return Json(new { success = false, error = "You cannot manage this team as you are not the creator" });

            try
            {
                //Change the name
                team.TeamName = teamName.Trim();
                await _db.SaveChangesAsync();

                //If add works, return json okay
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                return Json(new { success = false, error = exception.Message });
            }
        }

        // POST: Teams/UpdateTeamVelocity/5
        [HttpPost, ActionName("UpdateTeamVelocity")]
        public async Task<ActionResult> UpdateTeamVelocity(int teamId, int velocity)
        {
            var team = await _db.Teams.FindAsync(teamId);
            if (team == null)
                //return new HttpNotFoundResult("Cannot find a team with that Id");
                return Json(new { success = false, error = "Cannot find a team with that Id" });

            //Check if the current user is the team creator
            var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (!currentUser.Teams.Select(member => member.Team).Contains(team) ||
                team.Members.Where(member => member.Role == TeamMemberRole.Creator)
                    .Select(member => member.User)
                    .First() != currentUser)
                //return new HttpUnauthorizedResult("You cannot manage this team as you are not the creator");
                return Json(new { success = false, error = "You cannot manage this team as you are not the creator" });

            try
            {
                //Change the name
                team.Velocity = velocity;
                await _db.SaveChangesAsync();

                //If add works, return json okay
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                return Json(new { success = false, error = exception.Message });
            }
        }

        // GET: Teams/RemoveTeam
        public async Task<ActionResult> RemoveTeam(int teamId)
        {
            var team = await _db.Teams.FindAsync(teamId);
            if (team == null)
                return new HttpNotFoundResult("Cannot find a team with that Id");

            //Check the user has permission
            var currentUser = _userManager.FindById(User.Identity.GetUserId());
            var teamUser = team.Members.First(m => m.User == currentUser);
            if (teamUser == null || teamUser.Role != TeamMemberRole.Creator)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have permission to perform this action");

            return PartialView("_RemoveTeam", new TeamRemoveViewModel(team));
        }

        // POST: Teams/RemoveTeam/5
        [HttpPost, ActionName("RemoveTeam")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmRemoveTeam(int teamId)
        {
            var team = await _db.Teams.FindAsync(teamId);
            if (team == null)
                return new HttpNotFoundResult("Cannot find a team with that Id");

            //Check the user has permission
            var currentUser = _userManager.FindById(User.Identity.GetUserId());
            var teamUser = team.Members.First(m => m.User == currentUser);
            if (teamUser == null || teamUser.Role != TeamMemberRole.Creator)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have permission to perform this action");

            //Create model in case error
            var model = new TeamRemoveViewModel(team);

            try
            {
                //Remove TeamMembers
                _db.TeamsMembers.RemoveRange(team.Members);

                //Remove team
                _db.Teams.Remove(team);

                await _db.SaveChangesAsync();

                //If add works, return json okay
                return Json(new { success = true }); //(close dialog)
            }
            catch (Exception exception)
            {
                model.Error = exception.Message;
            }

            return PartialView("_RemoveTeam", model);
        }

        // GET: Teams/AddTeamProject
        public async Task<ActionResult> AddTeamProject(int teamId)
        {
            var team = await _db.Teams.FindAsync(teamId);
            if (team == null)
                return new HttpNotFoundResult("Cannot find a team with that Id");

            //Check if the current user is the team creator
            var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (!currentUser.Teams.Select(member => member.Team).Contains(team) ||
                team.Members.Where(member => member.Role == TeamMemberRole.Creator)
                    .Select(member => member.User)
                    .First() != currentUser)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You cannot manage this team as you are not the creator");

            //Select all projects the team isn't already part of
            var projects = currentUser.Projects.Where(project => !project.Teams.Select(t => t.Team).Contains(team)).ToList();

            var model = new AddTeamProjectViewModel(projects, team);
            return PartialView("_AddTeamProject", model);
        }

        // POST: Teams/AddTeamProject
        [HttpPost, ActionName("AddTeamProject")]
        public async Task<ActionResult> AddTeamProject(int teamId, int projectId)
        {
            var team = await _db.Teams.FindAsync(teamId);
            if (team == null)
                return Json(new { success = false, error = "Cannot find a team with that Id" });

            var project = await _db.Projects.FindAsync(projectId);
            if (project == null)
                return Json(new { success = false, error = "Cannot find a project with that Id" });

            //Check if the current user is the team creator
            var currentUser = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            if (!currentUser.Teams.Select(member => member.Team).Contains(team) ||
                team.Members.Where(member => member.Role == TeamMemberRole.Creator)
                    .Select(member => member.User)
                    .First() != currentUser)
                return Json(new { success = false, error = "You cannot manage this team as you are not the creator" });

            //Check if the team already contains this project
            //Desired outcome, so return success
            if(team.Projects.Select(teamProject => teamProject.Project).Contains(project))
                return Json(new { success = true });

            try
            {
                _db.TeamProjects.Add(new TeamProject()
                {
                    Project = project,
                    Team = team
                });
                await _db.SaveChangesAsync();

                //If add works, return json okay
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                return Json(new { success = false, error = exception.Message });
            }
        }

        // GET: Teams/RemoveTeamProject
        public async Task<ActionResult> RemoveTeamProject(int teamId, int projectId)
        {
            var team = await _db.Teams.FindAsync(teamId);
            if (team == null)
                return new HttpNotFoundResult("Cannot find a team with that Id");

            //Check the user has permission
            var currentUser = _userManager.FindById(User.Identity.GetUserId());
            var teamUser = team.Members.First(m => m.User == currentUser);
            if (teamUser == null || teamUser.Role != TeamMemberRole.Creator)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have permission to perform this action");

            var project = await _db.Projects.FindAsync(projectId);
            if (project == null)
                return new HttpNotFoundResult("Cannot find a project with that Id");

            return PartialView("_RemoveTeamProject", new TeamProjectRemoveViewModel(team, project));
        }

        // POST: Teams/RemoveTeamProject/5
        [HttpPost, ActionName("RemoveTeamProject")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmRemoveTeamProject(int teamId, int projectId)
        {
            var team = await _db.Teams.FindAsync(teamId);
            if (team == null)
                return new HttpNotFoundResult("Cannot find a team with that Id");

            var project = await _db.Projects.FindAsync(projectId);
            if (project == null)
                return new HttpNotFoundResult("Cannot find a project with that Id");

            //Check the user has permission
            var currentUser = _userManager.FindById(User.Identity.GetUserId());
            var teamUser = team.Members.First(m => m.User == currentUser);
            if (teamUser == null || teamUser.Role != TeamMemberRole.Creator)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have permission to perform this action");

            //Create model in case error
            var model = new TeamProjectRemoveViewModel(team, project);

            //If the project isn't part of the team then return success
            if (!team.Projects.Select(t => t.Project).Contains(project))
            {
                return RedirectToAction("EditTeam", new { id = teamId });
            }

            try
            {
                //Remove project from team
                var teamProject = team.Projects.First(t => t.Project == project);
                _db.Entry(teamProject).State = EntityState.Deleted;

                await _db.SaveChangesAsync();

                //If add works, redirect to edit team view
                return RedirectToAction("EditTeam", new { id = teamId });
            }
            catch (Exception exception)
            {
                model.Error = exception.Message;
            }

            return PartialView("_RemoveTeamUser", model);
        }

    }
}