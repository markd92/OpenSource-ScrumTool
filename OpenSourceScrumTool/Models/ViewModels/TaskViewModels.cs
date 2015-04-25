using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OpenSourceScrumTool.Models.ViewModels
{
    public class TaskCreateViewModel
    {
        public string Error { get; set; }
        public int FeatureId { get; set; }
        [Required]
        [StringLength(255)]
        public string Title { get; set; }
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Required]
        public EnumTaskState State { get; set; }
        [Required]
        [Display(Name = "Time Remaining")]
        public long TimeRemaining { get; set; }
    }

    public class TaskDeleteViewModel
    {
        public string Error { get; set; }
        public string FeatureTitle { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public EnumTaskState State { get; set; }
        [Display(Name = "Time Remaining")]
        public long TimeRemaining { get; set; }
    }

    public class TaskEditViewModel
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
        public EnumTaskState State { get; set; }
        [Required]
        [Display(Name = "Time Remaining")]
        public long TimeRemaining { get; set; }
    }

    public class TaskIndexViewModel
    {
        public TaskIndexViewModel()
        {
        }

        public TaskIndexViewModel(DataModels.Feature feature)
        {
            FeatureId = feature.Id;
            ProjectId = feature.ProjectId;
            Tasks = feature.Tasks.OrderBy(p => p.Priority).Select(t => new TaskItemViewModel(t)).ToList();
        }

        public List<TaskItemViewModel> Tasks { get; set; }
        public int ProjectId { get; set; }
        public int FeatureId { get; set; }
    }

    public class TaskItemViewModel
    {
        public TaskItemViewModel() { }

        public TaskItemViewModel(DataModels.ScrumTask scrumTask)
        {
            Id = scrumTask.Id;
            Title = scrumTask.Title;
            //HTML is escaped in view (when rendering as script)
            Description = scrumTask.Description;
            Priority = scrumTask.Priority;
            State = scrumTask.State;
            TimeRemaining = scrumTask.TimeRemaining;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public EnumTaskState State { get; set; }
        public long TimeRemaining { get; set; }
    }
}
