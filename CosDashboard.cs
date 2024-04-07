using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Cos.AzureFunctions.Services;
using Cos.AzureFunctions.Extensions;

namespace Cos.AzureFunctions;

public class CosDashboard(
	IHttpProxyService httpProxyService,
	IAvatarService avatarService,
	IVersionControlService versionControlService)
{
	private readonly IHttpProxyService _httpProxy = httpProxyService;
	private readonly IAvatarService _avatars = avatarService;
	private readonly IVersionControlService _versionControl = versionControlService;

	[Function(AzureFunctions.Dashboard.Name)]
	public Task<IActionResult> DashboardProxy(
		[HttpTrigger(
			AuthorizationLevel.Function,
			"get",
			"post",
			Route = AzureFunctions.Dashboard.Route)] HttpRequest request)
		=> TryAsync(() => _httpProxy.SendRequestAsync(request));

	[Function(AzureFunctions.Avatars.Name)]
	public Task<IActionResult> AvatarProxy(
		[HttpTrigger(
			AuthorizationLevel.Function,
			"get",
			Route = AzureFunctions.Avatars.Route)] HttpRequest request)
		=> TryAsync(async () =>
		{
			string path = request.PathAndQuery(AzureFunctions.Avatars.Name);
			Stream avatarData = await _avatars.GetAvatarAsync(path);
			return new FileStreamResult(avatarData, $"image/{path.Split('.').Last()}");
		});

	[Function(AzureFunctions.Versioning.HeadHash.Name)]
	public Task<IActionResult> HeadHash(
		[HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request)
		=> TryAsync(async () =>
		{
			var _ = request;
			string hash = await _versionControl.GetHeadHash();
			return new OkObjectResult(hash);
		});

	private static async Task<IActionResult> TryAsync(Func<Task<IActionResult>> action)
	{
		try
		{
			return await action();
		}
		catch (Exception exception)
		{
			return exception.AsSecureErrorResponse();
		}
	}
}
