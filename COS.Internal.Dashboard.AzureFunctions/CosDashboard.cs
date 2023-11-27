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
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace COS.Internal.Dashboard.AzureFunctions;

public class CosDashboard
{
	private readonly HttpClient _apiClient;
	private readonly HttpClient _avatarsClient;
	private readonly HttpClient _gitHubClient;

	public CosDashboard(IHttpClientFactory factory)
	{
		_apiClient = factory.CreateClient(HttpClientName.Api);
		_avatarsClient = factory.CreateClient(HttpClientName.Avatars);
		_gitHubClient = factory.CreateClient(HttpClientName.GitHub);
	}

	[FunctionName(AzureFunctions.Dashboard.Name)]
	public async Task<IActionResult> DashboardProxy(
		[HttpTrigger(
			AuthorizationLevel.Function,
			"get",
			"post",
			Route = AzureFunctions.Dashboard.Route)] HttpRequest request,
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
		[HttpTrigger(
			AuthorizationLevel.Function,
			"get",
			Route = AzureFunctions.Avatars.Route)] HttpRequest request,
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

	[FunctionName(AzureFunctions.Calendar.Name)]
	public static IActionResult Calendar(
		[HttpTrigger(
			AuthorizationLevel.Function,
			"get",
			Route = AzureFunctions.Calendar.Route)] HttpRequest request,
			string calendarName,
		ILogger log)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(calendarName))
			{
				log.LogInformation("Processed request for list of calendars.");
				return new OkObjectResult(Directory.GetFiles(Paths.Calendar)
					.Select(f => Path.GetFileNameWithoutExtension(f)));
			}
			log.LogInformation($"Processed request for calendar \"{calendarName}\".");
			return new OkObjectResult(File.ReadAllText($"{Paths.Calendar}/{calendarName}.ics"));
		}
		catch (FileNotFoundException)
		{
			return new BadRequestObjectResult("No such calendar.");
		}
		catch (Exception exception)
		{
			return SecureExceptionResult(exception);
		}
	}

	[FunctionName(AzureFunctions.Diagnostic.FileShare.Name)]
	public static IActionResult FileShare(
		[HttpTrigger(
			AuthorizationLevel.Function,
			"get",
			Route = AzureFunctions.Diagnostic.FileShare.Route)] HttpRequest request,
		ILogger log)
	{
		try
		{
			string path = GetRequestPathAndQuery(request, AzureFunctions.Diagnostic.FileShare.Name);
			log.LogInformation($"Processed diagnostic request to fileshare path: {(string.IsNullOrWhiteSpace(path) ? "[root]" : path)}");

			string resultPath = $"{Paths.Resources}/{path}";
			return new OkObjectResult(new Dictionary<string, IEnumerable<string>>
				{
					{"directories", Directory.GetDirectories(resultPath).Select(i => Path.GetFileName(i))},
					{"files", Directory.GetFiles(resultPath).Select(i => Path.GetFileName(i))}
				});
		}
		catch (Exception exception)
		{
			if (exception is DirectoryNotFoundException || exception is FileNotFoundException)
			{
				return new BadRequestObjectResult("No such directory.");
			}
			return SecureExceptionResult(exception);
		}
	}

	[FunctionName(AzureFunctions.Versioning.HeadHash.Name)]
	public async Task<IActionResult> HeadHash(
		[HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request,
		ILogger log)
	{
		try
		{
			log.LogInformation($"Processed request for latest client head hash");
			
			HttpResponseMessage response = await _gitHubClient.GetAsync(Paths.HeadHash);
			string responseBody = await response.Content.ReadAsStringAsync();

			return new OkObjectResult(JsonDocument
				.Parse(responseBody).RootElement
				.GetProperty("sha")
				.GetString());
		}
		catch (Exception exception)
		{
			return SecureExceptionResult(exception);
		}
	}

	private static string GetRequestPathAndQuery(HttpRequest request, string functionName)
	{
		string basePath = $"/api/{functionName}";
		string fullRequestPath = $"{request.Path.Value}{request.QueryString}";

		if (fullRequestPath == basePath)
		{
			fullRequestPath = $"{fullRequestPath}/";
		}

		return fullRequestPath.Replace($"/api/{functionName}/", "", StringComparison.InvariantCultureIgnoreCase);
	}

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

