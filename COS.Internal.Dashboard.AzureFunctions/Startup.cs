using System;
using System.Collections;
using System.Text;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(COS.Internal.Dashboard.AzureFunctions.Startup))]
namespace COS.Internal.Dashboard.AzureFunctions;

public class Startup : FunctionsStartup
{
	public override void Configure(IFunctionsHostBuilder builder)
	{
		builder.Services.AddHttpClient(Constants.HttpClientName, client =>
		{
			IDictionary variables = Environment.GetEnvironmentVariables();

			string appID = variables[Constants.PlanningCenterAppID] as string;
			string secret = variables[Constants.PlanningCenterSecret] as string;
			string encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{appID}:{secret}"));

			client.BaseAddress = new(variables[Constants.PlanningCenterApiUrl] as string);
			client.DefaultRequestHeaders.Authorization = new("Basic", encodedCredentials);
		});
	}
}
