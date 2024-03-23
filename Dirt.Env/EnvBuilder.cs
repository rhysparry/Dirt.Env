using System.Collections;
using Dirt.Environment;

namespace Dirt;

/// <summary>
/// A class to help build a set of environment variables.
/// </summary>
public sealed class EnvBuilder
{
    private const string NixPath = "PATH";
    private const string WinPath = "Path";

    private readonly Dictionary<string, List<string>> _env = new();

    /// <summary>
    /// Clears the environment variables.
    /// </summary>
    /// <returns>The builder</returns>
    public EnvBuilder Clear()
    {
        _env.Clear();
        return this;
    }

    /// <summary>
    /// Specifies that the path variable should be set to the Windows path variable (Path).
    /// </summary>
    /// <returns>The builder</returns>
    /// <remarks>
    /// Also configures the path separator to be the Windows path separator (;).
    /// </remarks>
    public EnvBuilder UsingWindowsPath()
    {
        PathVariable = WinPath;
        PathSeparator = ';';
        return this;
    }

    /// <summary>
    /// Specifies that the path variable should be set to the *nix path variable (PATH).
    /// </summary>
    /// <returns>The builder</returns>
    /// <remarks>
    /// Also configures the path separator to be the *nix path separator (:).
    /// </remarks>
    public EnvBuilder UsingNixPath()
    {
        PathVariable = NixPath;
        PathSeparator = ':';
        return this;
    }

    /// <summary>
    /// Applies the system environment variables to the builder.
    /// </summary>
    /// <returns>The builder</returns>
    public EnvBuilder WithSystemEnvironment()
    {
        return WithSystemEnvironment(DefaultDuplicateKeyStrategy);
    }

    /// <summary>
    /// Applies the system environment variables to the builder.
    /// </summary>
    /// <param name="ifDuplicate">The action to take if adding a new variable would result in a duplicate.</param>
    /// <returns>The builder</returns>
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

    /// <summary>
    /// Applies an existing set of environment variables to the builder.
    /// </summary>
    /// <param name="environment">The environment variables to add.</param>
    /// <returns>The builder</returns>
    public EnvBuilder WithEnvironment(IDictionary<string, string> environment)
    {
        return WithEnvironment(environment, DefaultDuplicateKeyStrategy);
    }

    /// <summary>
    /// Applies an existing set of environment variables to the builder.
    /// </summary>
    /// <param name="environment">The environment variables to add.</param>
    /// <param name="ifDuplicate">The action to take if adding a new variable would result in a duplicate.</param>
    /// <returns>The builder</returns>
    public EnvBuilder WithEnvironment(IDictionary<string, string> environment, IfDuplicate ifDuplicate)
    {
        foreach (var (key, value) in environment)
        {
            SetVariable(key, value, ifDuplicate);
        }

        return this;
    }

    /// <summary>
    /// Unsets an environment variable.
    /// </summary>
    /// <param name="key">The key of the environment variable to unset.</param>
    /// <returns>The builder</returns>
    public EnvBuilder UnsetVariable(string key)
    {
        _env.Remove(key);
        return this;
    }

    /// <summary>
    /// Sets an environment variable to a given value.
    /// </summary>
    /// <param name="key">The key of the environment variable to set.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>The builder</returns>
    public EnvBuilder SetVariable(string key, string value)
    {
        return SetVariable(key, value, DefaultDuplicateKeyStrategy);
    }

    /// <summary>
    /// Sets an environment variable to a given value.
    /// </summary>
    /// <param name="key">The key of the environment variable to set.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="ifDuplicate">The action to take if the variable already exists.</param>
    /// <returns>The builder</returns>
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

    /// <summary>
    /// Appends a path to the path variable.
    /// </summary>
    /// <param name="value">The value to append</param>
    /// <returns>The builder</returns>
    public EnvBuilder AppendPath(string value)
    {
        return AppendPath(PathVariable, value, DefaultDuplicatePathStrategy);
    }

    /// <summary>
    /// Appends a path to the path variable.
    /// </summary>
    /// <param name="value">The value to append</param>
    /// <param name="ifDuplicatePath">The action to take if the value already exists in the path variable.</param>
    /// <returns>The builder</returns>
    public EnvBuilder AppendPath(string value, IfDuplicatePath ifDuplicatePath)
    {
        return AppendPath(PathVariable, value, ifDuplicatePath);
    }

    /// <summary>
    /// Appends a path to the specified path variable.
    /// </summary>
    /// <param name="key">The path variable to append to</param>
    /// <param name="value">The value to append</param>
    /// <returns>The builder</returns>
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

    /// <summary>
    /// Appends a path to the specified path variable.
    /// </summary>
    /// <param name="key">The path variable to append to</param>
    /// <param name="value">The value to append</param>
    /// <param name="ifDuplicatePath">The action to take if the value already exists in the path variable.</param>
    /// <returns>The builder</returns>
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

    /// <summary>
    /// Prepends a path to the path variable.
    /// </summary>
    /// <param name="value">The value to prepend</param>
    /// <returns>The builder</returns>
    public EnvBuilder PrependPath(string value)
    {
        return PrependPath(PathVariable, value, DefaultDuplicatePathStrategy);
    }

    /// <summary>
    /// Prepends a path to the path variable.
    /// </summary>
    /// <param name="value">The value to prepend</param>
    /// <param name="ifDuplicatePath">The action to take if the value already exists in the path variable.</param>
    /// <returns>The builder</returns>
    public EnvBuilder PrependPath(string value, IfDuplicatePath ifDuplicatePath)
    {
        return PrependPath(PathVariable, value, ifDuplicatePath);
    }

    /// <summary>
    /// Prepends a path to the specified path variable.
    /// </summary>
    /// <param name="key">The path variable to prepend to</param>
    /// <param name="value">The value to prepend</param>
    /// <param name="ifDuplicatePath">The action to take if the value already exists in the path variable.</param>
    /// <returns>The builder</returns>
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

    /// <summary>
    /// Builds an environment based on the current configuration.
    /// </summary>
    /// <returns>A new environment.</returns>
    public Env Build()
    {
        return new Env(this);
    }

    /// <summary>
    /// Gets or sets the path variable to use.
    /// </summary>
    /// <remarks>
    /// Defaults to the path variable for the current operating system.
    /// </remarks>
    public string PathVariable { get; set; } = OperatingSystem.IsWindows() ? WinPath : NixPath;
    /// <summary>
    /// Gets or sets the path separator to use.
    /// </summary>
    /// <remarks>
    /// Defaults to the path separator for the current operating system.
    /// </remarks>
    public char PathSeparator { get; set; } = Path.PathSeparator;

    /// <summary>
    /// Gets or sets the default action to take when adding a duplicate key.
    /// </summary>
    public IfDuplicate DefaultDuplicateKeyStrategy { get; set; } = IfDuplicate.Replace;
    /// <summary>
    /// Gets or sets the default action to take when adding a duplicate path.
    /// </summary>
    public IfDuplicatePath DefaultDuplicatePathStrategy { get; set; } = IfDuplicatePath.Supersede;
}