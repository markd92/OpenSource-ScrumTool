using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OpenSourceScrumTool.Extensions;
using OpenSourceScrumTool.Models;
using OpenSourceScrumTool.Models.DataModels;

namespace OpenSourceScrumTool.Models.ViewModels
{
    public class TeamsIndexViewModel
    {
        public TeamsIndexViewModel(List<ApplicationUser> colleagues, List<TeamMember> teams)
        {
            ColleaguesViewModel = new ColleaguesViewModel(colleagues);
            TeamsViewModel = new TeamsViewModel(teams);
        }

        public ColleaguesViewModel ColleaguesViewModel { get; set; }
        public TeamsViewModel TeamsViewModel { get; set; }
    }

    public class ColleaguesViewModel
    {
        public ColleaguesViewModel(List<ApplicationUser> colleagues)
        {
            Colleagues = new List<ColleagueItemViewModel>();
            foreach (var colleague in colleagues)
            {
                Colleagues.Add(new ColleagueItemViewModel(colleague));
            }
        }

        public List<ColleagueItemViewModel> Colleagues { get; set; }
    }

    public class ColleagueItemViewModel
    {
        public ColleagueItemViewModel() { }

        public ColleagueItemViewModel(ApplicationUser colleague)
        {
            Fullname = colleague.Fullname ?? colleague.Email;
            Email = colleague.Email;
        }
        public string Email { get; set; }
        public string Fullname { get; set; }
    }

    public class TeamsViewModel
    {
        public TeamsViewModel() { }

        public TeamsViewModel(List<TeamMember> teams)
        {
            using (var db = new ApplicationDbContext())
            {
                Teams = new List<TeamItemViewModel>();
                foreach (var member in teams)
                {
                    var dbMember = db.TeamsMembers.Find(member.Id);
                    Teams.Add(new TeamItemViewModel(dbMember.Team));
                }
            }
        }

        public List<TeamItemViewModel> Teams { get; set; }
    }

    public class TeamItemViewModel
    {
        public TeamItemViewModel() { }
        public TeamItemViewModel(Team team)
        {
            Id = team.Id;
            TeamName = team.TeamName;
            Members = new List<TeamMemberItemViewModel>();
            foreach (var member in team.Members)
            {
                Members.Add(new TeamMemberItemViewModel(member));
            }
            Projects = new List<TeamProjectItemViewModel>();
            foreach (var project in team.Projects.Select(t => t.Project))
            {
                Projects.Add(new TeamProjectItemViewModel(project));
            }
        }

        public int Id { get; set; }
        public string TeamName { get; set; }
        public List<TeamMemberItemViewModel> Members { get; set; }
        public List<TeamProjectItemViewModel> Projects { get; set; }
    }

    public class TeamEditViewModel
    {
        public TeamEditViewModel() { }
        public TeamEditViewModel(Team team)
        {
            Id = team.Id;
            TeamName = team.TeamName;
            Velocity = team.Velocity;
            Members = new List<TeamMemberItemViewModel>();
            foreach (var member in team.Members)
            {
                Members.Add(new TeamMemberItemViewModel(member));
            }
            Projects = new List<TeamProjectItemViewModel>();
            foreach (var project in team.Projects.Select(t => t.Project))
            {
                Projects.Add(new TeamProjectItemViewModel(project));
            }
        }

        public string Error { get; set; }
        public int Id { get; set; }
        [Required]
        public string TeamName { get; set; }
        [Required]
        [Range(0, Int32.MaxValue)]
        [RegularExpression(@"^\d*$", ErrorMessage = "Please enter a whole number")]
        public int Velocity { get; set; }
        public List<TeamMemberItemViewModel> Members { get; set; }
        public List<TeamProjectItemViewModel> Projects { get; set; }
    }

    public class TeamProjectItemViewModel
    {
        public TeamProjectItemViewModel() { }
        public TeamProjectItemViewModel(Project project)
        {
            ProjectId = project.Id;
            ProjectName = project.Title;
        }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
    }

    public class TeamMemberItemViewModel
    {
        public TeamMemberItemViewModel() { }

        public TeamMemberItemViewModel(TeamMember member)
        {
            using (
                var userManager =
                    new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
            {
                var user = userManager.FindById(member.User_Id);
                Fullname = user.Fullname ?? user.Email;
            }
            UserId = member.User_Id;
            Role = member.Role;
            RoleName = member.Role.DisplayName();
        }

        public string UserId { get; set; }
        public string Fullname { get; set; }
        public TeamMemberRole Role { get; set; }
        public string RoleName { get; set; }
    }

    public class ColleagueCreateViewModel
    {
        public string Error { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ColleagueRemoveViewModel
    {
        public ColleagueRemoveViewModel(ApplicationUser colleague)
        {
            Fullname = colleague.Fullname ?? colleague.Email;
        }

        public string Error { get; set; }
        public string Fullname { get; set; }
    }

    public class TeamCreateViewModel
    {
        public string Error { get; set; }
        [Required]
        [DisplayName("Team Name")]
        public string TeamName { get; set; }
        [Required]
        [Range(0, Int32.MaxValue)]
        [RegularExpression(@"^\d*$", ErrorMessage = "Please enter a whole number")]
        public int Velocity { get; set; }
    }

    public class TeamUserRemoveViewModel
    {
        public TeamUserRemoveViewModel(ApplicationUser colleague, Team team)
        {
            Id = team.Id;
            TeamName = team.TeamName;
            Fullname = colleague.Fullname ?? colleague.Email;
        }

        public int Id { get; set; }
        public string Error { get; set; }
        [DisplayName("Team Name")]
        public string TeamName { get; set; }
        public string Fullname { get; set; }
    }

    public class TeamRemoveViewModel
    {
        public TeamRemoveViewModel(Team team)
        {
            Id = team.Id;
            TeamName = team.TeamName;
        }

        public int Id { get; set; }
        public string Error { get; set; }
        [DisplayName("Team Name")]
        public string TeamName { get; set; }
    }

    public class AddTeamUserViewModel
    {
        public AddTeamUserViewModel(List<ApplicationUser> colleagues, Team team)
        {
            Id = team.Id;
            TeamName = team.TeamName;
            Colleagues = new List<ColleagueItemViewModel>();
            foreach (var colleague in colleagues)
            {
                Colleagues.Add(new ColleagueItemViewModel(colleague));
            }
        }

        public int Id { get; set; }
        public string TeamName { get; set; }
        public List<ColleagueItemViewModel> Colleagues { get; set; }
    }

    public class AddTeamProjectViewModel
    {
        public AddTeamProjectViewModel(List<Project> projects, Team team)
        {
            Id = team.Id;
            TeamName = team.TeamName;
            Projects = new List<TeamProjectItemViewModel>();
            foreach (var project in projects)
            {
                Projects.Add(new TeamProjectItemViewModel(project));
            }
        }

        public int Id { get; set; }
        public string TeamName { get; set; }
        public List<TeamProjectItemViewModel> Projects { get; set; }
    }

    public class TeamProjectRemoveViewModel
    {
        public TeamProjectRemoveViewModel(Team team, Project project)
        {
            TeamId = team.Id;
            TeamName = team.TeamName;
            ProjectId = project.Id;
            ProjectName = project.Title;
        }

        public string Error { get; set; }
        public int TeamId { get; set; }
        public int ProjectId { get; set; }
        public string TeamName { get; set; }
        public string ProjectName { get; set; }
    }
}