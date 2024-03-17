namespace Dirt;

public interface IEnv : IReadOnlyDictionary<string, string>
{
    IReadOnlyList<string> Path { get; }
    char PathSeparator { get; }
    IReadOnlyList<string> GetPathSeparatedValue(string key);
}