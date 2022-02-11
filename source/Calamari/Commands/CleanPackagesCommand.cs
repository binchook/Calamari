﻿using Calamari.Commands.Support;
using Calamari.Common.Commands;
using Calamari.Common.Plumbing.Deployment.PackageRetention;
using Calamari.Integration.Packages.Download;

namespace Calamari.Commands
{
    [Command("clean-packages", Description = "Apply retention to the package cache")]
    public class CleanPackagesCommand : Command
    {
        readonly IManagePackageCache journal;
        readonly IPackageDownloaderUtils packageUtils = new PackageDownloaderUtils();
        int cacheSizeMegaBytes;

        public CleanPackagesCommand(IManagePackageCache journal)
        {
            this.journal = journal;
            Options.Add("cacheSize=", "Maximum size of the package cache", v => cacheSizeMegaBytes = int.Parse(v));
        }

        public override int Execute(string[] commandLineArguments)
        {
            journal.ApplyRetention(packageUtils.RootDirectory, cacheSizeMegaBytes);
            return 0;
        }
    }
}