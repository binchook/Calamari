using System.Collections.Generic;
using System.Linq;
using Calamari.Common.Plumbing.Logging;
using Calamari.Common.Plumbing.ServiceMessages;
using Calamari.Common.Plumbing.Variables;
using Calamari.ResourceStatus.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Calamari.ResourceStatus;

public static class ServiceMessages
{
    public static void Send(IEnumerable<Resource> resources, IVariables variables, ILog log)
    {
        var data = GenerateServiceMessageData(resources);

        var parameters = new Dictionary<string, string>
        {
            {"data", data},
            {"deploymentId", variables.Get("Octopus.Deployment.Id")},
            {"actionId", variables.Get("Octopus.Action.Id")}
        };

        var message = new ServiceMessage("kubernetes-deployment-status-update", parameters);
        log.WriteServiceMessage(message);
    }

    public static string GenerateServiceMessageData(IEnumerable<Resource> resources)
    {
        var result = resources
            .GroupBy(resource => resource.Kind)
            .ToDictionary(
                group => group.Key,
                group => group.Select(CreateEntry));
        return JsonConvert.SerializeObject(result);
    }

    private static MessageEntry CreateEntry(Resource resource)
    {
        var (status, message) = resource.CheckStatus();
        return new MessageEntry
        {
            Status = status,
            Message = message,
            Data = resource.RawJson
        };
    }

    public class MessageEntry
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ResourceStatus Status { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }
}