using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Globalization;
using System.Collections;
using System.IO;

namespace COS.Internal.Dashboard.AzureFunctions;

public class CosDashboard
{
	private readonly HttpClient _apiClient;
	private readonly HttpClient _avatarsClient;

	public CosDashboard(IHttpClientFactory factory)
	{
		_apiClient = factory.CreateClient(HttpClientNames.Api);
		_avatarsClient = factory.CreateClient(HttpClientNames.Avatars);
	}

	[FunctionName(AzureFunctions.Dashboard.Name)]
	public async Task<IActionResult> DashboardProxy(
		[HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = AzureFunctions.Dashboard.Route)] HttpRequest request,
		ILogger log)
	{
		try
		{
			string path = GetRequestPathAndQuery(request, AzureFunctions.Dashboard.Name);
			log.LogInformation($"Processed request to path: {path}");

			HttpResponseMessage response;
			if (request.Method.ToLowerInvariant() == "get")
			{
				response = await _apiClient.GetAsync(path);
			}
			else
			{
				response = await _apiClient.PostAsync(path, new StreamContent(request.Body));
			}
			string body = await response.Content.ReadAsStringAsync();

			return new OkObjectResult(body);
		}
		catch (Exception exception)
		{
			return SecureExceptionResult(exception);
		}
	}

	[FunctionName(AzureFunctions.Avatars.Name)]
	public async Task<IActionResult> AvatarProxy(
		[HttpTrigger(AuthorizationLevel.Function, "get", Route = AzureFunctions.Avatars.Route)] HttpRequest request,
		ILogger log)
	{
		try
		{
			string path = GetRequestPathAndQuery(request, AzureFunctions.Avatars.Name);
			log.LogInformation($"Processed Avatar request to path: {path}");

			HttpResponseMessage response = await _avatarsClient.GetAsync(path);
			return new FileStreamResult(await response.Content.ReadAsStreamAsync(), $"image/{path[^3..]}");
		}
		catch (Exception exception)
		{
			return SecureExceptionResult(exception);
		}
	}

	[FunctionName(AzureFunctions.Diagnostic.FileShare.Name)]
	public static IActionResult FileShare(
		[HttpTrigger(AuthorizationLevel.Function, "get", Route = AzureFunctions.Diagnostic.FileShare.Route)] HttpRequest request,
		ILogger log)
	{
		try
		{
			string path = GetRequestPathAndQuery(request, AzureFunctions.Diagnostic.FileShare.Name);
			log.LogInformation($"Processed diagnostic request to fileshare path: {(string.IsNullOrWhiteSpace(path) ? "[root]" : path)}");

			return new OkObjectResult(Directory.GetDirectories($"/mnt/resources/{path}"));
		}
		catch (Exception exception)
		{
			return SecureExceptionResult(exception);
		}
	}

	private static string GetRequestPathAndQuery(HttpRequest request, string functionName)
		=> $"{request.Path.Value}{request.QueryString}"
				.Replace($"/api/{functionName}/", "", StringComparison.InvariantCultureIgnoreCase);

	private static ObjectResult SecureExceptionResult(Exception exception)
	{
		IDictionary variables = Environment.GetEnvironmentVariables();
		string appID = variables[EnvironmentVariables.PlanningCenter.Credentials.AppID] as string;
		string secret = variables[EnvironmentVariables.PlanningCenter.Credentials.Secret] as string;

		string message = exception.Message
			.Replace(appID, "*****", true, CultureInfo.InvariantCulture)
			.Replace(secret, "*****", true, CultureInfo.InvariantCulture);

		ObjectResult response = new(message)
		{
			StatusCode = StatusCodes.Status500InternalServerError
		};
		return response;
	}
}

