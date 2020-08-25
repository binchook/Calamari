﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sashimi.Template.Wrangler
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string packageVersion = null;
            string installPath = null;

            if (args.Length < 2)
            {
                throw new Exception($"Invalid operation, Usage {typeof(Program).Namespace} <packageVersion> <installPath>");
            }

            if (args.Length == 2)
            {
                packageVersion = args[0];
                installPath = args[1];
            }

            var solutionPath = Path.GetFullPath(Path.Combine(installPath, "source"));

            var csProjFiles = Directory.EnumerateFiles(solutionPath, "*.csproj", SearchOption.AllDirectories);
            var packagesToAdd = new Dictionary<string, string>();

            ProcessUtils.ReadFromDotNetCommand("new", solutionPath, "sln -n Sashimi.NamingIsHard");

            foreach (var projFile in csProjFiles)
            {
                var sb = new StringBuilder();

                foreach (var line in File.ReadLines(projFile))
                {
                    if (!(line.Contains("<ProjectReference Include=\"..\\..\\") || line.Contains("<ProjectReference Include=\"../../")))
                    {
                        sb.AppendLine(line);
                        continue;
                    }

                    var startIndex = line.IndexOf('"');
                    var projectReferencePath = line.Substring(startIndex + 1, line.LastIndexOf('"') - startIndex);

                    var packageId = Path.GetFileNameWithoutExtension(projectReferencePath);

                    packagesToAdd.Add(projFile, packageId);
                }

                await File.WriteAllTextAsync(projFile, sb.ToString());
                ProcessUtils.ReadFromDotNetCommand("sln", solutionPath, $"Sashimi.NamingIsHard.sln add \"{projFile}\"");
            }

            foreach (var (projFile, packageId) in packagesToAdd)
            {
                ProcessUtils.ReadFromDotNetCommand("add", solutionPath, $"{projFile} package -n -v {packageVersion} {packageId}");
            }
        }
    }
}