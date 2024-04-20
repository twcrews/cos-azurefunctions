using System.Collections;
using System.Text.Json;

namespace Cos.AzureFunctions.Services;

public class GitHubService : IVersionControlService
{
	private readonly HttpClient _httpClient;

	public GitHubService(HttpClient client)
	{
		IDictionary variables = Environment.GetEnvironmentVariables();
		
		client.BaseAddress = new((variables[EnvironmentVariables.GitHub.BaseUrl] as string) ?? "localhost/");
		client.DefaultRequestHeaders.UserAgent.ParseAdd(
			variables[EnvironmentVariables.GitHub.UserAgent] as string);
		client.DefaultRequestHeaders.Authorization = new(
			"Bearer", variables[EnvironmentVariables.GitHub.ApiToken] as string);

		_httpClient = client;
	}

	public async Task<string> GetHeadHash()
	{
		IDictionary variables = Environment.GetEnvironmentVariables();

		HttpRequestMessage request = new()
		{
			Method = HttpMethod.Get,
			RequestUri = new($"{variables[EnvironmentVariables.GitHub.BaseUrl]}{Paths.HeadHash}")
		};
		request.Headers.Authorization = new("Bearer", variables[EnvironmentVariables.GitHub.ApiToken] as string);
		request.Headers.UserAgent.ParseAdd(variables[EnvironmentVariables.GitHub.UserAgent] as string);

		HttpResponseMessage response = await _httpClient.SendAsync(request);
		string responseBody = await response.Content.ReadAsStringAsync();

		return JsonDocument
			.Parse(responseBody).RootElement
			.GetProperty("sha")
			.GetString() ?? throw new NullReferenceException("Head hash was null.");
	}
}
