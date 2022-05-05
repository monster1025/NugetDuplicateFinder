using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProjectNugetDuplicateFinder
{
    public record FileSearchCriteria
    {
        public string[] Extensions { get; set; } = Array.Empty<string>();

        public string BaseDirectory { get; set; }

        public string[] TargetDirectories { get; set; } = Array.Empty<string>();

        public IEnumerable<string> AbsoluteTargetDirectories
        {
            get
            {
                return TargetDirectories?
                    .Select(td => Path.Combine(BaseDirectory, td))
                    .Where(td => !string.IsNullOrWhiteSpace(td) && Directory.Exists(td));
            }
        }

        public bool Recursive { get; set; }

        public SearchOption SearchOption => Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
    }
}