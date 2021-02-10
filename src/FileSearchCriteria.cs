using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProjectNugetDuplicateFinder
{
    public class FileSearchCriteria
    {
        public string[] Extensions { get; set; } = new string[0];

        public string BaseDirectory { get; set; }

        public string[] TargetDirectories { get; set; } = new string[0];

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

        public SearchOption SearchOption
        {
            get
            {
                return Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            }
        }

    }
}