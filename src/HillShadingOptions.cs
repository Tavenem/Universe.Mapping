﻿namespace Tavenem.Universe.Maps;

/// <summary>
/// Options for specifying hill shading to a surface map.
/// </summary>
/// <param name="ApplyToLand">
/// Whether the shading will be applied to areas above sea level.
/// </param>
/// <param name="ApplyToOcean">
/// Whether the shading will be applied to areas below sea level.
/// </param>
/// <param name="ScaleFactor">
/// Controls the intensity of the shading relative to local slope.
/// </param>
/// <param name="ScaleIsRelative">
/// Whether <see cref="ScaleIsRelative"/> is adjusted dynamically based on local elevation.
/// </param>
/// <param name="ShadeMultiplier">
/// Adjusts the intensity of all shading.
/// </param>
public readonly record struct HillShadingOptions(
    bool ApplyToLand,
    bool ApplyToOcean,
    double ScaleFactor = 5,
    bool ScaleIsRelative = true,
    double ShadeMultiplier = 1.25)
{
    /// <summary>
    /// Controls the intensity of the shading relative to local slope.
    /// </summary>
    public double ScaleFactor { get; init; } = Math.Max(0, ScaleFactor);

    /// <summary>
    /// Adjusts the intensity of all shading.
    /// </summary>
    public double ShadeMultiplier { get; init; } = Math.Max(0, ShadeMultiplier);
}
