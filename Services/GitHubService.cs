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
		HttpResponseMessage response = await _httpClient.GetAsync(Paths.HeadHash);
		string responseBody = await response.Content.ReadAsStringAsync();

		return JsonDocument
			.Parse(responseBody).RootElement
			.GetProperty("sha")
			.GetString() ?? throw new NullReferenceException("Head hash was null.");
	}
}
