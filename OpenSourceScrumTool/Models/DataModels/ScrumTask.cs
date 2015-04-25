using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenSourceScrumTool.Models.DataModels
{
    public class ScrumTask
    {
        [Key]
        public int Id { get; set; }

        public int FeatureId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        public EnumTaskState State { get; set; }

        [Required]
        [Display(Name = "Time Remaining")]
        public long TimeRemaining { get; set; }

        [Required]
        public int Priority { get; set; }


        [ForeignKey("FeatureId")]
        public virtual Feature Feature { get; set; }
    }
}
