using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cos.AzureFunctions.Services;

/// <summary>
/// Defines a service that proxies an HTTP request to another URL.
/// Useful for working around CORS or for sanitizing requests and responses.
/// </summary>
public interface IHttpProxyService
{
	/// <summary>
	/// Sends a request. The implementing class is repsonsible for determining how this is done.
	/// </summary>
	/// <param name="request">The HTTP request to send.</param>
	/// <returns>Returns the result received from the destination.</returns>
	Task<IActionResult> SendRequestAsync(HttpRequest request);
}
