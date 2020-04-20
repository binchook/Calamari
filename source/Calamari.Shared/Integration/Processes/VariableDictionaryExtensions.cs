using Calamari.Common.Variables;
using Octostache;
using SpecialVariables = Calamari.Deployment.SpecialVariables;

namespace Calamari.Integration.Processes
{
    public static class VariableDictionaryExtensions
    {
        public static void SetOutputVariable(this IVariables variables, string name, string value)
        {
            variables.Set(name, value);

            // And set the output-variables.
            // Assuming we are running in a step named 'DeployWeb' and are setting a variable named 'Foo'
            // then we will set Octopus.Action[DeployWeb].Output.Foo
            var actionName = variables.Get(ActionVariables.Name);

            if (string.IsNullOrWhiteSpace(actionName))
                return;

            var actionScopedVariable = SpecialVariables.GetOutputVariableName(actionName, name);

            variables.Set(actionScopedVariable, value);

            // And if we are on a machine named 'Web01'
            // Then we will set Octopus.Action[DeployWeb].Output[Web01].Foo
            var machineName = variables.Get(MachineVariables.Name);

            if (string.IsNullOrWhiteSpace(machineName))
                return;

            var machineIndexedVariableName = SpecialVariables.GetMachineIndexedOutputVariableName(actionName, machineName, name);
            variables.Set(machineIndexedVariableName, value);
        }

    }
}
