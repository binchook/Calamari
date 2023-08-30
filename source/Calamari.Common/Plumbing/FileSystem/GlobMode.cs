using Calamari.Common.FeatureToggles;
using Calamari.Common.Plumbing.Variables;

namespace Calamari.Common.Plumbing.FileSystem.GlobExpressions
{
    public enum GlobMode
    {
        LegacyMode,
        GroupExpansionMode
    }

    public static class GlobModeRetriever
    {
        public static GlobMode GetFromVariables(IVariables variables)
        {
            return FeatureToggle.GlobPathsGroupSupportInvertedFeatureToggle.IsEnabled(variables)
                ? GlobMode.LegacyMode
                : GlobMode.GroupExpansionMode;
        }
    }
}