using System.Collections.Generic;
using OpenSourceScrumTool.Models.DataModels;

namespace OpenSourceScrumTool.Models.ViewModels
{
    public class HomeChangeLogViewModels
    {
        public List<ChangeLog> Changes { get; set; }
    }

    public class HomeKnownIssuesViewModel
    {
        public List<KnownIssue> Issues { get; set; }
    }
}