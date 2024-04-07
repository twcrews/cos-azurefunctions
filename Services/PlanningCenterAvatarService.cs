namespace Cos.AzureFunctions.Services;

public class PlanningCenterAvatarService : IAvatarService
{
	private readonly HttpClient _httpClient;

	public PlanningCenterAvatarService(HttpClient client)
	{
		client.BaseAddress = new(
			(Environment.GetEnvironmentVariable(EnvironmentVariables.PlanningCenter.BaseUrls.Avatars)) ?? "localhost/");
		_httpClient = client;
	}
	
	public async Task<Stream> GetAvatarAsync(string path)
	{
		HttpResponseMessage response = await _httpClient.GetAsync(path);
		return await response.Content.ReadAsStreamAsync();
	}
}
