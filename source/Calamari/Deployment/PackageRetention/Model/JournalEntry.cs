﻿using System;
using System.Collections.Generic;
using System.Linq;
using Calamari.Common.Plumbing.Deployment.PackageRetention;
using Calamari.Deployment.PackageRetention.Caching;
using Newtonsoft.Json;

namespace Calamari.Deployment.PackageRetention.Model
{
    public class JournalEntry
    {
        public PackageIdentity Package { get; }
       // log.SetOutputVariableButDoNotAddToVariables("StagedPackage.Size", pkg.Size.ToString(CultureInfo.InvariantCulture));
        [JsonProperty]
        readonly PackageUsages usages;

        [JsonProperty]
        readonly PackageLocks locks;

        [JsonConstructor]
        public JournalEntry(PackageIdentity package, PackageLocks packageLocks = null, PackageUsages packageUsages = null)
        {
            Package = package ?? throw new ArgumentNullException(nameof(package));
            locks = packageLocks ?? new PackageLocks();
            usages = packageUsages ?? new PackageUsages();
        }

        public void AddUsage(ServerTaskId deploymentTaskId, CacheAge cacheAge)
        {
            usages.Add(new UsageDetails(deploymentTaskId, cacheAge));
        }

        public void AddLock(ServerTaskId deploymentTaskId, CacheAge cacheAge)
        {
            locks.Add(new UsageDetails(deploymentTaskId, cacheAge));
        }

        public void RemoveLock(ServerTaskId deploymentTaskId)
        {
            //We only remove the first lock that matches the deployment task id, in case we have a deployment that uses the same package twice.
            var usageLock = locks.FirstOrDefault(l => l.DeploymentTaskId == deploymentTaskId);
            locks.Remove(usageLock);
        }

        public bool HasLock() => locks.Count > 0;

        public IEnumerable<UsageDetails> GetUsageDetails() => usages;
        public IEnumerable<UsageDetails> GetLockDetails() => locks;
    }
}