using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OpenSourceScrumTool.Models.DataModels
{
    public enum TeamMemberRole
    {
        Creator = 0,
        ScrumMaster = 1,
        Developer = 2
    }

    public class TeamMember
    {
        [Key]
        public long Id { get; set; }
        [ForeignKey("Team_Id")]
        public virtual Team Team { get; set; }
        public int Team_Id { get; set; }

        [ForeignKey("User_Id")]
        public virtual ApplicationUser User { get; set; }
        public string User_Id { get; set; }
        public TeamMemberRole Role { get; set; }
    }
}