namespace Art.Common.IO;

/// <summary>
/// Specifies an action to take for a newly created path.
/// </summary>
/// <seealso cref="ArtIOUtility.CreateRandomPath"/>
public enum PathCreationAction
{
    /// <summary>
    /// Do not create any item.
    /// </summary>
    None = 0,
    /// <summary>
    /// Create a blank file.
    /// </summary>
    CreateFile = 1,
    /// <summary>
    /// Create an empty directory.
    /// </summary>
    CreateDirectory = 2,
}
