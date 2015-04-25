using System;
using System.Collections.Generic;

namespace OpenSourceScrumTool.Models.DataModels
{
    public class ChangeLog
    {
        public double Version { get; set; }
        public string MajorFeature { get; set; }
        public List<String> Changes { get; set; }
    }
}