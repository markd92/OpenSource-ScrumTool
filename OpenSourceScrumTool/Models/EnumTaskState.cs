using System;
using System.ComponentModel.DataAnnotations;

namespace OpenSourceScrumTool.Models
{
    public enum EnumTaskState : int
    {
        [Display(Name = "Not Started")]
        NotStarted = 0,
        [Display(Name = "In Progress")]
        InProgress = 1,
        Done = 2
    }
}
