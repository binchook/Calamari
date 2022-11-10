﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Calamari.Common.Features.Processes;
using Calamari.Common.Plumbing;
using Calamari.Common.Plumbing.Logging;
using Calamari.Common.Plumbing.Variables;

namespace Calamari.Kubernetes.Integration
{
    class Kubectl : CommandLineTool
    {
        string kubectl;
        readonly IVariables variables;

        public string ExecutableLocation => kubectl;

        public Kubectl(IVariables variables, string kubectl, ILog log, ICommandLineRunner commandLineRunner, string workingDirectory, Dictionary<string, string> environmentVars, Dictionary<string, string> redactMap)
            : base(log, commandLineRunner, workingDirectory, environmentVars, redactMap)
        {
            this.variables = variables;
            this.kubectl = kubectl;
        }

        public bool TrySetKubectl()
        {
            kubectl = variables.Get("Octopus.Action.Kubernetes.CustomKubectlExecutable");
            if (string.IsNullOrEmpty(kubectl))
            {
                kubectl = CalamariEnvironment.IsRunningOnWindows
                    ? ExecuteCommandAndReturnOutput("where", "kubectl.exe").FirstOrDefault()
                    : ExecuteCommandAndReturnOutput("which", "kubectl").FirstOrDefault();

                if (string.IsNullOrEmpty(kubectl))
                {
                    log.Error("Could not find kubectl. Make sure kubectl is on the PATH. See https://g.octopushq.com/KubernetesTarget for more information.");
                    return false;
                }

                kubectl = kubectl.Trim();
            }
            else if (!File.Exists(kubectl))
            {
                log.Error($"The custom kubectl location of {kubectl} does not exist. See https://g.octopushq.com/KubernetesTarget for more information.");
                return false;
            }

            if (TryExecuteKubectlCommand("version", "--client", "--short"))
            {
                return true;
            }

            log.Error($"Could not find kubectl. Make sure {kubectl} is on the PATH. See https://g.octopushq.com/KubernetesTarget for more information.");
            return false;
        }

        bool TryExecuteKubectlCommand(params string[] arguments)
        {
            return ExecuteCommandAndLogOutput(new CommandLineInvocation(kubectl, arguments.Concat(new[] { "--request-timeout=1m" }).ToArray())).ExitCode == 0;
        }
    }
}
