using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace COS.Internal.Dashboard.AzureFunctions;

public static class Services
{
	[FunctionName(Constants.AuthenticationFunctionName)]
	public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest _) => new OkResult();
}
