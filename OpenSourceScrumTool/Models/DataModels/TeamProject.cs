using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OpenSourceScrumTool.Models.DataModels
{
    public class TeamProject
    {
        //Foreign key for Project
        //Forms Composite Key
        [Key]
        [Column(Order = 1)]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        //Foreign key for Team
        //Forms Composite Key
        [Key]
        [Column(Order = 2)]
        public int TeamId { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
    }
}