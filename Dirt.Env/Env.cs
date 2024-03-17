using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Dirt;

public sealed class Env : IEnv
{
    private readonly FrozenDictionary<string, string> _env;
    private readonly Lazy<IReadOnlyList<string>> _path;
    private readonly FrozenDictionary<string, Lazy<IReadOnlyList<string>>> _pathSeparatedValues;

    public Env() : this(new EnvBuilder().WithSystemEnvironment())
    {
    }

    public Env(IDictionary<string, string> environment) : this(new EnvBuilder().WithEnvironment(environment))
    {
    }

    internal Env(EnvBuilder builder) : this(builder.BuildEnvironment(), builder.PathSeparator, builder.PathVariable)
    {
    }

    internal Env(IEnumerable<KeyValuePair<string, string>> environment, char pathSeparator, string pathVariable)
    {
        PathSeparator = pathSeparator;
        _env = environment.ToFrozenDictionary();
        _pathSeparatedValues = _env.ToFrozenDictionary(
            kvp => kvp.Key,
            kvp => new Lazy<IReadOnlyList<string>>(() => kvp.Value.Split(PathSeparator)));
        _path = new Lazy<IReadOnlyList<string>>(() => GetPathSeparatedValue(pathVariable));
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _env.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _env.Count;
    public bool ContainsKey(string key) => _env.ContainsKey(key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value) =>
        _env.TryGetValue(key, out value);

    public string this[string key] => _env[key];

    public IEnumerable<string> Keys => _env.Keys;
    public IEnumerable<string> Values => _env.Values;
    public IReadOnlyList<string> Path => _path.Value;
    public char PathSeparator { get; }

    public IReadOnlyList<string> GetPathSeparatedValue(string key)
    {
        return _pathSeparatedValues.TryGetValue(key, out var value) ? value.Value : Array.Empty<string>();
    }
}