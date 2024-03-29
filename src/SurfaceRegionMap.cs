﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using Tavenem.HugeNumbers;
using Tavenem.Mathematics;
using Tavenem.Time;
using Tavenem.Universe.Climate;
using Tavenem.Universe.Place;
using Tavenem.Universe.Space;

namespace Tavenem.Universe.Maps;

/// <summary>
/// Static methods to assist with producing equirectangular projections that map the surface of
/// a planetary region.
/// </summary>
public static class SurfaceRegionMap
{
    /// <summary>
    /// Gets the value for a <paramref name="position"/> in a <paramref name="region"/> at a
    /// given proportion of the year from a set of ranges.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="position">A position relative to the center of <paramref
    /// name="region"/>.</param>
    /// <param name="ranges">A set of ranges.</param>
    /// <param name="proportionOfYear">
    /// The proportion of the year, starting and ending with midwinter, at which the calculation
    /// is to be performed.
    /// </param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>The value for a <paramref name="position"/> in a <paramref name="region"/> at a
    /// given proportion of the year from a set of ranges.</returns>
    public static float GetAnnualValueFromLocalPosition(
        this SurfaceRegion region,
        Planetoid planet,
        Vector3 position,
        FloatRange[,] ranges,
        float proportionOfYear,
        bool equalArea = false)
    {
        var (x, y) = region.GetProjectionFromLocalPosition(
            planet,
            position,
            ranges.GetLength(0),
            ranges.GetLength(1),
            equalArea);
        return SurfaceMap.GetAnnualRangeValue(
            ranges[x, y],
            proportionOfYear);
    }

    /// <summary>
    /// Gets the value for a <paramref name="position"/> in a <paramref name="region"/> at a
    /// given <paramref name="moment"/> from a set of ranges.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="position">A position relative to the center of <paramref
    /// name="region"/>.</param>
    /// <param name="ranges">A set of ranges.</param>
    /// <param name="moment">The time at which the calculation is to be performed.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>The value for a <paramref name="position"/> in a <paramref name="region"/> at a
    /// given <paramref name="moment"/> from a set of ranges.</returns>
    public static float GetAnnualValueFromLocalPosition(
        this SurfaceRegion region,
        Planetoid planet,
        Vector3 position,
        FloatRange[,] ranges,
        Instant moment,
        bool equalArea = false)
    {
        var (x, y) = region.GetProjectionFromLocalPosition(
            planet,
            position,
            ranges.GetLength(0),
            ranges.GetLength(1),
            equalArea);
        return planet.GetAnnualRangeValue(
            ranges[x, y],
            moment);
    }

    /// <summary>
    /// Determines whether the given proportion of the year falls within the range indicated for
    /// a <paramref name="position"/> in a <paramref name="region"/>.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="position">A position relative to the center of <paramref
    /// name="region"/>.</param>
    /// <param name="ranges">A set of ranges.</param>
    /// <param name="proportionOfYear">
    /// The proportion of the year, starting and ending with midwinter, at which the calculation
    /// is to be performed.
    /// </param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns><see langword="true"/> if the given proportion of the year falls within the
    /// range indicated for a <paramref name="position"/> in a <paramref name="region"/>;
    /// otherwise <see langword="false"/>.</returns>
    public static bool GetAnnualRangeIsPositiveAtTimeAndLocalPosition(
        this SurfaceRegion region,
        Planetoid planet,
        Vector3 position,
        FloatRange[,] ranges,
        float proportionOfYear,
        bool equalArea = false)
    {
        var (x, y) = region.GetProjectionFromLocalPosition(
            planet,
            position,
            ranges.GetLength(0),
            ranges.GetLength(1),
            equalArea);
        return SurfaceMap.GetAnnualRangeIsPositiveAtTime(ranges[x, y], proportionOfYear);
    }

    /// <summary>
    /// Determines whether the given <paramref name="moment"/> falls within the range indicated
    /// for a <paramref name="position"/> in a <paramref name="region"/>.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="position">A position relative to the center of <paramref
    /// name="region"/>.</param>
    /// <param name="ranges">A set of ranges.</param>
    /// <param name="moment">The time at which the determination is to be performed.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns><see langword="true"/> if the given <paramref name="moment"/> falls within the
    /// range indicated for a <paramref name="position"/> in a <paramref name="region"/>;
    /// otherwise <see langword="false"/>.</returns>
    public static bool GetAnnualRangeIsPositiveAtTimeAndLocalPosition(
        this SurfaceRegion region,
        Planetoid planet,
        Vector3 position,
        FloatRange[,] ranges,
        Instant moment,
        bool equalArea = false)
    {
        var (x, y) = region.GetProjectionFromLocalPosition(
            planet,
            position,
            ranges.GetLength(0),
            ranges.GetLength(1),
            equalArea);
        return planet.GetAnnualRangeIsPositiveAtTime(ranges[x, y], moment);
    }

    /// <summary>
    /// Calculates the approximate area of a point on a map projection with the given
    /// characteristics, by transforming the point and its nearest neighbors to latitude and
    /// longitude, calculating the midpoints between them, and calculating the area of the
    /// region enclosed within those midpoints.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="x">The x coordinate of a point on a map projection, with zero as the
    /// westernmost point.</param>
    /// <param name="y">The y coordinate of a point on a map projection, with zero as the
    /// northernmost point.</param>
    /// <param name="resolution">The vertical resolution of the projection.</param>
    /// <param name="options">
    /// The map projection options used to generate the map used.
    /// </param>
    /// <returns>The area of the given point, in m².</returns>
    public static HugeNumber GetAreaOfLocalPoint(
        this SurfaceRegion region,
        Planetoid planet,
        int x, int y,
        int resolution,
        MapProjectionOptions options) => SurfaceMap.GetAreaOfPointFromRadiusSquared(
            planet.RadiusSquared,
            x, y,
            (int)Math.Floor(resolution * options.AspectRatio),
            resolution,
            region.GetProjection(planet, options.EqualArea));

    /// <summary>
    /// Gets the elevation at the given <paramref name="latitude"/> and <paramref
    /// name="longitude"/>, in meters.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="elevationMap">An elevation map.</param>
    /// <param name="latitude">The latitude at which to determine elevation.</param>
    /// <param name="longitude">The longitude at which to determine elevation.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection is a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>
    /// The elevation at the given <paramref name="latitude"/> and <paramref name="longitude"/>,
    /// in meters. Or <see cref="double.NaN"/> if the given <paramref name="latitude"/> and
    /// <paramref name="longitude"/> are not contained within this region.
    /// </returns>
    public static double GetElevationAt(
        this SurfaceRegion region,
        Planetoid planet,
        Image<L16> elevationMap,
        double latitude,
        double longitude,
        bool equalArea = false) => region.IsPositionWithin(planet, latitude, longitude)
        ? (elevationMap.GetValueFromImage(
            latitude,
            longitude,
            region.GetProjection(planet, equalArea),
            true)
            - planet.NormalizedSeaLevel)
            * planet.MaxElevation
        : double.NaN;

    /// <summary>
    /// Gets the elevation at the given <paramref name="position"/>, in meters.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="elevationMap">An elevation map.</param>
    /// <param name="position">The position at which to determine elevation.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection is a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>
    /// The elevation at the given <paramref name="position"/>, in meters. Or <see
    /// cref="double.NaN"/> if the given <paramref name="position"/> is not contained within
    /// this region.
    /// </returns>
    public static double GetElevationAt(
        this SurfaceRegion region,
        Planetoid planet,
        Image<L16> elevationMap,
        Vector3 position,
        bool equalArea = false)
    {
        var pos = region.PlanetaryPosition + position;
        return region.GetElevationAt(
                planet,
                elevationMap,
                planet.VectorToLatitude(pos),
                planet.VectorToLongitude(pos),
                equalArea);
    }

    /// <summary>
    /// Produces an elevation map projection of this region.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The planet being mapped.</param>
    /// <param name="resolution">The vertical resolution of the projection.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>
    /// A projected map of elevation. Pixel luminosity indicates elevation relative to <see
    /// cref="Planetoid.MaxElevation"/>, with values below the midpoint indicating elevations
    /// below the mean surface.
    /// </returns>
    public static Image<L16> GetElevationMap(
        this SurfaceRegion region,
        Planetoid planet,
        int resolution,
        bool equalArea = false) => planet.GetElevationMap(resolution, region.GetProjection(planet, equalArea));

    /// <summary>
    /// Calculates the latitude and longitude that correspond to a set of coordinates from a map
    /// projection.
    /// </summary>
    /// <param name="region">The region being mapped.</param>
    /// <param name="planet">The planet being mapped.</param>
    /// <param name="x">
    /// The x coordinate of a point on a map projection, with zero as the westernmost point.
    /// </param>
    /// <param name="y">
    /// The y coordinate of a point on a map projection, with zero as the northernmost point.
    /// </param>
    /// <param name="resolution">The vertical resolution of the projection.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>
    /// The latitude and longitude of the given coordinates, in radians.
    /// </returns>
    public static (double latitude, double longitude) GetLatLonFromLocalPosition(
        this SurfaceRegion region,
        Planetoid planet,
        int x, int y,
        int resolution,
        bool equalArea = false)
        => SurfaceMap.GetLatLonForMapProjection(
            x, y,
            resolution,
            region.GetProjection(planet, equalArea));

    /// <summary>
    /// Calculates the latitude and longitude that correspond to a set of coordinates from a map
    /// projection.
    /// </summary>
    /// <param name="region">The region being mapped.</param>
    /// <param name="planet">The planet being mapped.</param>
    /// <param name="position">The position.</param>
    /// <returns>
    /// The latitude and longitude of the given coordinates, in radians.
    /// </returns>
    public static (double latitude, double longitude) GetLatLonFromLocalPosition(
        this SurfaceRegion region,
        Planetoid planet,
        Vector3 position)
    {
        var pos = region.PlanetaryPosition + position;
        return (planet.VectorToLatitude(pos), planet.VectorToLongitude(pos));
    }

    /// <summary>
    /// Calculates the position that corresponds to a set of coordinates from a cylindrical
    /// equal-area projection.
    /// </summary>
    /// <param name="region">The region being mapped.</param>
    /// <param name="planet">The planet being mapped.</param>
    /// <param name="x">The x coordinate of a point on a cylindrical equal-area projection, with
    /// zero as the westernmost point.</param>
    /// <param name="y">The y coordinate of a point on a cylindrical equal-area projection, with
    /// zero as the northernmost point.</param>
    /// <param name="resolution">The vertical resolution of the projection.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection is assumed to be a cylindrical equal-area
    /// projection. Otherwise, an equirectangular projection is assumed.
    /// </param>
    /// <returns>
    /// The local position of the given coordinates, as a vector.
    /// </returns>
    public static Vector3<HugeNumber> GetLocalPositionForMapProjection(
        this SurfaceRegion region,
        Planetoid planet,
        int x, int y,
        int resolution,
        bool equalArea = false)
    {
        var (lat, lon) = SurfaceMap.GetLatLonForMapProjection(
            x, y,
            resolution,
            region.GetProjection(planet, equalArea));
        return planet.LatitudeAndLongitudeToVector(lat, lon) - region.PlanetaryPosition;
    }

    /// <summary>
    /// Gets the precipitation at the given <paramref name="latitude"/> and <paramref
    /// name="longitude"/>, in mm/hr.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="precipitationMap">A precipitation map.</param>
    /// <param name="latitude">The latitude at which to determine precipitation.</param>
    /// <param name="longitude">The longitude at which to determine precipitation.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>
    /// The precipitation at the given <paramref name="latitude"/> and <paramref
    /// name="longitude"/>, in mm/hr. Or <see cref="double.NaN"/> if the given <paramref
    /// name="latitude"/> and <paramref name="longitude"/> are not contained within this
    /// region.
    /// </returns>
    public static double GetPrecipitationAt(
        this SurfaceRegion region,
        Planetoid planet,
        Image<L16> precipitationMap,
        double latitude,
        double longitude,
        bool equalArea = false) => region.IsPositionWithin(planet, latitude, longitude)
        ? precipitationMap.GetValueFromImage(
            latitude,
            longitude,
            region.GetProjection(planet, equalArea),
            true)
            * planet.Atmosphere.MaxPrecipitation
        : double.NaN;

    /// <summary>
    /// Gets the precipitation at the given <paramref name="position"/>, in mm/hr.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="precipitationMap">A precipitation map.</param>
    /// <param name="position">The longitude at which to determine precipitation.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>
    /// The precipitation at the given <paramref name="position"/>, in mm/hr. Or <see
    /// cref="double.NaN"/> if the given <paramref name="position"/> is not contained within
    /// this region.
    /// </returns>
    public static double GetPrecipitationAt(
        this SurfaceRegion region,
        Planetoid planet,
        Image<L16> precipitationMap,
        Vector3 position,
        bool equalArea = false)
        => region.GetPrecipitationAt(
            planet,
            precipitationMap,
            planet.VectorToLatitude(position),
            planet.VectorToLongitude(position),
            equalArea);

    /// <summary>
    /// Produces precipitation and snowfall map projections of this region.
    /// </summary>
    /// <param name="region">The region being mapped.</param>
    /// <param name="planet">The planet being mapped.</param>
    /// <param name="winterTemperatures">A planetary winter temperature map.</param>
    /// <param name="summerTemperatures">A planetary summer temperature map.</param>
    /// <param name="resolution">The vertical resolution of the projection.</param>
    /// <param name="steps">
    /// The number of maps to generate internally (representing evenly spaced "seasons" during a
    /// year, starting and ending at the winter solstice in the northern hemisphere), before
    /// averaging them into a single image.
    /// </param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>
    /// Projected maps of precipitation and snowfall. Pixel luminosity indicates precipitation
    /// in mm/hr, relative to the <see cref="Atmosphere.MaxPrecipitation"/> and <see
    /// cref="Atmosphere.MaxSnowfall"/> of the <paramref name="planet"/>'s <see
    /// cref="Atmosphere"/>.
    /// </returns>
    public static (Image<L16>[] precipitationMaps, Image<L16>[] snowfallMaps) GetPrecipitationAndSnowfallMaps(
        this SurfaceRegion region,
        Planetoid planet,
        Image<L16> winterTemperatures,
        Image<L16> summerTemperatures,
        int resolution,
        int steps = 1,
        bool equalArea = false)
    {
        var projection = region.GetProjection(planet, equalArea);
        return planet.GetPrecipitationAndSnowfallMaps(
                winterTemperatures,
                summerTemperatures,
                resolution,
                steps,
                projection,
                projection);
    }

    /// <summary>
    /// Gets the map projection options which represent this region.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The planet on which this region occurs.</param>
    /// <param name="equalArea">Whether to generate cylindrical equal-area options.</param>
    /// <returns>A <see cref="MapProjectionOptions"/> instance.</returns>
    public static MapProjectionOptions GetProjection(this SurfaceRegion region, Planetoid planet, bool equalArea = false)
        => new(planet.VectorToLongitude(region.PlanetaryPosition),
            planet.VectorToLatitude(region.PlanetaryPosition),
            Range: (double)((Frustum<HugeNumber>)region.Shape).FieldOfViewAngle,
            EqualArea: equalArea);

    /// <summary>
    /// Calculates the x and y coordinates on an equirectangular projection that correspond to a
    /// given <paramref name="position"/> relative to the center of the specified mapped
    /// <paramref name="region"/>, where 0,0 is at the top, left and is the northwestern-most
    /// point on the map.
    /// </summary>
    /// <param name="region">The region being mapped.</param>
    /// <param name="planet">The planet being mapped.</param>
    /// <param name="position">
    /// A position relative to the center of <paramref name="region"/>.
    /// </param>
    /// <param name="xResolution">The horizontal resolution of the projection.</param>
    /// <param name="yResolution">The vertical resolution of the projection.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>
    /// The latitude and longitude of the given coordinates, in radians.
    /// </returns>
    public static (int x, int y) GetProjectionFromLocalPosition(
        this SurfaceRegion region,
        Planetoid planet,
        Vector3 position,
        int xResolution,
        int yResolution,
        bool equalArea = false)
    {
        var pos = region.PlanetaryPosition + position;
        return SurfaceMap.GetProjectionFromLatLong(
            planet.VectorToLatitude(pos),
            planet.VectorToLongitude(pos),
            xResolution,
            yResolution,
            region.GetProjection(planet, equalArea));
    }

    /// <summary>
    /// Calculates the approximate distance by which the given point is separated from its
    /// neighbors on a map projection with the given characteristics, by transforming the point
    /// and its nearest neighbors to latitude and longitude, and averaging the distances between
    /// them.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="x">The x coordinate of a point on a map projection, with zero as the
    /// westernmost point.</param>
    /// <param name="y">The y coordinate of a point on a map projection, with zero as the
    /// northernmost point.</param>
    /// <param name="resolution">The vertical resolution of the projection.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>The area of the given point, in m².</returns>
    public static HugeNumber GetSeparationOfPoint(
        this SurfaceRegion region,
        Planetoid planet,
        int x, int y,
        int resolution,
        bool equalArea = false)
        => SurfaceMap.GetSeparationOfPointFromRadiusSquared(
            planet.RadiusSquared,
            x, y,
            resolution,
            region.GetProjection(planet, equalArea));

    /// <summary>
    /// Gets the snowfall at the given <paramref name="latitude"/> and <paramref
    /// name="longitude"/>, in mm/hr.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="snowfallMap">A snowfall map.</param>
    /// <param name="latitude">The latitude at which to determine snowfall.</param>
    /// <param name="longitude">The longitude at which to determine snowfall.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>
    /// The snowfall at the given <paramref name="latitude"/> and <paramref
    /// name="longitude"/>, in mm/hr. Or <see cref="double.NaN"/> if the given <paramref
    /// name="latitude"/> and <paramref name="longitude"/> are not contained within this
    /// region.
    /// </returns>
    public static double GetSnowfallAt(
        this SurfaceRegion region,
        Planetoid planet,
        Image<L16> snowfallMap,
        double latitude,
        double longitude,
        bool equalArea = false) => region.IsPositionWithin(planet, latitude, longitude)
        ? snowfallMap.GetValueFromImage(
            latitude,
            longitude,
            region.GetProjection(planet, equalArea))
            * planet.Atmosphere.MaxSnowfall
        : double.NaN;

    /// <summary>
    /// Gets the snowfall at the given <paramref name="position"/>, in mm/hr.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="snowfallMap">A snowfall map.</param>
    /// <param name="position">The longitude at which to determine snowfall.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>
    /// The snowfall at the given <paramref name="position"/>, in mm/hr. Or <see
    /// cref="double.NaN"/> if the given <paramref name="position"/> is not contained within
    /// this region.
    /// </returns>
    public static double GetSnowfallAt(
        this SurfaceRegion region,
        Planetoid planet,
        Image<L16> snowfallMap,
        Vector3 position,
        bool equalArea = false)
    {
        var pos = region.PlanetaryPosition + position;
        return region.GetSnowfallAt(
            planet,
            snowfallMap,
            planet.VectorToLatitude(pos),
            planet.VectorToLongitude(pos),
            equalArea);
    }

    /// <summary>
    /// Gets the surface temperature at the given position, in K.
    /// </summary>
    /// <param name="region">The region being mapped.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="temperatureMap">A temperature map.</param>
    /// <param name="position">
    /// A position relative to the center of <paramref name="region"/>.
    /// </param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>The surface temperature, in K.</returns>
    public static double GetSurfaceTemperature(
        this SurfaceRegion region,
        Planetoid planet,
        Image<L16> temperatureMap,
        Vector3 position,
        bool equalArea = false)
    {
        var pos = region.PlanetaryPosition + position;
        return region.GetSurfaceTemperature(
            planet,
            temperatureMap,
            planet.VectorToLatitude(pos),
            planet.VectorToLongitude(pos),
            equalArea);
    }

    /// <summary>
    /// Gets the surface temperature at the given position, in K.
    /// </summary>
    /// <param name="region">The region being mapped.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="temperatureMap">A temperature map.</param>
    /// <param name="latitude">
    /// The latitude at which to calculate the temperature, in radians.
    /// </param>
    /// <param name="longitude">
    /// The latitude at which to calculate the temperature, in radians.
    /// </param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>The surface temperature, in K.</returns>
    public static double GetSurfaceTemperature(
        this SurfaceRegion region,
        Planetoid planet,
        Image<L16> temperatureMap,
        double latitude,
        double longitude,
        bool equalArea = false) => region.IsPositionWithin(planet, latitude, longitude)
        ? temperatureMap.GetTemperature(
            latitude,
            longitude,
            region.GetProjection(planet, equalArea))
        : double.NaN;

    /// <summary>
    /// Calculates the surface temperature at the given position, in K.
    /// </summary>
    /// <param name="region">The region being mapped.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="winterTemperatures">A winter temperature map.</param>
    /// <param name="summerTemperatures">A summer temperature map.</param>
    /// <param name="proportionOfYear">
    /// The proportion of a full year at which the map is to be generated, assuming a year
    /// begins and ends at the winter solstice in the northern hemisphere.
    /// </param>
    /// <param name="latitude">
    /// The latitude at which to calculate the temperature, in radians.
    /// </param>
    /// <param name="longitude">
    /// The latitude at which to calculate the temperature, in radians.
    /// </param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>The surface temperature, in K.</returns>
    public static double GetSurfaceTemperature(
        this SurfaceRegion region,
        Planetoid planet,
        Image<L16> winterTemperatures,
        Image<L16> summerTemperatures,
        double proportionOfYear,
        double latitude,
        double longitude,
        bool equalArea = false)
    {
        var (x, y) = SurfaceMap.GetProjectionFromLatLong(
            latitude,
            longitude,
            winterTemperatures.Width,
            winterTemperatures.Height,
            region.GetProjection(planet, equalArea));
        return SurfaceMapImage.InterpolateAmongImages(winterTemperatures, summerTemperatures, proportionOfYear, x, y)
            * SurfaceMapImage.TemperatureScaleFactor;
    }

    /// <summary>
    /// Calculates the range of temperatures at the given <paramref name="latitude"/> and
    /// <paramref name="longitude"/>, in K.
    /// </summary>
    /// <param name="region">The region being mapped.</param>
    /// <param name="planet">The planet being mapped.</param>
    /// <param name="winterTemperatures">A winter temperature map.</param>
    /// <param name="summerTemperatures">A summer temperature map.</param>
    /// <param name="latitude">
    /// The latitude at which to calculate the temperature range, in radians.
    /// </param>
    /// <param name="longitude">
    /// The latitude at which to calculate the temperature range, in radians.
    /// </param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>
    /// A <see cref="FloatRange"/> giving the range of temperatures at the given <paramref
    /// name="latitude"/> and <paramref name="longitude"/>, in K.
    /// </returns>
    public static FloatRange GetSurfaceTemperature(
        this SurfaceRegion region,
        Planetoid planet,
        Image<L16> winterTemperatures,
        Image<L16> summerTemperatures,
        double latitude,
        double longitude,
        bool equalArea = false)
    {
        var options = region.GetProjection(planet, equalArea);
        var winterTemperature = winterTemperatures.GetTemperature(latitude, longitude, options);
        var summerTemperature = summerTemperatures.GetTemperature(latitude, longitude, options);
        if (winterTemperature <= summerTemperature)
        {
            return new FloatRange((float)winterTemperature, (float)summerTemperature);
        }
        return new FloatRange((float)summerTemperature, (float)winterTemperature);
    }

    /// <summary>
    /// Generates new winter and summer temperature map projections of this region.
    /// </summary>
    /// <param name="region">The region being mapped.</param>
    /// <param name="planet">The planet being mapped.</param>
    /// <param name="elevationMap">An elevation map.</param>
    /// <param name="resolution">The vertical resolution.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>Winter and summer temperature map images.</returns>
    public static (Image<L16> winter, Image<L16> summer) GetTemperatureMaps(
        this SurfaceRegion region,
        Planetoid planet,
        Image<L16> elevationMap,
        int resolution,
        bool equalArea = false)
    {
        var projection = region.GetProjection(planet, equalArea);
        return planet.GetTemperatureMaps(elevationMap, resolution, projection, projection);
    }

    /// <summary>
    /// Gets the value for a <paramref name="position"/> in a <paramref name="region"/> from a
    /// set of values.
    /// </summary>
    /// <param name="region">The mapped region.</param>
    /// <param name="planet">The mapped planet.</param>
    /// <param name="position">A position relative to the center of <paramref
    /// name="region"/>.</param>
    /// <param name="values">A set of values.</param>
    /// <param name="equalArea">
    /// If <see langword="true"/> the projection will be a cylindrical equal-area projection.
    /// Otherwise, an equirectangular projection will be used.
    /// </param>
    /// <returns>The value for a <paramref name="position"/> in a <paramref name="region"/> from
    /// a set of values.</returns>
    public static T GetValueFromLocalPosition<T>(
        this SurfaceRegion region,
        Planetoid planet,
        Vector3 position,
        T[,] values,
        bool equalArea = false)
    {
        var (x, y) = region.GetProjectionFromLocalPosition(
            planet,
            position,
            values.GetLength(0),
            values.GetLength(1),
            equalArea);
        return values[x, y];
    }
}
