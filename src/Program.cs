using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Semver;

namespace ProjectNugetDuplicateFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            var searchCriteria = new FileSearchCriteria
            {
                Extensions = new string[] { ".csproj" },
                BaseDirectory = "D:\\Projects\\onelia",
                TargetDirectories = new string[] { "onelia" },
                Recursive = true,
            };

            var result = new Finder().Find(searchCriteria);


            //result = result.Where(f => f.EndsWith("Falcon.Api.Payment.Tests.csproj")).ToList();

            foreach (var project in result)
            {
                try
                {
                    var text = File.ReadAllText(project);
                    var document = XDocument.Parse(text);
                    var dublicates = document
                        .Descendants("ItemGroup")
                        .Descendants("PackageReference")
                        .GroupBy(f => f.Attribute("Include").Value.ToLower())
                        .Where(f => f.Count() > 1);

                    var isAnyRemoved = false;
                    foreach (var duplicate in dublicates)
                    {
                        var maxVersion = duplicate.Max(f => SemVersion.Parse(f.Attribute("Version").Value));

                        var max = duplicate.First(f => f.Attribute("Version").Value == maxVersion.ToString());
                        IEnumerable<XElement> toRemove = duplicate.Except(new List<XElement> { max });
                        foreach (var xElement in toRemove)
                        {
                            xElement.Remove();
                            isAnyRemoved = true;
                        }
                    }

                    if (isAnyRemoved)
                    {
                        var newXml = document.ToString();
                        File.WriteAllText(project, newXml);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            Console.WriteLine(string.Join("\n", result));
            Console.ReadLine();
        }
    }
}
