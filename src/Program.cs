using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            FixDuplicates();
            //UpgradePackage("Sibur.Digital.MultiBus.Dto", "0.2.84");
        }

        private static void UpgradePackage(string packageName, string targetVersion)
        {
            var searchCriteria = new FileSearchCriteria
            {
                Extensions = new string[] {".sln"},
                BaseDirectory = "D:\\Projects\\sibur",
                TargetDirectories = new string[] { "implementation" },
                Recursive = true,
            };

            var solutions = new Finder().Find(searchCriteria);
            foreach (var solution in solutions)
            {
                var solutionDirectory = new FileInfo(solution).Directory!.ToString();
                var projectSearchCriteria = searchCriteria with
                {
                    Extensions = new string[] { ".csproj" },
                    BaseDirectory = solutionDirectory,
                    TargetDirectories = new string[] {"."}
                };
                var projects = new Finder().Find(projectSearchCriteria);

                var isRepositoryChanged = false;
                foreach (var project in projects)
                {
                    try
                    {
                        var text = File.ReadAllText(project);
                        var document = XDocument.Parse(text);
                        var versionsDiffersFromCurrent = document
                            .Descendants("ItemGroup")
                            .Descendants("PackageReference")
                            .Where(f => f.Attribute("Include").Value.ToLower() == packageName.ToLower());

                        var isAnyRemoved = false;
                        foreach (var versionDiffersFromCurrent in versionsDiffersFromCurrent)
                        {
                            var oldVersion = versionDiffersFromCurrent.Attribute("Version")!.Value;
                            if (oldVersion != targetVersion)
                            {
                                versionDiffersFromCurrent.Attribute("Version").Value = targetVersion;
                                isAnyRemoved = true;
                                isRepositoryChanged = true;
                            }
                        }

                        if (isAnyRemoved)
                        {
                            var newXml = document.ToString();
                            File.WriteAllText(project, newXml);
                            Console.WriteLine($"> Fixed: {project}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                if (isRepositoryChanged)
                {
                    var hash = Guid.NewGuid().ToString("N")[0..4];
                    var cmd = "git switch master && " +
                              $"git pull && " +
                              $"git checkout -b MPSH-upgrade-dto-{hash} && " +
                              "git add . && " +
                              $"git commit -m \"Upgrade_{packageName}_to_version_{targetVersion}\" && " +
                              $"git push -o merge_request.create origin MPSH-upgrade-dto-{hash} && " +
                              $"git switch master && " +
                              $"git branch -D MPSH-upgrade-dto-{hash}";

                              var info = new ProcessStartInfo
                              {
                                  FileName = "cmd.exe",
                                  WorkingDirectory = solutionDirectory,
                                  ArgumentList = {$"/C {cmd}"}
                              };
                    System.Diagnostics.Process.Start(info);
                }
            }

            Console.WriteLine("Press enter to continue.");
            Console.WriteLine("Fixed following project files:");
            Console.ReadLine();
        }


        static void FixDuplicates()
        {
            var searchCriteria = new FileSearchCriteria
            {
                Extensions = new string[] { ".csproj" },
                BaseDirectory = "D:\\Projects",
                TargetDirectories = new string[] { "sibur" },
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
                        Console.WriteLine($"> Fixed: {project}.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            Console.WriteLine("Press enter to continue.");
            Console.WriteLine("Fixed following project files:");
            Console.ReadLine();

            Console.WriteLine(string.Join("\n", result));
            Console.ReadLine();
        }
    }
}
