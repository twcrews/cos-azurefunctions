using System.Collections;
using System.Text;
using Cos.AzureFunctions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cos.AzureFunctions.Services;

public class PlanningCenterProxyService : IHttpProxyService
{
	private readonly HttpClient _httpClient;

	public PlanningCenterProxyService(HttpClient client)
	{
		IDictionary variables = Environment.GetEnvironmentVariables();
		
		string? appID = variables[EnvironmentVariables.PlanningCenter.Credentials.AppID] as string;
		string? secret = variables[EnvironmentVariables.PlanningCenter.Credentials.Secret] as string;
		string encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{appID}:{secret}"));

		client.BaseAddress = new(
			(variables[EnvironmentVariables.PlanningCenter.BaseUrls.Api] as string) ?? "localhost/");
		client.DefaultRequestHeaders.Authorization = new("Basic", encodedCredentials);

		_httpClient = client;
	}

	public async Task<IActionResult> SendRequestAsync(HttpRequest request)
	{
		try
		{
			string path = request.PathAndQuery(AzureFunctions.Dashboard.Name);

			HttpResponseMessage response;
			if (string.Equals(request.Method, "get", StringComparison.InvariantCultureIgnoreCase))
			{
				response = await _httpClient.GetAsync(path);
			}
			else
			{
				response = await _httpClient.PostAsync(path, new StreamContent(request.Body));
			}
			string body = await response.Content.ReadAsStringAsync();

			return new OkObjectResult(body);
		}
		catch (Exception exception)
		{
			return exception.AsSecureErrorResponse();
		}
	}
}
