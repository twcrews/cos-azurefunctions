using Microsoft.AspNetCore.Http;

namespace Cos.AzureFunctions.Extensions;

public static class HttpRequestExtensions
{
	public static string PathAndQuery(this HttpRequest request, string functionName)
	{
		string basePath = $"/api/{functionName}";
		string fullRequestPath = $"{request.Path.Value}{request.QueryString}";

		if (fullRequestPath == basePath)
		{
			fullRequestPath = $"{fullRequestPath}/";
		}

		return fullRequestPath.Replace($"/api/{functionName}/", "", StringComparison.InvariantCultureIgnoreCase);
	}
}
