using System;
using Sashimi.Server.Contracts.ActionHandlers;
using System.Collections.Generic;
using FluentValidation;
using Sashimi.Server.Contracts.Accounts;

namespace Sashimi.Server.Contracts.Endpoints
{
    public interface IDeploymentTargetTypeProvider
    {
        DeploymentTargetType DeploymentTargetType { get; }
        Type DomainType { get; }
        Type ApiType { get; }
        IActionHandler HealthCheckActionHandlerForTargetType();
        IValidator Validator { get; }
        IEnumerable<AccountType> SupportedAccountTypes { get; }
        IEnumerable<(string key, object value)> GetMetric(IEndpointMetricContext context);
    }
}