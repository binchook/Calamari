using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Calamari.Kubernetes.ResourceStatus.Resources;

public class EndpointSlice : Resource
{
    public IEnumerable<string> Endpoints { get; }
    
    // There isn't really a failed or in-progress state for an EndpointSlice
    public override ResourceStatus Status => ResourceStatus.Successful;

    public EndpointSlice(JObject json) : base(json)
    {
        Endpoints = Data.SelectTokens("$.endpoints[*].addresses[0]").Values<string>();
    }

    public override bool HasUpdate(Resource lastStatus)
    {
        var last = CastOrThrow<EndpointSlice>(lastStatus);
        return !last.Endpoints.SequenceEqual(Endpoints);
    }
}