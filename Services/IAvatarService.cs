namespace Cos.AzureFunctions.Services;

/// <summary>
/// Defines a service for handling user avatar images.
/// </summary>
public interface IAvatarService
{
	/// <summary>
	/// Retrieves a user's avatar as a data stream using the provided path.
	/// </summary>
	/// <param name="path">The identifying path of the avatar.</param>
	/// <returns>Returns a Stream representing the avatar image data.</returns>
	Task<Stream> GetAvatarAsync(string path);
}
