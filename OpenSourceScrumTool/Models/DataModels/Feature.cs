using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OpenSourceScrumTool.Models.DataModels
{
    public class Feature
    {
        public Feature()
        {
            this.Tasks = new HashSet<ScrumTask>();
            this.Sprints = new HashSet<Sprint>();
        }

        [Key]
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public double Weight { get; set; }

        [Required]
        public int Priority { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }


        public virtual ICollection<ScrumTask> Tasks { get; set; }

        public virtual ICollection<Sprint> Sprints { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        public int TasksDone { get { return Tasks.Count(task => task.State == EnumTaskState.Done); } }
        public int TasksInProgress { get { return Tasks.Count(task => task.State == EnumTaskState.InProgress); } }
        public int TasksNotStarted { get { return Tasks.Count(task => task.State == EnumTaskState.NotStarted); } }
        public long TimeRemaining { get { return Tasks.Sum(task => task.TimeRemaining); } }

        public EnumTaskState Status
        {
            get
            {
                //if no tasks 
                //  or all tasks are not started
                if(Tasks.Count == 0 || Tasks.Count == this.TasksNotStarted)
                    return EnumTaskState.NotStarted;

                //if all tasks are done
                if(Tasks.Count == this.TasksDone)
                    return EnumTaskState.Done;

                //otherwise
                return EnumTaskState.InProgress;
            }
        }
    }
}
