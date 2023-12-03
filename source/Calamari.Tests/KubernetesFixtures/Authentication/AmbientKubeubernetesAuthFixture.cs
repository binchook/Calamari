using Calamari.Common.Features.Processes;
using Calamari.Common.Plumbing.Variables;
using Calamari.Testing.Helpers;
using NUnit.Framework;

namespace Calamari.Tests.KubernetesFixtures.Authentication
{
    [TestFixture]
    public class AmbientKubeubernetesAuthFixture
    {
        [Test]
        [Ignore("Still Need to get this working")]
        public void Foobar()
        {
            // kind create cluster --name kind-shoemaker
            // kubectl config use-context kind-kind-shoemaker
            //
            // kubectl apply -f testjob.yaml
            //  kubectl describe job testthing
            // kubectl get pods --selector=batch.kubernetes.io/job-name=testthing --output=jsonpath='{.items[*].metadata.name}'

            //CleanUpPreviousJobs();
            RunJon();
                
           // GetJobDetails();
            var log = new InMemoryLog();
            var runner = new CommandLineRunner(log, new CalamariVariables());

            var result = runner.Execute(new CommandLineInvocation("kubectl", new[] { "describe", "job","testthing" }));
            result.VerifySuccess();
        }


        const string job = "foobased";
        
        public void RunJon()
        {
            RunCommand("kubectl", new[] { "delete", "job", job });
            RunCommand("kubectl",
                       "create",
                       "job",
                       job,
                       "--image=mcr.microsoft.com/dotnet/sdk:7.0",
                       "--",
                       "dotnet",
                       "\"--version\"").Result.VerifySuccess();
            RunCommand("kubectl", "wait", "--for=condition=complete", $"job/{job}", "--timeout=60s").Result.VerifySuccess();
            
            var (logs, result) = RunCommand("kubectl", "logs", $"job/{job}");
            
            RunCommand("kubectl", new[] { "delete", "job", job });
        }
        
        /*
        public void GetJobDetails()
        {
            
            kubectl delete job my-job || true
            kubectl apply -f ./jobs/my-job.yaml
            kubectl wait --for=condition=complete job/my-job --timeout=60s
                echo "Job output:"
            kubectl logs job/my-job
                
                
                
            var (log, result) = RunCommand("kubectl", new[] { "describe", "job", "testthing" });
            result.VerifySuccess();
            
            kubectl delete pod foobarxx
            kubectl run foobarxx --image mcr.microsoft.com/dotnet/sdk:7.0 --restart Never  -- dotnet "--version"
            kubectl logs foobarxx
                
                
            kubectl delete job foobar || true
            kubectl create job foobar --image=mcr.microsoft.com/dotnet/sdk:7.0 -- dotnet "--version"
            kubectl wait --for=condition=complete job/foobar --timeout=60s
            kubectl logs job/foobar
                
            kubectl describe job foobar
            kubectl get pods --selector=batch.kubernetes.io/job-name=foobar --output=jsonpath='{.items[*].metadata.name}'
            kubectl logs foobar-rhqnc
            kubectl delete job foobar
        }*/

        public (InMemoryLog Log, CommandResult Result ) RunCommand(string executable, params string[] arguments)
        {
            var log = new InMemoryLog();
            var runner = new CommandLineRunner(log, new CalamariVariables());

            var result = runner.Execute(new CommandLineInvocation(executable, arguments));
            return (log, result);
        }
    }
}