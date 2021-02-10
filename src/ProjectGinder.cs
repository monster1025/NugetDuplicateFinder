using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProjectNugetDuplicateFinder
{
    public class Finder
    {
        public IEnumerable<string> Find(FileSearchCriteria criteria)
        {
            return criteria.AbsoluteTargetDirectories.SelectMany(td => FindFiles(td, criteria.Extensions, criteria.SearchOption));
        }

        public IEnumerable<string> FindFiles(string folderPath, string[] extensions, SearchOption searchOption)
        {
            return Directory.GetFiles(folderPath, "*.*", searchOption)
                .Where(s =>
                    !extensions.Any() ||
                    extensions.Any(e => string.Compare(e, Path.GetExtension(s), true) == 0)
                );
        }
    }
}
