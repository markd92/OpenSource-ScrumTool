using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenSourceScrumTool.Models.DataModels
{
    public class Sprint
    {
        public Sprint()
        {
            this.Features = new HashSet<Feature>();
        }

        [Key]
        public int Id { get; set; }

        public int Project_Id { get; set; }

        public int Iteration { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [ForeignKey("Project_Id")]
        public virtual Project Project { get; set; }
        public virtual ICollection<Feature> Features { get; set; }
    }
}
