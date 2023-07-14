﻿using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Calamari.AzureAppService
{
    internal class Auth
    {
        public static async Task<string> GetAuthTokenAsync(string tenantId, string applicationId, string password, string managementEndPoint, string activeDirectoryEndPoint)
        { 
            var authContext = GetContextUri(activeDirectoryEndPoint, tenantId);

            var app = ConfidentialClientApplicationBuilder.Create(applicationId)
                                                          .WithClientSecret(password)
                                                          .WithAuthority(authContext)
                                                          .Build();

            var result = await app.AcquireTokenForClient(
                                                         new [] { $"{managementEndPoint}/.default" })
                                  .WithTenantId(tenantId)
                                  .ExecuteAsync()
                                  .ConfigureAwait(false);
            return result.AccessToken;
        }

        static string GetContextUri(string activeDirectoryEndPoint, string tenantId)
        {
            if (!activeDirectoryEndPoint.EndsWith("/"))
            {
                return $"{activeDirectoryEndPoint}/{tenantId}";
            }
            return $"{activeDirectoryEndPoint}{tenantId}";
        }
    }
}
