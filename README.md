# Dirt.Env

Dirt.Env is a simple library for working with environment variables in .NET.

Its goal is to provide some convenience and safety around working with environment variables.

## Usage

You can use the default constructor of the `Env` class to get access to the current environment variables.

```csharp
var env = new Dirt.Env();
```

This class implements the `IEnv` interface, which allows accessing the environment variables in a read-only manner through the API of `IReadOnlyDictionary<string, string>`.

In addition to general dictionary access, the path environment variable can be accessed through the `Path` property which returns a read-only list.

You can also use the path separation logic on any environment variable by using the `GetPathSeparatedValue(string)` method. 

The path separator of the current system will be used by default, but this can be configured by using the `EnvBuilder` class.

### `EnvBuilder`

The `EnvBuilder` class makes it easy to build up an `Env` instance with custom environment variables.

```csharp
var env = new EnvBuilder()
    .WithSystemEnvironment()
    .SetVariable("MY_CUSTOM_VARIABLE", "MY_VALUE")
    .Build();
```

You can also work with the path variable in a natural way. The underlying path variable used and path separator will be managed by the `EnvBuilder` class.

```csharp
var env = new EnvBuilder()
    .WithSystemEnvironment()
    .AppendPath("C:\\My\\Path")
    .AppendPath("C:\\My\\OtherPath")
    .Build();
```

When working with paths, the `EnvBuilder` class provides three ways it can handle duplicate paths being appended or prepended:

- `IfDuplicatePath.Supersede` will remove any duplicate paths from the list before adding the new path. This is the default.
- `IfDuplicatePath.Ignore` will always add the path if it is already in the list. This will result in duplicate paths.
- `IfDuplicatePath.Skip` will not add the path if it is already in the list.

This enum is located in the `Dirt.Environment` namespace.

These strategies can be configured with every addition to the builder or set a default. When set in the builder it will only take effect for future additions.

## License

MIT