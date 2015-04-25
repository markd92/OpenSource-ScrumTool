using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using OpenSourceScrumTool.Extensions;
using OpenSourceScrumTool.Models.DataModels;

namespace OpenSourceScrumTool.Models.ViewModels
{
    public static class FeatureWeights
    {
        public static List<SelectListItem> WeightTypesSelectListItems
        {
            get
            {
                return new List<SelectListItem>()
                {
                    new SelectListItem(){ Text = "0", Value = "0"},
                    new SelectListItem(){ Text = "1/2", Value = "0.5"},
                    new SelectListItem(){ Text = "1", Value = "1"},
                    new SelectListItem(){ Text = "2", Value = "2"},
                    new SelectListItem(){ Text = "3", Value = "3"},
                    new SelectListItem(){ Text = "5", Value = "5"},
                    new SelectListItem(){ Text = "8", Value = "8"},
                    new SelectListItem(){ Text = "13", Value = "13"},
                    new SelectListItem(){ Text = "20", Value = "20"},
                    new SelectListItem(){ Text = "40", Value = "40"},
                    new SelectListItem(){ Text = "100", Value = "100"},
                    new SelectListItem(){ Text = "?", Value = "-1"}
                };
            }
        }
    }

    public class FeatureItemViewModel
    {
        public FeatureItemViewModel()
        {
        }

        public FeatureItemViewModel(Feature feature)
        {
            Id = feature.Id;
            Title = feature.Title;
            //HTML is escaped in view (when rendering as script)
            Description = feature.Description;
            Priority = feature.Priority;
            State = feature.Status;
            Weight = feature.Weight;
            ContainsTasks = feature.Tasks.Any();
            DoneTasksPercentage = feature.Tasks.Any() ? (double)feature.TasksDone / feature.Tasks.Count * 100 : 0;
            InProgressTasksPercentage = feature.Tasks.Any() ? (double)feature.TasksInProgress / feature.Tasks.Count * 100 : 0;
            NotStartedTasksPercentage = feature.Tasks.Any() ? (double)feature.TasksNotStarted / feature.Tasks.Count * 100 : 0;
            InCurrentSprint = feature.Sprints.Any() && feature.Project.GetThisSprint().Features.Contains(feature);
            Sprints = feature.Sprints.Any() ? feature.Sprints.Select(sprint => sprint.Id).ToArray() : new int[0];
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public EnumTaskState State { get; set; }
        public double Weight { get; set; }
        public bool ContainsTasks { get; set; }
        public double DoneTasksPercentage { get; set; }
        public double InProgressTasksPercentage { get; set; }
        public double NotStartedTasksPercentage { get; set; }
        public bool InCurrentSprint { get; set; }
        public int[] Sprints { get; set; }
    }

    public class FeatureIndexViewModel
    {
        public FeatureIndexViewModel()
        {
        }

        public FeatureIndexViewModel(Project project)
        {
            var db = new ApplicationDbContext();

            //Find the current sprint
            var currentSprint = project.GetThisSprint();

            //Get list of features and filter if arguments provided
            var filteredFeaturesList = project.Features;
            ProjectId = project.Id;
            CurrentSprintId = currentSprint != null ? (int?)currentSprint.Id : null;
            Features = filteredFeaturesList
                .OrderBy(p => p.Priority)
                .Select(f => new FeatureItemViewModel(f)).ToList();
            Sprints = project.Sprints
                .OrderBy(s => s.Iteration)
                .Select(s => new SprintItemViewModel()
                {
                    Id = s.Id,
                    Iteration = s.Iteration
                }).ToList();
            TeamVelocity = project.Teams.Any() ? project.Teams.Sum(i => i.Team.Velocity) : -1;
        }

        public int ProjectId { get; set; }
        public int? CurrentSprintId { get; set; }
        public List<SprintItemViewModel> Sprints { get; set; }
        public List<FeatureItemViewModel> Features { get; set; }
        public int TeamVelocity { get; set; }
    }

    public class FeatureCreateViewModel
    {
        public string Error { get; set; }
        public int ProjectId { get; set; }
        [Required]
        [StringLength(255)]
        public string Title { get; set; }
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Required]
        public double Weight { get; set; }
        public List<SelectListItem> WeightTypes { get { return FeatureWeights.WeightTypesSelectListItems; } }
    }

    public class FeatureEditViewModel
    {
        public string Error { get; set; }
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Title { get; set; }
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Required]
        public double Weight { get; set; }
        public List<SelectListItem> WeightTypes { get { return FeatureWeights.WeightTypesSelectListItems; } }
    }

    public class FeatureDeleteViewModel
    {
        public string Error { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Weight { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DateCreated { get; set; }
    }

    public class FeatureDetailsViewModel
    {
        public FeatureDetailsViewModel()
        {
        }

        public FeatureDetailsViewModel(Feature feature)
        {
            Id = feature.Id;
            ProjectId = feature.ProjectId;
            //HTML is escaped in view (when rendering as script)
            Description = feature.Description;
            Title = feature.Title;
            TasksDone = feature.TasksDone;
            TasksInProgress = feature.TasksInProgress;
            TasksNotStarted = feature.TasksNotStarted;
            TimeRemaining = feature.TimeRemaining;
        }

        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TasksDone { get; set; }
        public int TasksInProgress { get; set; }
        public int TasksNotStarted { get; set; }
        public long TimeRemaining { get; set; }
    }
}