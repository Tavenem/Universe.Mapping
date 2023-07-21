using System.Text.Json.Serialization;
using Tavenem.Mathematics;

namespace Tavenem.Universe.Maps;

/// <summary>
/// Options for projecting a map.
/// </summary>
/// <param name="CentralMeridian">
/// <para>
/// The longitude of the central meridian of the projection, in radians.
/// </para>
/// <para>
/// Values are truncated to the range -π..π.
/// </para>
/// </param>
/// <param name="CentralParallel">
/// <para>
/// The latitude of the central parallel of the projection, in radians.
/// </para>
/// <para>
/// Values are truncated to the range -π/2..π/2.
/// </para>
/// </param>
/// <param name="StandardParallels">
/// <para>
/// The latitude of the standard parallels (north and south of the equator) where the scale
/// of the projection is 1:1, in radians.
/// </para>
/// <para>
/// It does not matter whether the positive or negative latitude is provided, if it is
/// non-zero.
/// </para>
/// <para>
/// If left <see langword="null"/> the central parallel is assumed.
/// </para>
/// <para>
/// Values are truncated to the range -π/2..π/2.
/// </para>
/// </param>
/// <param name="Range">
/// <para>
/// If provided, indicates the latitude range (north and south of the central parallel)
/// shown on the projection, in radians.
/// </para>
/// <para>
/// If left <see langword="null"/>, or equal to zero, the full globe is projected.
/// </para>
/// <para>
/// Values are truncated to the range 0..π.
/// </para>
/// </param>
/// <param name="EqualArea">
/// Indicates whether the projection is to be cylindrical equal-area (rather than
/// equirectangular).
/// </param>
[method: JsonConstructor]
public readonly record struct MapProjectionOptions(
    double CentralMeridian = 0,
    double CentralParallel = 0,
    double? StandardParallels = null,
    double? Range = null,
    bool EqualArea = false)
{
    /// <summary>
    /// An equirectangular projection of the entire globe, with the standard parallel at the
    /// equator.
    /// </summary>
    public static MapProjectionOptions Default { get; } = new(EqualArea: false);

    /// <summary>
    /// <para>
    /// The aspect ratio of the map.
    /// </para>
    /// <para>
    /// Always 2 for an equirectangular projection.
    /// </para>
    /// <para>
    /// Equal to ScaleFactor²π for a cylindrical equal-area projection.
    /// </para>
    /// </summary>
    [JsonIgnore]
    public double AspectRatio { get; } = EqualArea
        ? Math.PI * Math.Cos(StandardParallels ?? CentralParallel).Square()
        : 2;

    /// <summary>
    /// <para>
    /// The longitude of the central meridian of the projection, in radians.
    /// </para>
    /// <para>
    /// Values are truncated to the range -π..π.
    /// </para>
    /// </summary>
    public double CentralMeridian { get; } = CentralMeridian.Clamp(-Math.PI, Math.PI);

    /// <summary>
    /// <para>
    /// The latitude of the central parallel of the projection, in radians.
    /// </para>
    /// <para>
    /// Values are truncated to the range -π/2..π/2.
    /// </para>
    /// </summary>
    public double CentralParallel { get; } = CentralParallel.Clamp(-DoubleConstants.HalfPi, DoubleConstants.HalfPi);

    /// <summary>
    /// <para>
    /// If provided, indicates the latitude range (north and south of the central parallel)
    /// shown on the projection, in radians.
    /// </para>
    /// <para>
    /// If left <see langword="null"/>, or equal to zero, the full globe is projected.
    /// </para>
    /// <para>
    /// Values are truncated to the range 0..π.
    /// </para>
    /// </summary>
    public double? Range { get; } = Range.HasValue
        ? Range.Value.Clamp(0, Math.PI)
        : null;

    /// <summary>
    /// The cosine of the standard parallel.
    /// </summary>
    [JsonIgnore]
    public double ScaleFactor { get; } = Math.Cos(StandardParallels ?? CentralParallel);

    /// <summary>
    /// <para>
    /// The latitude of the standard parallels (north and south of the equator) where the scale
    /// of the projection is 1:1, in radians.
    /// </para>
    /// <para>
    /// It does not matter whether the positive or negative latitude is provided, if it is
    /// non-zero.
    /// </para>
    /// <para>
    /// If left <see langword="null"/> the central parallel is assumed.
    /// </para>
    /// <para>
    /// Values are truncated to the range -π/2..π/2.
    /// </para>
    /// </summary>
    public double? StandardParallels { get; } = StandardParallels.HasValue
        ? StandardParallels.Value.Clamp(-DoubleConstants.HalfPi, DoubleConstants.HalfPi)
        : null;

    /// <summary>
    /// Gets a new instance of <see cref="MapProjectionOptions"/> with the same properties as this
    /// one, except the values indicated.
    /// </summary>
    /// <param name="centralMeridian">
    /// <para>
    /// The longitude of the central meridian of the projection, in radians.
    /// </para>
    /// <para>
    /// Values are truncated to the range -π..π.
    /// </para>
    /// </param>
    /// <param name="centralParallel">
    /// <para>
    /// The latitude of the central parallel of the projection, in radians.
    /// </para>
    /// <para>
    /// Values are truncated to the range -π/2..π/2.
    /// </para>
    /// </param>
    /// <param name="standardParallels">
    /// <para>
    /// The latitude of the standard parallels (north and south of the equator) where the scale of
    /// the projection is 1:1, in radians.
    /// </para>
    /// <para>
    /// It does not matter whether the positive or negative latitude is provided, if it is non-zero.
    /// </para>
    /// <para>
    /// If left <see langword="null"/> the central parallel is assumed.
    /// </para>
    /// <para>
    /// Values are truncated to the range -π/2..π/2.
    /// </para>
    /// </param>
    /// <param name="range">
    /// <para>
    /// If provided, indicates the latitude range (north and south of the central parallel) shown on
    /// the projection, in radians.
    /// </para>
    /// <para>
    /// If left <see langword="null"/>, or equal to zero, the full globe is projected.
    /// </para>
    /// <para>
    /// Values are truncated to the range 0..π.
    /// </para>
    /// </param>
    /// <param name="equalArea">
    /// Indicates whether the projection is to be cylindrical equal-area (rather than
    /// equirectangular).
    /// </param>
    public MapProjectionOptions With(
        double? centralMeridian = null,
        double? centralParallel = null,
        double? standardParallels = null,
        double? range = null,
        bool? equalArea = null) => new(
            centralMeridian ?? CentralMeridian,
            centralParallel ?? CentralParallel,
            standardParallels ?? StandardParallels,
            range ?? Range,
            equalArea ?? EqualArea);
}
