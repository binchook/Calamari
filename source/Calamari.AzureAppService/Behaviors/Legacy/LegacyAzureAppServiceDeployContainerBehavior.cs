﻿using System;
using System.Threading.Tasks;
using Calamari.AzureAppService.Azure;
using Calamari.CloudAccounts;
using Calamari.Common.Commands;
using Calamari.Common.FeatureToggles;
using Calamari.Common.Plumbing.Logging;
using Calamari.Common.Plumbing.Pipeline;
using Microsoft.Azure.Management.WebSites;
using Microsoft.Rest;
using Octopus.CoreUtilities.Extensions;
using AccountVariables = Calamari.AzureAppService.Azure.AccountVariables;

namespace Calamari.AzureAppService.Behaviors
{
    class LegacyAzureAppServiceDeployContainerBehavior : IDeployBehaviour
    {
        private ILog Log { get; }
        public LegacyAzureAppServiceDeployContainerBehavior(ILog log)
        {
            Log = log;
        }

        public bool IsEnabled(RunningDeployment context) => !FeatureToggle.ModernAzureAppServiceSdkFeatureToggle.IsEnabled(context.Variables);

        public async Task Execute(RunningDeployment context)
        {
            var variables = context.Variables;

            var hasJwt = !variables.Get(AccountVariables.Jwt).IsNullOrEmpty();
            var account = hasJwt ? (IAzureAccount)new AzureOidcAccount(variables) : new AzureServicePrincipalAccount(variables);
            var webAppName = variables.Get(SpecialVariables.Action.Azure.WebAppName);
            var slotName = variables.Get(SpecialVariables.Action.Azure.WebAppSlot);
            var rgName = variables.Get(SpecialVariables.Action.Azure.ResourceGroupName);
            var targetSite = new AzureTargetSite(account.SubscriptionNumber, rgName, webAppName, slotName);

            var image = variables.Get(SpecialVariables.Action.Package.Image);
            var registryHost = variables.Get(SpecialVariables.Action.Package.Registry);
            var regUsername = variables.Get(SpecialVariables.Action.Package.Feed.Username);
            var regPwd = variables.Get(SpecialVariables.Action.Package.Feed.Password);

            var token = await Auth.GetAuthTokenAsync(account);

            var webAppClient = new WebSiteManagementClient(new Uri(account.ResourceManagementEndpointBaseUri),
                    new TokenCredentials(token))
                {SubscriptionId = account.SubscriptionNumber};

            Log.Info($"Updating web app to use image {image} from registry {registryHost}");

            Log.Verbose("Retrieving config (this is required to update image)");
            var config = await webAppClient.WebApps.GetConfigurationAsync(targetSite);
            config.LinuxFxVersion = $@"DOCKER|{image}";


            Log.Verbose("Retrieving app settings");
            var appSettings = await webAppClient.WebApps.ListApplicationSettingsAsync(targetSite);

            appSettings.Properties["DOCKER_REGISTRY_SERVER_URL"] = "https://" + registryHost;
            appSettings.Properties["DOCKER_REGISTRY_SERVER_USERNAME"] = regUsername;
            appSettings.Properties["DOCKER_REGISTRY_SERVER_PASSWORD"] = regPwd;

            Log.Info("Updating app settings with container registry");
            await webAppClient.WebApps.UpdateApplicationSettingsAsync(targetSite, appSettings);

            Log.Info("Updating configuration with container image");
            await webAppClient.WebApps.UpdateConfigurationAsync(targetSite, config);
        }
    }
}
