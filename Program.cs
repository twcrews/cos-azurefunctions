using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Cos.AzureFunctions.Services;

IHost host = new HostBuilder()
	.ConfigureFunctionsWebApplication()
	.ConfigureServices(services => {
		services.AddHttpClient<IHttpProxyService, PlanningCenterProxyService>();
		services.AddHttpClient<IAvatarService, PlanningCenterAvatarService>();
		services.AddHttpClient<IVersionControlService, GitHubService>();
	})
	.Build();

host.Run();
