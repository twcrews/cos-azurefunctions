using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Collections;
using System.Text;
using System.Globalization;

namespace COS.Internal.Dashboard.AzureFunctions
{
	public class CosDashboard
	{
		private static readonly HttpClient _client = new();
		private const string FunctionName = "CosDashboard";
		private readonly string _appID;
		private readonly string _secret;

		public CosDashboard()
		{
			IDictionary variables = Environment.GetEnvironmentVariables();

			_appID = variables["PLANNING_CENTER_APP_ID"] as string;
			_secret = variables["PLANNING_CENTER_SECRET"] as string;

			string encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_appID}:{_secret}"));
			_client.DefaultRequestHeaders.Authorization = new("Basic", encodedCredentials);

			_client.BaseAddress = new(variables["PLANNING_CENTER_API_URL"] as string);
		}

		[FunctionName(FunctionName)]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "get", Route = $"{FunctionName}/{{*remainder}}")] HttpRequest request,
			ILogger log)
		{
			try
			{
				string path = request.Path.Value
					.Replace($"/api/{FunctionName}/", "", StringComparison.InvariantCultureIgnoreCase);
				log.LogInformation($"Processed a request to the following path: {path}");

				HttpResponseMessage response = await _client.GetAsync(path);
				string body = await response.Content.ReadAsStringAsync();

				return new OkObjectResult(body);
			} 
			catch (Exception exception)
			{
				string message = exception.Message
					.Replace(_appID, "*****", true, CultureInfo.InvariantCulture)
					.Replace(_secret, "*****", true, CultureInfo.InvariantCulture);

				ObjectResult response = new(message);
				response.StatusCode = StatusCodes.Status500InternalServerError;
				return response;
			}
		}
	}
}

