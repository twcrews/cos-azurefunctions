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

namespace COS.Internal.Dashboard.AzureFunctions
{
	public class CosDashboard
	{
		private static readonly HttpClient _client = new();
		private const string FunctionName = "CosDashboard";

		public CosDashboard()
		{
			IDictionary variables = Environment.GetEnvironmentVariables();

			string appID = variables["PLANNING_CENTER_APP_ID"] as string;
			string secret = variables["PLANNING_CENTER_SECRET"] as string;

			string encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{appID}:{secret}"));
			_client.DefaultRequestHeaders.Authorization = new("Basic", encodedCredentials);

			_client.BaseAddress = new(variables["PLANNING_CENTER_API_URL"] as string);
		}

		[FunctionName(FunctionName)]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "get", Route = $"{FunctionName}/{{*remainder}}")] HttpRequest request,
			ILogger log)
		{
			string path = request.Path.Value
				.Replace($"/api/{FunctionName}/", "", StringComparison.InvariantCultureIgnoreCase);
			log.LogInformation($"Processed a request to the following path: {path}");

			HttpResponseMessage response = await _client.GetAsync(path);
			string body = await response.Content.ReadAsStringAsync();

			return new OkObjectResult(body);
		}
	}
}

