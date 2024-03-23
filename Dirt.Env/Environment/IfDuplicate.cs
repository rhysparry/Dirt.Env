namespace Dirt.Environment;

/// <summary>
/// Action to perform if a new entry will introduce a duplicate
/// </summary>
public enum IfDuplicate
{
    /// <summary>
    /// Replaces the existing entry with the new one
    /// </summary>
    Replace,
    /// <summary>
    /// Skip the addition of the new entry. The existing entry will remain.
    /// </summary>
    Skip
}