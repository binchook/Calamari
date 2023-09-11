#if NETCORE
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Calamari.Azure;
using Calamari.CloudAccounts;
using Calamari.Common.Features.Discovery;
using Calamari.Common.Plumbing.FileSystem;
using Calamari.Common.Plumbing.ServiceMessages;
using Calamari.Kubernetes.Commands;
using Calamari.Testing;
using Calamari.Testing.Helpers;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SpecialVariables = Calamari.Kubernetes.SpecialVariables;

namespace Calamari.Tests.KubernetesFixtures
{
    [TestFixture]
    [Category(TestCategory.RunOnceOnWindowsAndLinux)]
    public class KubernetesContextScriptWrapperLiveFixtureAks: KubernetesContextScriptWrapperLiveFixture
    {
        string aksClusterHost;
        string aksClusterClientCertificate;
        string aksClusterClientKey;
        string aksClusterCaCertificate;
        string aksClusterName;
        string azurermResourceGroup;
        string aksPodServiceAccountToken;
        string azureSubscriptionId;

        protected override string KubernetesCloudProvider => "AKS";

        protected override IEnumerable<string> ToolsToAddToPath(InstallTools tools)
        {
            yield return tools.KubeloginExecutable;
        }
        
        protected override async Task InstallOptionalTools(InstallTools tools)
        {
            await tools.InstallKubelogin();
        }

        protected override void ExtractVariablesFromTerraformOutput(JObject jsonOutput)
        {
            aksClusterHost = jsonOutput.Get<string>("aks_cluster_host", "value");
            aksClusterClientCertificate = jsonOutput.Get<string>("aks_cluster_client_certificate", "value");
            aksClusterClientKey = jsonOutput.Get<string>("aks_cluster_client_key", "value");
            aksClusterCaCertificate = jsonOutput.Get<string>("aks_cluster_ca_certificate", "value");
            aksClusterName = jsonOutput.Get<string>("aks_cluster_name", "value");
            aksPodServiceAccountToken = jsonOutput.Get<string>("aks_service_account_token", "value");
            azurermResourceGroup = jsonOutput.Get<string>("aks_rg_name", "value");
        }

        protected override Dictionary<string, string> GetEnvironmentVars()
        {
            azureSubscriptionId = ExternalVariables.Get(ExternalVariable.AzureSubscriptionId);
            return new Dictionary<string, string>()
            {
                { "ARM_SUBSCRIPTION_ID", azureSubscriptionId},
                { "ARM_CLIENT_ID", ExternalVariables.Get(ExternalVariable.AzureSubscriptionClientId) },
                { "ARM_CLIENT_SECRET", ExternalVariables.Get(ExternalVariable.AzureSubscriptionPassword) },
                { "ARM_TENANT_ID", ExternalVariables.Get(ExternalVariable.AzureSubscriptionTenantId) },
                { "TF_VAR_aks_client_id", ExternalVariables.Get(ExternalVariable.AzureSubscriptionClientId) },
                { "TF_VAR_aks_client_secret", ExternalVariables.Get(ExternalVariable.AzureSubscriptionPassword) },
                { "TF_VAR_test_namespace", TestNamespace },
            };
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void AuthorisingWithPodServiceAccountToken(bool runAsScript)
        {
            variables.Set(SpecialVariables.ClusterUrl, aksClusterHost);

            using (var dir = TemporaryDirectory.Create())
            using (var podServiceAccountToken = new TemporaryFile(Path.Combine(dir.DirectoryPath, "podServiceAccountToken")))
            using (var certificateAuthority = new TemporaryFile(Path.Combine(dir.DirectoryPath, "certificateAuthority")))
            {
                File.WriteAllText(podServiceAccountToken.FilePath, aksPodServiceAccountToken);
                File.WriteAllText(certificateAuthority.FilePath, aksClusterCaCertificate);
                variables.Set("Octopus.Action.Kubernetes.PodServiceAccountTokenPath", podServiceAccountToken.FilePath);
                variables.Set("Octopus.Action.Kubernetes.CertificateAuthorityPath", certificateAuthority.FilePath);

                if (runAsScript)
                {
                    DeployWithKubectlTestScriptAndVerifyResult();
                }
                else
                {
                    ExecuteCommandAndVerifyResult(TestableKubernetesDeploymentCommand.Name);
                }
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void AuthorisingWithAzureServicePrincipal(bool runAsScript)
        {
            variables.Set(Deployment.SpecialVariables.Account.AccountType, "AzureServicePrincipal");
            variables.Set("Octopus.Action.Kubernetes.AksClusterResourceGroup", azurermResourceGroup);
            variables.Set(SpecialVariables.AksClusterName, aksClusterName);
            variables.Set("Octopus.Action.Kubernetes.AksAdminLogin", Boolean.FalseString);
            variables.Set("Octopus.Action.Azure.SubscriptionId", ExternalVariables.Get(ExternalVariable.AzureSubscriptionId));
            variables.Set("Octopus.Action.Azure.TenantId", ExternalVariables.Get(ExternalVariable.AzureSubscriptionTenantId));
            variables.Set("Octopus.Action.Azure.Password", ExternalVariables.Get(ExternalVariable.AzureSubscriptionPassword));
            variables.Set("Octopus.Action.Azure.ClientId", ExternalVariables.Get(ExternalVariable.AzureSubscriptionClientId));
            if (runAsScript)
            {
                DeployWithKubectlTestScriptAndVerifyResult();
            }
            else
            {
                ExecuteCommandAndVerifyResult(TestableKubernetesDeploymentCommand.Name);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void AuthorisingWithClientCertificate(bool runAsScript)
        {
            variables.Set(SpecialVariables.ClusterUrl, aksClusterHost);
            var certificateAuthority = "myauthority";
            variables.Set("Octopus.Action.Kubernetes.CertificateAuthority", certificateAuthority);
            variables.Set($"{certificateAuthority}.CertificatePem", aksClusterCaCertificate);
            var clientCert = "myclientcert";
            variables.Set("Octopus.Action.Kubernetes.ClientCertificate", clientCert);
            variables.Set($"{clientCert}.CertificatePem", aksClusterClientCertificate);
            variables.Set($"{clientCert}.PrivateKeyPem", aksClusterClientKey);
            if (runAsScript)
            {
                DeployWithKubectlTestScriptAndVerifyResult();
            }
            else
            {
                ExecuteCommandAndVerifyResult(TestableKubernetesDeploymentCommand.Name);
            }
        }

        [Test]
        public void UnreachableK8Cluster_ShouldExecuteTargetScript()
        {
            const string certificateAuthority = "myauthority";
            const string unreachableClusterUrl = "https://example.kubernetes.com";
            const string clientCert = "myclientcert";

            variables.Set(SpecialVariables.ClusterUrl, unreachableClusterUrl);
            variables.Set("Octopus.Action.Kubernetes.CertificateAuthority", certificateAuthority);
            variables.Set($"{certificateAuthority}.CertificatePem", aksClusterCaCertificate);
            variables.Set("Octopus.Action.Kubernetes.ClientCertificate", clientCert);
            variables.Set($"{clientCert}.CertificatePem", aksClusterClientCertificate);
            variables.Set($"{clientCert}.PrivateKeyPem", aksClusterClientKey);

            DeployWithNonKubectlTestScriptAndVerifyResult();
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void DiscoverKubernetesClusterWithAzureServicePrincipalAccount(bool setHealthCheckContainer)
        {
            var scope = new TargetDiscoveryScope("TestSpace",
                "Staging",
                "testProject",
                null,
                new[] { "discovery-role" },
                "WorkerPool-1",
                setHealthCheckContainer ? new FeedImage("MyImage:with-tag", "Feeds-123") : null);

            var account = new AzureServicePrincipalAccount(
                                                           ExternalVariables.Get(ExternalVariable.AzureSubscriptionId),
                                                           ExternalVariables.Get(ExternalVariable.AzureSubscriptionClientId),
                                                           ExternalVariables.Get(ExternalVariable.AzureSubscriptionTenantId),
                                                           ExternalVariables.Get(ExternalVariable.AzureSubscriptionPassword),
                                                           null,
                                                           null,
                                                           null);

            var authenticationDetails =
                new AccountAuthenticationDetails<AzureServicePrincipalAccount>(
                                                                               "AzureServicePrincipal",
                                                                               "Accounts-1",
                                                                               account);

            var targetDiscoveryContext =
                new TargetDiscoveryContext<AccountAuthenticationDetails<AzureServicePrincipalAccount>>(scope,
                                                                                                       authenticationDetails);

            ExecuteDiscoveryCommandAndVerifyResult(targetDiscoveryContext);

            var targetName = $"aks/{azureSubscriptionId}/{azurermResourceGroup}/{aksClusterName}";
            var serviceMessageProperties = new Dictionary<string, string>
            {
                { "name", targetName },
                { "clusterName", aksClusterName },
                { "clusterResourceGroup", azurermResourceGroup },
                { "skipTlsVerification", bool.TrueString },
                { "octopusDefaultWorkerPoolIdOrName", scope.WorkerPoolId },
                { "octopusAccountIdOrName", "Accounts-1" },
                { "octopusRoles", "discovery-role" },
                { "updateIfExisting", bool.TrueString },
                { "isDynamic", bool.TrueString },
                { "awsUseWorkerCredentials", bool.FalseString },
                { "awsAssumeRole", bool.FalseString }
            };

            if (scope.HealthCheckContainer != null)
            {
                serviceMessageProperties.Add("healthCheckContainerImageFeedIdOrName", scope.HealthCheckContainer.FeedIdOrName);
                serviceMessageProperties.Add("healthCheckContainerImage", scope.HealthCheckContainer.ImageNameAndTag);
            }

            var expectedServiceMessage = new ServiceMessage(
                KubernetesDiscoveryCommand.CreateKubernetesTargetServiceMessageName,
                serviceMessageProperties);

            Log.ServiceMessages.Should()
                .ContainSingle(s => s.Properties.ContainsKey("name") && s.Properties["name"] == targetName)
                .Which.Should()
                .BeEquivalentTo(expectedServiceMessage);
        }
    }
}
#endif