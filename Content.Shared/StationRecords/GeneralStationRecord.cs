using Robust.Shared.Enums;
using Robust.Shared.Prototypes; // Lua
using Robust.Shared.Serialization;
using Content.Shared.StatusIcon; // Lua
using Content.Shared.Roles; // Lua

namespace Content.Shared.StationRecords;

/// <summary>
///     General station record. Indicates the crewmember's name and job.
/// </summary>
[Serializable, NetSerializable]
public sealed record GeneralStationRecord
{
    /// <summary>
    ///     Name tied to this station record.
    /// </summary>
    [DataField]
    public string Name = string.Empty;

    /// <summary>
    ///     Age of the person that this station record represents.
    /// </summary>
    [DataField]
    public int Age;

    /// <summary>
    ///     Job title tied to this station record.
    /// </summary>
    [DataField]
    public string JobTitle = string.Empty;

    /// <summary>
    ///     Job icon tied to this station record.
    /// </summary>
    [DataField]
    public ProtoId<JobIconPrototype> JobIcon = string.Empty; // Lua

    [DataField]
    public ProtoId<JobPrototype> JobPrototype = string.Empty; // Lua

    /// <summary>
    ///     Species tied to this station record.
    /// </summary>
    [DataField]
    public string Species = string.Empty;

    /// <summary>
    ///     Gender identity tied to this station record.
    /// </summary>
    /// <remarks>Sex should be placed in a medical record, not a general record.</remarks>
    [DataField]
    public Gender Gender = Gender.Epicene;

    /// <summary>
    ///     The priority to display this record at.
    ///     This is taken from the 'weight' of a job prototype,
    ///     usually.
    /// </summary>
    [DataField]
    public int DisplayPriority;

    /// <summary>
    ///     Fingerprint of the person.
    /// </summary>
    [DataField]
    public string? Fingerprint;

    /// <summary>
    ///     DNA of the person.
    /// </summary>
    [DataField]
    public string? DNA;
}
