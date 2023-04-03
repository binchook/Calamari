using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Calamari.Kubernetes.ResourceStatus.Resources
{
    public static class ResourceFactory
    {
        public static Resource FromJson(string json) => FromJObject(JObject.Parse(json));
        
        public static IEnumerable<Resource> FromListJson(string json)
        {
            var listResponse = JObject.Parse(json);
            return listResponse.SelectTokens("$.items[*]").Select(item => FromJObject((JObject)item));
        }
        
        public static Resource FromJObject(JObject data)
        {
            var kind = data.SelectToken("$.kind")?.Value<string>();
            switch (kind)
            {   
                case "Pod": 
                    return new Pod(data);
                case "ReplicaSet": 
                    return new ReplicaSet(data);
                case "Deployment":
                    return new Deployment(data);
                case "StatefulSet":
                    return new StatefulSet(data);
                case "DaemonSet":
                    return new DaemonSet(data);
                case "Job":
                    return new Job(data);
                case "CronJob":
                    return new CronJob(data);
                case "Service": 
                    return new Service(data);
                case "Ingress":
                    return new Ingress(data);
                case "EndpointSlice": 
                    return new EndpointSlice(data); 
                case "ConfigMap":
                    return new ConfigMap(data);
                case "Secret":
                    return new Secret(data);
                case "PersistentVolumeClaim":
                    return new PersistentVolumeClaim(data);
                default:
                    return new Resource(data);
            }
        }
    }
}