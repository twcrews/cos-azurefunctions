using System.Collections;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cos.AzureFunctions;

public static class ExceptionExtensions
{
	public static ObjectResult AsSecureErrorResponse(this Exception exception)
	{
		IDictionary variables = Environment.GetEnvironmentVariables();
		string appID = variables[EnvironmentVariables.PlanningCenter.Credentials.AppID] as string ?? "appId";
		string secret = variables[EnvironmentVariables.PlanningCenter.Credentials.Secret] as string ?? "secret";

		string message = exception.Message
			.Replace(appID, "*****", true, CultureInfo.InvariantCulture)
			.Replace(secret, "*****", true, CultureInfo.InvariantCulture);

		ObjectResult response = new(message)
		{
			StatusCode = StatusCodes.Status500InternalServerError
		};
		return response;
	}
}
