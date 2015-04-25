using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OpenSourceScrumTool.Models.DataModels
{
    public class Team
    {
        public Team()
        {
            this.Members = new HashSet<TeamMember>();
            this.Projects = new HashSet<TeamProject>();
        }

        [Key]
        public int Id { get; set; }
        [Required]
        public string TeamName { get; set; }
        [Required]
        [DefaultValue(0)]
        public int Velocity { get; set; }
        public virtual ICollection<TeamMember> Members { get; set; }
        public virtual ICollection<TeamProject> Projects { get; set; }
    }
}