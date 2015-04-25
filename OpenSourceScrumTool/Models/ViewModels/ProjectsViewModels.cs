using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OpenSourceScrumTool.Models.DataModels;

namespace OpenSourceScrumTool.Models.ViewModels
{
    public class ProjectIndexViewModel
    {
        public ProjectIndexViewModel(List<Project> projects)
        {
            Projects = projects.Select(p => new ProjectItemViewModel(p)).ToList();
        }

        public List<ProjectItemViewModel> Projects { get; set; }
    }

    public class ProjectItemViewModel
    {
        public ProjectItemViewModel(Project p)
        {
            Id = p.Id;
            Title = p.Title;
            Description = p.Description;
            Priority = 0; //TODO: This will be difficult because it needs to be different for different users...
            FeaturesCount = p.Features.Count;

            var featuresWeight = p.Features.Where(feature => feature.Weight >= 0).ToList();
            var totalWeight = featuresWeight.Any()
                ? featuresWeight.Sum(feature => feature.Weight)
                : 0;
            double doneWeightCount = 0, inProgWeightCount = 0, notStartWeightCount = 0;
            if (totalWeight > 0)
            {
                doneWeightCount = featuresWeight.Any()
                    ? featuresWeight.Where(feature => feature.Status == EnumTaskState.Done)
                        .Sum(feature => feature.Weight)
                    : 0;
                inProgWeightCount = featuresWeight.Any()
                    ? featuresWeight.Where(feature => feature.Status == EnumTaskState.InProgress)
                        .Sum(feature => feature.Weight)
                    : 0;
                notStartWeightCount = featuresWeight.Any()
                    ? featuresWeight.Where(feature => feature.Status == EnumTaskState.NotStarted)
                        .Sum(feature => feature.Weight)
                    : 0;
            }
            else
            {
                //Fallback to counts if weights set to 0
                totalWeight = featuresWeight.Any()
                    ? featuresWeight.Count()
                    : 0;
                doneWeightCount = featuresWeight.Any()
                   ? featuresWeight.Count(feature => feature.Status == EnumTaskState.Done)
                   : 0;
                inProgWeightCount = featuresWeight.Any()
                    ? featuresWeight.Count(feature => feature.Status == EnumTaskState.InProgress)
                    : 0;
                notStartWeightCount = featuresWeight.Any()
                    ? featuresWeight.Count(feature => feature.Status == EnumTaskState.NotStarted)
                    : 0;
            }

            DoneTasksPercentage = p.Features.Any() ? doneWeightCount / totalWeight * 100 : 0;
            InProgressTasksPercentage = p.Features.Any() ? inProgWeightCount / totalWeight * 100 : 0;
            NotStartedTasksPercentage = p.Features.Any() ? notStartWeightCount / totalWeight * 100 : 0;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        //HTML is escaped in view (when rendering as script)
        public string Description { get; set; }
        public int Priority { get; set; }
        public int FeaturesCount { get; set; }
        public double DoneTasksPercentage { get; set; }
        public double InProgressTasksPercentage { get; set; }
        public double NotStartedTasksPercentage { get; set; }
    }

    public class ProjectCreateViewModel
    {
        public string Error { get; set; }
        [Required]
        [StringLength(255)]
        public string Title { get; set; }
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [DefaultValue(Project.DefaultSprintDuration)]
        [DisplayName("Sprint Duration")]
        [Required]
        public int SprintDuration { get; set; }
        public List<SelectListItem> SprintTimesList { get { return SprintTimes.SprintTimesSelectListItems; } }
    }

    public class ProjectDeleteViewModel
    {
        public string Error { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class ProjectEditViewModel
    {
        public string Error { get; set; }
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Title { get; set; }
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [DefaultValue(Project.DefaultSprintDuration)]
        [DisplayName("Sprint Duration")]
        [Required]
        public int SprintDuration { get; set; }
        public List<SelectListItem> SprintTimesList { get { return SprintTimes.SprintTimesSelectListItems; } }
    }

    public class ProjectDetailsViewModel
    {
        public ProjectDetailsViewModel()
        {
        }

        public ProjectDetailsViewModel(Project project)
        {
            Id = project.Id;
            Title = project.Title;
            //HTML is escaped in view (when rendering as script)
            Description = project.Description;
            CompleteFeaturesCount = project.CompleteFeaturesCount;
            IncompleteFeaturesCount = project.IncompleteFeaturesCount;
            HoursRemaining = project.HoursRemaining;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int IncompleteFeaturesCount { get; set; }
        public int CompleteFeaturesCount { get; set; }
        public double HoursRemaining { get; set; }
    }
}