using System;
using System.Collections;
using System.Text;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Cos.AzureFunctions.Startup))]
namespace Cos.AzureFunctions;

public class Startup : FunctionsStartup
{
	public override void Configure(IFunctionsHostBuilder builder)
	{
		IDictionary variables = Environment.GetEnvironmentVariables();

		builder.Services.AddHttpClient(HttpClientName.Api, client =>
		{
			string appID = variables[EnvironmentVariables.PlanningCenter.Credentials.AppID] as string;
			string secret = variables[EnvironmentVariables.PlanningCenter.Credentials.Secret] as string;
			string encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{appID}:{secret}"));

			client.BaseAddress = new(variables[EnvironmentVariables.PlanningCenter.BaseUrls.Api] as string);
			client.DefaultRequestHeaders.Authorization = new("Basic", encodedCredentials);
		});
		builder.Services.AddHttpClient(HttpClientName.Avatars, client =>
			client.BaseAddress = new(variables[EnvironmentVariables.PlanningCenter.BaseUrls.Avatars] as string));
		builder.Services.AddHttpClient(HttpClientName.GitHub, client =>
		{
			client.BaseAddress = new(variables[EnvironmentVariables.GitHub.BaseUrl] as string);
			client.DefaultRequestHeaders.UserAgent.ParseAdd(
				variables[EnvironmentVariables.GitHub.UserAgent] as string);
			client.DefaultRequestHeaders.Authorization = new(
				"Bearer", variables[EnvironmentVariables.GitHub.ApiToken] as string);
		});
	}
}
