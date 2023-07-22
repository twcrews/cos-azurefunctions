using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace COS.Internal.Dashboard.AzureFunctions;

public static class Services
{
	[FunctionName(AzureFunctions.Authentication.Name)]
	public static IActionResult Run(
		[HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request)
	{
		request.GetHashCode(); // Keeps warnings away for unused parameter.
		return new OkResult();
	}
}
