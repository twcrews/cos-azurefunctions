using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Cos.AzureFunctions;

public static class Services
{
	[Function(AzureFunctions.Authentication.Name)]
	public static IActionResult Run(
		[HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request)
	{
		var _ = request;
		return new OkResult();
	}
}
