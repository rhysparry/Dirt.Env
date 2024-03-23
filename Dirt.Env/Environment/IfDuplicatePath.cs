namespace Dirt.Environment;

/// <summary>
/// The action to take if adding to a path variable would introduce a duplicate.
/// </summary>
public enum IfDuplicatePath
{
    /// <summary>
    /// Remove the existing entry, or entries if there are already duplicates, and add the new entry.
    /// </summary>
    Supersede,
    /// <summary>
    /// Ignore the existing entry and add the new entry. This will result in duplicates.
    /// </summary>
    Ignore,
    /// <summary>
    /// Skip adding the new entry. The original entry will remain.
    /// </summary>
    Skip
}