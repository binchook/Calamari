﻿using Autofac;
using Octopus.Server.Extensibility.Extensions.Mappings;
using Sashimi.AzureCloudService.Endpoints;
using Sashimi.Server.Contracts.Accounts;
using Sashimi.Server.Contracts.ActionHandlers;
using Sashimi.Server.Contracts.Endpoints;

namespace Sashimi.AzureCloudService
{
    public class AzureCloudServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<AzureSubscriptionTypeProvider>().As<IAccountTypeProvider>().As<IContributeMappings>().SingleInstance();
            builder.RegisterType<AzureCertificateMayBeAutoGenerated>().As<AccountStoreContributor>().SingleInstance();
            builder.RegisterType<AzureCertificateRequiresPrivateKey>().As<AccountStoreContributor>().SingleInstance();
            builder.RegisterType<AzureCertificateThumbprintWillBeSet>().As<AccountStoreContributor>().SingleInstance();
            builder.RegisterType<CertificateEncoder>().SingleInstance();
            builder.RegisterType<CertificateGenerator>().SingleInstance();
            builder.RegisterType<AzureCloudServiceHealthCheckActionHandler>().As<IActionHandler>().AsSelf()
                   .InstancePerLifetimeScope();
            builder.RegisterType<AzureCloudServiceServiceMessageHandler>().AsSelf()
                   .InstancePerLifetimeScope();
            builder.RegisterType<AzureCloudServiceDeploymentTargetTypeProvider>()
                   .As<IDeploymentTargetTypeProvider>()
                   .As<IContributeMappings>()
                   .SingleInstance();
        }
    }
}