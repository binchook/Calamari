﻿using System;
using Calamari.Common.Plumbing.Variables;
using Calamari.Deployment.PackageRetention;

namespace Calamari.Common.Plumbing.Deployment.PackageRetention
{
    public class PackageIdentity
    {
        public PackageID PackageID { get; }
        public string Version { get; }

        public PackageIdentity(string packageID, string version): this(new PackageID(packageID), version)
        {
        }

        public PackageIdentity(IVariables variables)
        {
            if (variables == null) throw new ArgumentNullException(nameof(variables));

            PackageID = new PackageID(variables.Get(PackageVariables.PackageId));
            Version = variables.Get(PackageVariables.PackageVersion);
        }

        public PackageIdentity(PackageID packageID, string version)
        {
            PackageID = packageID ?? throw new ArgumentNullException(nameof(packageID));
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj) || obj.GetType() != this.GetType())
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            var other = (PackageIdentity)obj;

            return this == other;
        }

        protected bool Equals(PackageIdentity other)
        {
            return Equals(PackageID, other.PackageID)
                   && Equals(Version, other.Version);
        }

        public override int GetHashCode()
        {
            unchecked
            {   //Generated by rider
                return ((PackageID != null ? PackageID.GetHashCode() : 0) * 397) ^ (Version != null ? Version.GetHashCode() : 0);
            }
        }

        public static bool operator == (PackageIdentity first, PackageIdentity second)
        {               
            return first.PackageID == second.PackageID && first.Version == second.Version;
        }

        public static bool operator !=(PackageIdentity first, PackageIdentity second)
        {
            return !(first == second);
        }
    }
}