namespace Cos.AzureFunctions.Services;

/// <summary>
/// Defines a service for managing version control systems.
/// </summary>
public interface IVersionControlService
{
	/// <summary>
	/// Gets a hash representing the current head commit of a repository.
	/// </summary>
	/// <returns>Returns the hash as a string.</returns>
	public Task<string> GetHeadHash();
}
