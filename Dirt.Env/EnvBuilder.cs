using System.Collections;
using Dirt.Environment;

namespace Dirt;

public sealed class EnvBuilder
{
    private const string NixPath = "PATH";
    private const string WinPath = "Path";

    private readonly Dictionary<string, List<string>> _env = new();

    public EnvBuilder Clear()
    {
        _env.Clear();
        return this;
    }

    public EnvBuilder UsingWindowsPath()
    {
        PathVariable = WinPath;
        PathSeparator = ';';
        return this;
    }

    public EnvBuilder UsingNixPath()
    {
        PathVariable = NixPath;
        PathSeparator = ':';
        return this;
    }

    public EnvBuilder WithSystemEnvironment()
    {
        return WithSystemEnvironment(DefaultDuplicateKeyStrategy);
    }

    public EnvBuilder WithSystemEnvironment(IfDuplicate ifDuplicate)
    {
        foreach (DictionaryEntry entry in System.Environment.GetEnvironmentVariables())
        {
            if (entry is not { Key: string key, Value: string value })
            {
                continue;
            }

            SetVariable(key, value);
        }

        return this;
    }

    public EnvBuilder WithEnvironment(IDictionary<string, string> environment)
    {
        return WithEnvironment(environment, DefaultDuplicateKeyStrategy);
    }

    public EnvBuilder WithEnvironment(IDictionary<string, string> environment, IfDuplicate ifDuplicate)
    {
        foreach (var (key, value) in environment)
        {
            SetVariable(key, value, ifDuplicate);
        }

        return this;
    }

    public EnvBuilder UnsetVariable(string key)
    {
        _env.Remove(key);
        return this;
    }

    public EnvBuilder SetVariable(string key, string value)
    {
        return SetVariable(key, value, DefaultDuplicateKeyStrategy);
    }

    public EnvBuilder SetVariable(string key, string value, IfDuplicate ifDuplicate)
    {
        if (ifDuplicate != IfDuplicate.Replace && _env.ContainsKey(key))
        {
            return this;
        }

        if (key == PathVariable)
        {
            _env[key] = value.Split(PathSeparator).ToList();
        }
        else
        {
            _env[key] = [value];
        }

        return this;
    }

    public EnvBuilder AppendPath(string value)
    {
        return AppendPath(PathVariable, value, DefaultDuplicatePathStrategy);
    }

    public EnvBuilder AppendPath(string value, IfDuplicatePath ifDuplicatePath)
    {
        return AppendPath(PathVariable, value, ifDuplicatePath);
    }

    public EnvBuilder AppendPath(string key, string value)
    {
        return AppendPath(key, value, DefaultDuplicatePathStrategy);
    }

    private static bool HandleDuplicatePath(List<string> values, string value, IfDuplicatePath ifDuplicatePath)
    {
        switch (ifDuplicatePath)
        {
            case IfDuplicatePath.Supersede:
                values.RemoveAll(v => v == value);
                return true;
            case IfDuplicatePath.Skip:
                return !values.Contains(value);
            case IfDuplicatePath.Ignore:
            default:
                return true;
        }
    }

    public EnvBuilder AppendPath(string key, string value, IfDuplicatePath ifDuplicatePath)
    {
        if (_env.TryGetValue(key, out var values))
        {
            if (HandleDuplicatePath(values, value, ifDuplicatePath))
            {
                values.Add(value);
            }
        }
        else
        {
            _env[key] = [value];
        }

        return this;
    }

    public EnvBuilder PrependPath(string value)
    {
        return PrependPath(PathVariable, value, DefaultDuplicatePathStrategy);
    }

    public EnvBuilder PrependPath(string value, IfDuplicatePath ifDuplicatePath)
    {
        return PrependPath(PathVariable, value, ifDuplicatePath);
    }

    public EnvBuilder PrependPath(string key, string value, IfDuplicatePath ifDuplicatePath)
    {
        if (_env.TryGetValue(PathVariable, out var values))
        {
            if (HandleDuplicatePath(values, value, ifDuplicatePath))
            {
                values.Insert(0, value);
            }
        }
        else
        {
            _env[key] = [value];
        }

        return this;
    }

    internal IEnumerable<KeyValuePair<string, string>> BuildEnvironment() =>
        _env.Select(kvp => new KeyValuePair<string, string>(kvp.Key, string.Join(PathSeparator, kvp.Value)));

    public Env Build()
    {
        return new Env(this);
    }

    public string PathVariable { get; set; } = OperatingSystem.IsWindows() ? WinPath : NixPath;
    public char PathSeparator { get; set; } = Path.PathSeparator;

    public IfDuplicate DefaultDuplicateKeyStrategy { get; set; } = IfDuplicate.Replace;
    public IfDuplicatePath DefaultDuplicatePathStrategy { get; set; } = IfDuplicatePath.Supersede;
}