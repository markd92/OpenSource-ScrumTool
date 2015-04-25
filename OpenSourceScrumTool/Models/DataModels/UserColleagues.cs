using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OpenSourceScrumTool.Models.DataModels
{
    public class UserColleagues
    {
        //Foreign key for Colleague
        //Forms Composite Key
        [Key]
        [Column(Order = 1)]
        public string ColleagueId { get; set; }

        [ForeignKey("ColleagueId")]
        public virtual ApplicationUser Colleague { get; set; }

        //Foreign key for User
        //Forms Composite Key
        [Key]
        [Column(Order = 2)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}