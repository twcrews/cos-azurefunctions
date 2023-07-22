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

namespace COS.Internal.Dashboard.AzureFunctions;

public class CosDashboard
{
	private readonly HttpClient _client;
	public CosDashboard(IHttpClientFactory factory) => _client = factory.CreateClient(Constants.HttpClientName);

	[FunctionName(Constants.DashboardFunctionName)]
	public async Task<IActionResult> Run(
		[HttpTrigger(AuthorizationLevel.Function, "get", Route = Constants.DashboardFunctionRoute)] HttpRequest request,
		ILogger log)
	{
		try
		{
			string path = $"{request.Path.Value}{request.QueryString}"
				.Replace($"/api/{Constants.DashboardFunctionName}/", "", StringComparison.InvariantCultureIgnoreCase);
			log.LogInformation($"Processed a request to the following path: {path}");

			HttpResponseMessage response = await _client.GetAsync(path);
			string body = await response.Content.ReadAsStringAsync();

			return new OkObjectResult(body);
		} 
		catch (Exception exception)
		{
			IDictionary variables = Environment.GetEnvironmentVariables();
			string appID = variables[Constants.PlanningCenterAppID] as string;
			string secret = variables[Constants.PlanningCenterSecret] as string;

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
}

