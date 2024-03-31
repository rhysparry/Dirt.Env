using System.Runtime.Versioning;
using Mono.Unix;

namespace Dirt.Environment;

/// <summary>
/// Extension methods for finding files in the path.
/// </summary>
public static class FindInPath
{
    /// <summary>
    /// Finds the first executable file in the path with the given name.
    /// </summary>
    /// <param name="env">The environment to search.</param>
    /// <param name="fileName">The name of the executable to search for.</param>
    /// <returns>The full path to the executable if found; otherwise null.</returns>
    /// <remarks>
    /// On Windows, the PATHEXT environment variable is used to automatically identify
    /// appropriate file extensions. On Unix-like systems, the file must be executable
    /// by the current user (i.e. the file must have the execute bit set for the user,
    /// any of the user's groups, or others).
    /// </remarks>
    public static string? FindExecutableInPath(this IEnv env, string fileName) =>
        SearchExecutableInPath(env, fileName).FirstOrDefault();

    /// <summary>
    /// Finds all executable files in the path with the given name.
    /// </summary>
    /// <param name="env">The environment to search.</param>
    /// <param name="fileName">The name of the executable to search for.</param>
    /// <returns>A list of paths for each executable found.</returns>
    /// <remarks>
    /// On Windows, the PATHEXT environment variable is used to automatically identify
    /// appropriate file extensions. On Unix-like systems, the file must be executable
    /// by the current user (i.e. the file must have the execute bit set for the user,
    /// any of the user's groups, or others).
    /// </remarks>
    public static IReadOnlyList<string> FindAllExecutablesInPath(this IEnv env, string fileName) =>
        SearchExecutableInPath(env, fileName).ToList();

    private static IEnumerable<string> SearchExecutableInPath(IEnv env, string fileName) =>
        OperatingSystem.IsWindows()
            ? SearchWindowsExecutableInPath(env, fileName)
            : SearchNixExecutableInPath(env, fileName);

    [SupportedOSPlatform("windows")]
    private static IEnumerable<string> SearchWindowsExecutableInPath(IEnv env, string fileName)
    {
        foreach (var path in env.Path)
        {
            var fullPath = Path.Combine(path, fileName);
            if (File.Exists(fullPath) && Path.GetExtension(fullPath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                yield return fullPath;
            }
            else
            {
                var pathExt = env.GetPathSeparatedValue("PATHEXT");
                foreach (var ext in pathExt)
                {
                    fullPath = Path.Combine(path, fileName + ext);
                    if (File.Exists(fullPath))
                    {
                        yield return fullPath;
                    }
                }
            }
        }
    }

    [UnsupportedOSPlatform("windows")]
    private static IEnumerable<string> SearchNixExecutableInPath(IEnv env, string fileName) =>
    env.Path
        .Select(path => Path.Combine(path, fileName))
        .Where(File.Exists)
        .Where(IsNixExecutableByCurrentUser);

    [UnsupportedOSPlatform("windows")]
    private static bool IsNixExecutableByCurrentUser(string path)
    {
        var fileMode = File.GetUnixFileMode(path);
        if (fileMode.HasFlag(UnixFileMode.OtherExecute))
        {
            return true;
        }

        var fileInfo = new UnixFileInfo(path);
        var realUser = UnixUserInfo.GetRealUser();
        if (fileMode.HasFlag(UnixFileMode.UserExecute))
        {
            var fileUser = fileInfo.OwnerUser;
            if (fileUser.UserId == realUser.UserId)
            {
                return true;
            }
        }

        if (!fileMode.HasFlag(UnixFileMode.GroupExecute))
        {
            return false;
        }

        // Check if the real user is in the group
        var fileGroup = fileInfo.OwnerGroup;
        if (fileGroup.GroupId == realUser.GroupId)
        {
            return true;
        }

        // Check the user's other groups
        var usersGroups = UnixGroupInfo.GetLocalGroups()
            .Where(g => g.GetMembers().Any(m => m.UserId == realUser.UserId));
        return usersGroups.Any(group => group.GroupId == fileGroup.GroupId);
    }
}