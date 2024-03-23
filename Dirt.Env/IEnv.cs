namespace Dirt;

/// <summary>
/// A read-only representation of environment variables.
/// </summary>
public interface IEnv : IReadOnlyDictionary<string, string>
{
    /// <summary>
    /// Gets the paths specified by the appropriate path environment variable
    /// split by the path separator.
    /// </summary>
    IReadOnlyList<string> Path { get; }
    /// <summary>
    /// The path separator used to split path environment variables.
    /// </summary>
    char PathSeparator { get; }
    /// <summary>
    /// Gets an environment variable split by the path separator.
    /// </summary>
    /// <param name="key">The key of the environment variable</param>
    /// <returns>A list of values from the environment variable.</returns>
    /// <remarks>
    /// An empty list will be returned if the key does not exist.
    /// </remarks>
    IReadOnlyList<string> GetPathSeparatedValue(string key);
}