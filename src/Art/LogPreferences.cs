namespace Art;

/// <summary>
/// Stores preferences to use when logging.
/// </summary>
/// <param name="DataUnits">Units to display values in.</param>
/// <param name="DataUnitFormat">Unit format to display values in.</param>
public record LogPreferences(DataUnits DataUnits, DataUnitFormat DataUnitFormat)
{
    /// <summary>
    /// Stores a basic <see cref="LogPreferences"/> to use by default.
    /// </summary>
    public static readonly LogPreferences Default = new(DataUnits.Binary, DataUnitFormat.Short);
}
