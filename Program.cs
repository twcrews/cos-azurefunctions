using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using Cos.AzureFunctions;
using System.Text;


IDictionary variables = Environment.GetEnvironmentVariables();

IHost host = new HostBuilder()
	.ConfigureFunctionsWebApplication()
	.ConfigureServices(services => {
		services.AddHttpClient(HttpClientName.Api, client =>
		{
			string? appID = variables[EnvironmentVariables.PlanningCenter.Credentials.AppID] as string;
			string? secret = variables[EnvironmentVariables.PlanningCenter.Credentials.Secret] as string;
			string encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{appID}:{secret}"));

			client.BaseAddress = new(
				(variables[EnvironmentVariables.PlanningCenter.BaseUrls.Api] as string) ?? "localhost/");
			client.DefaultRequestHeaders.Authorization = new("Basic", encodedCredentials);
		});
		services.AddHttpClient(HttpClientName.Avatars, client =>
			client.BaseAddress = new(
				(variables[EnvironmentVariables.PlanningCenter.BaseUrls.Avatars] as string) ?? "localhost/"));
		services.AddHttpClient(HttpClientName.GitHub, client =>
		{
			client.BaseAddress = new((variables[EnvironmentVariables.GitHub.BaseUrl] as string) ?? "localhost/");
			client.DefaultRequestHeaders.UserAgent.ParseAdd(
				variables[EnvironmentVariables.GitHub.UserAgent] as string);
			client.DefaultRequestHeaders.Authorization = new(
				"Bearer", variables[EnvironmentVariables.GitHub.ApiToken] as string);
		});
	})
	.Build();

host.Run();
