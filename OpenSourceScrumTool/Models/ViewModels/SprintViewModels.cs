using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using OpenSourceScrumTool.Models.DataModels;

namespace OpenSourceScrumTool.Models.ViewModels
{
    public static class SprintTimes
    {
        public static List<SelectListItem> SprintTimesSelectListItems
        {
            get
            {
                return new List<SelectListItem>()
                {
                    new SelectListItem(){ Text = "1 Week", Value = "1"},
                    new SelectListItem(){ Text = "2 Weeks", Value = "2"},
                    new SelectListItem(){ Text = "3 Weeks", Value = "3"},
                    new SelectListItem(){ Text = "4 Weeks", Value = "4"}
                };
            }
        }
    }

    public class SprintIndexViewModel
    {
        public string UserName { get; set; }
        public List<ProjectSprintViewModel> Projects { get; set; }
    }

    public class ProjectSprintViewModel
    {
        public int ProjectId { get; set; }
        public string TeamName { get; set; }
        public string ProjectName { get; set; }
        public int SprintItteration { get; set; }
        public DateTime SprintStartDate { get; set; }
        public DateTime SprintEndDate { get; set; }
        public List<SprintFeatures> Features { get; set; }
    }

    public class SprintFeatures
    {
        public int FeatureId { get; set; }
        public string FeatureName { get; set; }
        public string FeatureDescription { get; set; }
        public List<TaskItemViewModel> TasksNotStarted { get; set; }
        public List<TaskItemViewModel> TasksInProgress { get; set; }
        public List<TaskItemViewModel> TasksDone { get; set; }
    }

    public class SprintEditViewModel
    {
        public string Error { get; set; }
        public int Id { get; set; }
        public int SprintDuration { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
    }

    public class SprintSettingsViewModel
    {
        public int Id { get; set; }
        public string Error { get; set; }
        public string Info { get; set; }
        public SprintSettingsFormViewModel FormViewModel { get; set; }
        public SprintSettingsTableViewModel TableViewModel { get; set; }
    }

    public class SprintSettingsFormViewModel
    {
        public string Error { get; set; }
        public int Id { get; set; }
        [DefaultValue(Project.DefaultSprintDuration)]
        [DisplayName("Sprint Duration")]
        [Required]
        public int SprintDuration { get; set; }
        public List<SelectListItem> SprintTimesList { get { return SprintTimes.SprintTimesSelectListItems; } }
    }

    public class SprintSettingsTableViewModel
    {
        public int Id { get; set; }
        public List<SprintSettingsTableItemViewModel> Sprints { get; set; }
    }

    public class SprintSettingsTableItemViewModel
    {
        public int Id { get; set; }
        public int Iteration { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int FeaturesCount { get; set; }
    }

    public class SprintItemViewModel
    {
        public int Id { get; set; }
        public int Iteration { get; set; }
        public int[] FeatureIds { get; set; }
    }
}
