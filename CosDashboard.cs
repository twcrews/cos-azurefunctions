﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Collections;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;

namespace Cos.AzureFunctions;

public class CosDashboard(ILogger<CosDashboard> logger, IHttpClientFactory factory)
{
	private readonly ILogger _logger = logger;
	private readonly HttpClient _apiClient = factory.CreateClient(HttpClientName.Api);
	private readonly HttpClient _avatarsClient = factory.CreateClient(HttpClientName.Avatars);
	private readonly HttpClient _gitHubClient = factory.CreateClient(HttpClientName.GitHub);

    [Function(AzureFunctions.Dashboard.Name)]
	public async Task<IActionResult> DashboardProxy(
		[HttpTrigger(
			AuthorizationLevel.Function,
			"get",
			"post",
			Route = AzureFunctions.Dashboard.Route)] HttpRequest request)
	{
		try
		{
			string path = GetRequestPathAndQuery(request, AzureFunctions.Dashboard.Name);
			_logger.LogInformation($"Processed request to path: {path}");

			HttpResponseMessage response;
			if (string.Equals(request.Method, "get", StringComparison.InvariantCultureIgnoreCase))
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

	[Function(AzureFunctions.Avatars.Name)]
	public async Task<IActionResult> AvatarProxy(
		[HttpTrigger(
			AuthorizationLevel.Function,
			"get",
			Route = AzureFunctions.Avatars.Route)] HttpRequest request)
	{
		try
		{
			string path = GetRequestPathAndQuery(request, AzureFunctions.Avatars.Name);
			_logger.LogInformation($"Processed Avatar request to path: {path}");

			HttpResponseMessage response = await _avatarsClient.GetAsync(path);
			return new FileStreamResult(await response.Content.ReadAsStreamAsync(), $"image/{path[^3..]}");
		}
		catch (Exception exception)
		{
			return SecureExceptionResult(exception);
		}
	}

	[Function(AzureFunctions.Calendar.Name)]
	public IActionResult Calendar(
		[HttpTrigger(
			AuthorizationLevel.Function,
			"get",
			Route = AzureFunctions.Calendar.Route)] HttpRequest request,
			string calendarName)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(calendarName))
			{
				_logger.LogInformation("Processed request for list of calendars.");
				return new OkObjectResult(Directory.GetFiles(Paths.Calendar)
					.Select(f => Path.GetFileNameWithoutExtension(f)));
			}
			_logger.LogInformation($"Processed request for calendar \"{calendarName}\".");
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

	[Function(AzureFunctions.Diagnostic.FileShare.Name)]
	public IActionResult FileShare(
		[HttpTrigger(
			AuthorizationLevel.Function,
			"get",
			Route = AzureFunctions.Diagnostic.FileShare.Route)] HttpRequest request)
	{
		try
		{
			string path = GetRequestPathAndQuery(request, AzureFunctions.Diagnostic.FileShare.Name);
			_logger.LogInformation($"Processed diagnostic request to fileshare path: {(string.IsNullOrWhiteSpace(path) ? "[root]" : path)}");

			string resultPath = $"{Environment.GetEnvironmentVariable(EnvironmentVariables.Paths.Resources) ?? ""}/{path}";
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

	[Function(AzureFunctions.Versioning.HeadHash.Name)]
	public async Task<IActionResult> HeadHash(
		[HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request)
	{
		string rateLimitHeaderKey = "X-RateLimit-Limit";
		string remainingHeaderKey = "X-RateLimit-Remaining";
		try
		{
			_logger.LogInformation($"Processed request for latest client head hash");
			
			HttpResponseMessage response = await _gitHubClient.GetAsync(Paths.HeadHash);

			string rateLimitHeader = "";
			string remainingLimitHeader = "";

			if (response.Headers.TryGetValues(rateLimitHeaderKey, out IEnumerable<string>? values))
			{
				rateLimitHeader = values.FirstOrDefault() ?? "";
			}
			if (response.Headers.TryGetValues(remainingHeaderKey, out values))
			{
				remainingLimitHeader = values.FirstOrDefault() ?? "";
			}

			request.HttpContext.Response.Headers.Append(rateLimitHeaderKey, rateLimitHeader);
			request.HttpContext.Response.Headers.Append(remainingHeaderKey, remainingLimitHeader);

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
		string appID = variables[EnvironmentVariables.PlanningCenter.Credentials.AppID] as string ?? "appId";
		string secret = variables[EnvironmentVariables.PlanningCenter.Credentials.Secret] as string ?? "secret";

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