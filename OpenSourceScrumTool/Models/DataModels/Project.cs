using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OpenSourceScrumTool.Models.DataModels
{
    public class Project
    {
        private const int MAX_SPRINTS_DISPLAY = 5;

        public Project()
        {
            this.Features = new HashSet<Feature>();
            this.Teams = new HashSet<TeamProject>();
            this.Sprints = new HashSet<Sprint>();
        }

        public const int DefaultSprintDuration = 2;

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        [DefaultValue(Project.DefaultSprintDuration)]
        [Required]
        public int SprintDuration { get; set; }

        public virtual ICollection<Feature> Features { get; set; }

        [InverseProperty("Project")]
        public virtual ICollection<TeamProject> Teams { get; set; }

        public virtual ICollection<Sprint> Sprints { get; set; }

        [ForeignKey("User_Id")]
        public virtual ApplicationUser Owner { get; set; }
        public string User_Id { get; set; }

        public int IncompleteFeaturesCount
        {
            get { return Features.Count - this.CompleteFeaturesCount; }
        }

        public int CompleteFeaturesCount
        {
            get { return Features.Count(feature => feature.Status == EnumTaskState.Done); }
        }

        public long HoursRemaining
        {
            get { return Features.Sum(feature => feature.Tasks.Sum(task => task.TimeRemaining)); }
        }

        public Sprint GetThisSprint()
        {
            if (this.Sprints.Any())
            {
                //Find this sprint
                Sprint thisSprint = null;
                var matchedSprints =
                    this.Sprints.Where(sprint => sprint.StartDate.Date <= DateTime.Now && sprint.EndDate.Date > DateTime.Now);
                var enumerable = matchedSprints as Sprint[] ?? matchedSprints.ToArray();
                if (enumerable.Any())
                    thisSprint = enumerable.OrderByDescending(sprint => sprint.EndDate).First();
                else
                {
                    var oldSprints = this.Sprints.Where(sprint => sprint.EndDate.Date < DateTime.Now);
                    var sprints = oldSprints as Sprint[] ?? oldSprints.ToArray();
                    thisSprint = sprints.Any() ? sprints.OrderByDescending(sprint => sprint.EndDate).First() : this.Sprints.OrderBy(sprint => sprint.StartDate).First();
                }
                return thisSprint;
            }
            //No Sprints
            return null;
        }
    }
}
