using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Text.Json.Serialization;
using Tavenem.Chemistry;
using Tavenem.Mathematics;
using Tavenem.Universe.Climate;
using Tavenem.Universe.Space;

namespace Tavenem.Universe.Maps;

/// <summary>
/// A collection of weather maps providing yearlong climate data.
/// </summary>
/// <param name="Biome">The overall <see cref="BiomeType"/> of the area.</param>
/// <param name="BiomeMap">
/// A two-dimensional array corresponding to points on an equirectangular projected map of a
/// terrestrial planet's surface. The first index corresponds to the X coordinate, and the
/// second index corresponds to the Y coordinate. The values represent <see
/// cref="BiomeType"/>.
/// </param>
/// <param name="Climate">
/// The overall <see cref="ClimateType"/> of the area, based on average annual temperature.
/// </param>
/// <param name="ClimateMap">
/// A two-dimensional array corresponding to points on an equirectangular projected map of a
/// terrestrial planet's surface. The first index corresponds to the X coordinate, and the
/// second index corresponds to the Y coordinate. The values represent <see
/// cref="ClimateType"/>, based on average annual temperature.
/// </param>
/// <param name="SeaIceRangeMap">
/// A two-dimensional array corresponding to points on an equirectangular projected map of a
/// terrestrial planet's surface. The first index corresponds to the X coordinate, and the
/// second index corresponds to the Y coordinate. The values represent the proportion of the
/// year during which there is persistent sea ice.
/// </param>
[method: JsonConstructor]
public readonly record struct WeatherMaps(
    BiomeType Biome,
    BiomeType[][] BiomeMap,
    ClimateType Climate,
    ClimateType[][] ClimateMap,
    FloatRange[][] SeaIceRangeMap)
{
    /// <summary>
    /// The length of the "X" (0-index) dimension of the maps.
    /// </summary>
    [JsonIgnore]
    public int XLength { get; } = ClimateMap.Length != BiomeMap.Length
        || SeaIceRangeMap.Length != BiomeMap.Length
        ? throw new ArgumentException("All X lengths must be the same")
        : BiomeMap.Length;

    /// <summary>
    /// The length of the "Y" (1-index) dimension of the maps.
    /// </summary>
    [JsonIgnore]
    public int YLength { get; } = BiomeMap switch
    {
        { Length: var l } when l != ClimateMap.Length
            || l != SeaIceRangeMap.Length
            => throw new ArgumentException("All X lengths must be the same"),
        { Length: 0 } => 0,
        [var first, ..] when first.Length != ClimateMap[0].Length
            || first.Length != SeaIceRangeMap[0].Length
            => throw new ArgumentException("All Y lengths must be the same"),
        _ => BiomeMap[0].Length,
    };

    /// <summary>
    /// Initializes a new instance of <see cref="WeatherMaps"/>.
    /// </summary>
    /// <param name="planet">The planet being mapped.</param>
    /// <param name="elevationMap">An elevation map.</param>
    /// <param name="winterTemperatureMap">A winter temperature map.</param>
    /// <param name="summerTemperatureMap">A summer temperature map.</param>
    /// <param name="precipitationMap">A precipitation map.</param>
    /// <param name="resolution">The intended vertical resolution of the maps.</param>
    /// <param name="options">
    /// The map projection options to use. All the map images must have been generated using
    /// these same options, or thew results will not be accurate.
    /// </param>
    public WeatherMaps(
        Planetoid planet,
        Image<L16> elevationMap,
        Image<L16> winterTemperatureMap,
        Image<L16> summerTemperatureMap,
        Image<L16> precipitationMap,
        int resolution,
        MapProjectionOptions? options = null) : this(
            BiomeType.None,
            [],
            ClimateType.None,
            [],
            [])
    {
        var projection = options ?? MapProjectionOptions.Default;

        var xLength = (int)Math.Floor(projection.AspectRatio * resolution);
        XLength = xLength;
        YLength = resolution;

        var biomeMap = new BiomeType[xLength][];
        var climateMap = new ClimateType[xLength][];
        var humidityMap = new HumidityType[xLength][];
        var seaIceRangeMap = new FloatRange[xLength][];

        for (var x = 0; x < xLength; x++)
        {
            biomeMap[x] = new BiomeType[resolution];
            climateMap[x] = new ClimateType[resolution];
            humidityMap[x] = new HumidityType[resolution];
            seaIceRangeMap[x] = new FloatRange[resolution];
        }

        var scale = SurfaceMap.GetScale(resolution, projection.Range);
        var stretch = scale / projection.ScaleFactor;
        var elevationScale = SurfaceMap.GetScale(elevationMap.Height, projection.Range);
        var winterScale = winterTemperatureMap.Height == elevationMap.Height
            ? elevationScale
            : SurfaceMap.GetScale(winterTemperatureMap.Height, projection.Range);
        var summerScale = summerTemperatureMap.Height == elevationMap.Height
            ? elevationScale
            : SurfaceMap.GetScale(summerTemperatureMap.Height, projection.Range);
        var precipitationScale = precipitationMap.Height == elevationMap.Height
            ? elevationScale
            : SurfaceMap.GetScale(precipitationMap.Height, projection.Range);

        var elevationYs = new int[resolution];
        var normalizedElevations = new double[resolution][];
        var totalElevation = 0.0;
        var xToEX = new Dictionary<int, int>();
        var xToWX = new Dictionary<int, int>();
        var xToSX = new Dictionary<int, int>();
        var xToPX = new Dictionary<int, int>();
        elevationMap.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < resolution; y++)
            {
                normalizedElevations[y] = new double[xLength];

                var latitude = projection.EqualArea
                    ? SurfaceMap.GetLatitudeOfCylindricalEqualAreaProjection(y, resolution, scale, projection)
                    : SurfaceMap.GetLatitudeOfEquirectangularProjection(y, resolution, scale, projection);

                elevationYs[y] = projection.EqualArea
                    ? SurfaceMap.GetCylindricalEqualAreaYFromLatWithScale(latitude, elevationMap.Height, elevationScale, projection)
                    : SurfaceMap.GetEquirectangularYFromLatWithScale(latitude, elevationMap.Height, elevationScale, projection);
                var elevationSpan = accessor.GetRowSpan(elevationYs[y]);
                for (var x = 0; x < xLength; x++)
                {
                    if (!xToEX.TryGetValue(x, out var elevationX))
                    {
                        var longitude = projection.EqualArea
                            ? SurfaceMap.GetLongitudeOfCylindricalEqualAreaProjection(x, xLength, scale, projection)
                            : SurfaceMap.GetLongitudeOfEquirectangularProjection(x, xLength, stretch, projection);

                        elevationX = projection.EqualArea
                            ? SurfaceMap.GetCylindricalEqualAreaXFromLonWithScale(longitude, elevationMap.Width, elevationScale, projection)
                            : SurfaceMap.GetEquirectangularXFromLonWithScale(longitude, elevationMap.Width, elevationScale, projection);
                        int wX;
                        if (winterTemperatureMap.Width == elevationMap.Width)
                        {
                            wX = elevationX;
                        }
                        else if (projection.EqualArea)
                        {
                            wX = SurfaceMap.GetCylindricalEqualAreaXFromLonWithScale(longitude, winterTemperatureMap.Width, winterScale, projection);
                        }
                        else
                        {
                            wX = SurfaceMap.GetEquirectangularXFromLonWithScale(longitude, winterTemperatureMap.Width, winterScale, projection);
                        }

                        int sX;
                        if (summerTemperatureMap.Width == elevationMap.Width)
                        {
                            sX = elevationX;
                        }
                        else if (projection.EqualArea)
                        {
                            sX = SurfaceMap.GetCylindricalEqualAreaXFromLonWithScale(longitude, summerTemperatureMap.Width, summerScale, projection);
                        }
                        else
                        {
                            sX = SurfaceMap.GetEquirectangularXFromLonWithScale(longitude, summerTemperatureMap.Width, summerScale, projection);
                        }

                        int pX;
                        if (precipitationMap.Width == elevationMap.Width)
                        {
                            pX = elevationX;
                        }
                        else if (projection.EqualArea)
                        {
                            pX = SurfaceMap.GetCylindricalEqualAreaXFromLonWithScale(longitude, precipitationMap.Width, precipitationScale, projection);
                        }
                        else
                        {
                            pX = SurfaceMap.GetEquirectangularXFromLonWithScale(longitude, precipitationMap.Width, precipitationScale, projection);
                        }

                        xToEX.Add(x, elevationX);
                        xToWX.Add(x, wX);
                        xToSX.Add(x, sX);
                        xToPX.Add(x, pX);
                    }
                    normalizedElevations[y][x] = elevationSpan[elevationX].GetValueFromPixel_PosNeg() - planet.NormalizedSeaLevel;
                    totalElevation += normalizedElevations[y][x];
                }
            }
        });

        var minTemperature = 5000.0f;
        var maxTemperature = 0.0f;
        var totalTemperature = 0.0f;
        var totalPrecipitation = 0.0;
        winterTemperatureMap.ProcessPixelRows(
            summerTemperatureMap,
            precipitationMap,
            (winterMap, summerMap, precipitationMap) =>
            {
                for (var y = 0; y < resolution; y++)
                {
                    var latitude = projection.EqualArea
                        ? SurfaceMap.GetLatitudeOfCylindricalEqualAreaProjection(y, resolution, scale, projection)
                        : SurfaceMap.GetLatitudeOfEquirectangularProjection(y, resolution, scale, projection);

                    int winterY;
                    if (winterTemperatureMap.Height == elevationMap.Height)
                    {
                        winterY = elevationYs[y];
                    }
                    else if (projection.EqualArea)
                    {
                        winterY = SurfaceMap.GetCylindricalEqualAreaYFromLatWithScale(latitude, winterTemperatureMap.Height, winterScale, projection);
                    }
                    else
                    {
                        winterY = SurfaceMap.GetEquirectangularYFromLatWithScale(latitude, winterTemperatureMap.Height, winterScale, projection);
                    }

                    var winterSpan = winterMap.GetRowSpan(winterY);

                    int summerY;
                    if (summerTemperatureMap.Height == elevationMap.Height)
                    {
                        summerY = elevationYs[y];
                    }
                    else if (projection.EqualArea)
                    {
                        summerY = SurfaceMap.GetCylindricalEqualAreaYFromLatWithScale(latitude, summerTemperatureMap.Height, summerScale, projection);
                    }
                    else
                    {
                        summerY = SurfaceMap.GetEquirectangularYFromLatWithScale(latitude, summerTemperatureMap.Height, summerScale, projection);
                    }

                    var summerSpan = summerMap.GetRowSpan(summerY);

                    int precipitationY;
                    if (precipitationMap.Height == elevationMap.Height)
                    {
                        precipitationY = elevationYs[y];
                    }
                    else if (projection.EqualArea)
                    {
                        precipitationY = SurfaceMap.GetCylindricalEqualAreaYFromLatWithScale(latitude, precipitationMap.Height, precipitationScale, projection);
                    }
                    else
                    {
                        precipitationY = SurfaceMap.GetEquirectangularYFromLatWithScale(latitude, precipitationMap.Height, precipitationScale, projection);
                    }

                    var precipitationSpan = precipitationMap.GetRowSpan(precipitationY);

                    for (var x = 0; x < xLength; x++)
                    {
                        var winterX = xToWX[x];
                        var summerX = xToSX[x];
                        var precipitationX = xToPX[x];

                        var winterTemperature = (float)(winterSpan[winterX].GetValueFromPixel_Pos() * SurfaceMapImage.TemperatureScaleFactor);
                        var summerTemperature = (float)(summerSpan[summerX].GetValueFromPixel_Pos() * SurfaceMapImage.TemperatureScaleFactor);
                        minTemperature = Math.Min(minTemperature, Math.Min(winterTemperature, summerTemperature));
                        maxTemperature = Math.Max(maxTemperature, Math.Max(winterTemperature, summerTemperature));
                        totalTemperature += (minTemperature + maxTemperature) / 2;

                        var precipitationValue = precipitationSpan[precipitationX].GetValueFromPixel_Pos();
                        var precipitation = precipitationValue * planet.Atmosphere.MaxPrecipitation;
                        totalPrecipitation += precipitationValue;

                        climateMap[x][y] = Universe.Climate.Climate.GetClimateType(new FloatRange(
                            Math.Min(winterTemperature, summerTemperature),
                            Math.Max(winterTemperature, summerTemperature)));
                        humidityMap[x][y] = Universe.Climate.Climate.GetHumidityType(precipitation);
                        biomeMap[x][y] = Universe.Climate.Climate.GetBiomeType(
                            climateMap[x][y],
                            humidityMap[x][y],
                            normalizedElevations[y][x]);

                        if (normalizedElevations[y][x] > 0
                            || (summerTemperature >= Substances.All.Seawater.MeltingPoint
                            && winterTemperature >= Substances.All.Seawater.MeltingPoint))
                        {
                            continue;
                        }

                        if (summerTemperature < Substances.All.Seawater.MeltingPoint
                            && winterTemperature < Substances.All.Seawater.MeltingPoint)
                        {
                            seaIceRangeMap[x][y] = FloatRange.ZeroToOne;
                            continue;
                        }

                        var freezeProportion = ((summerTemperature >= winterTemperature
                            ? winterTemperature.InverseLerp(summerTemperature, (float)(Substances.All.Seawater.MeltingPoint ?? 0))
                            : summerTemperature.InverseLerp(winterTemperature, (float)(Substances.All.Seawater.MeltingPoint ?? 0))) * 0.8f) - 0.1f;
                        if (freezeProportion <= 0
                            || float.IsNaN(freezeProportion))
                        {
                            continue;
                        }

                        var freezeStart = 1 - (freezeProportion / 4);
                        var iceMeltFinish = freezeProportion * 3 / 4;
                        if (latitude < 0)
                        {
                            freezeStart += 0.5f;
                            if (freezeStart > 1)
                            {
                                freezeStart--;
                            }

                            iceMeltFinish += 0.5f;
                            if (iceMeltFinish > 1)
                            {
                                iceMeltFinish--;
                            }
                        }
                        seaIceRangeMap[x][y] = new FloatRange(freezeStart, iceMeltFinish);
                    }
                }
            });

        BiomeMap = biomeMap;
        ClimateMap = climateMap;
        SeaIceRangeMap = seaIceRangeMap;
        Climate = Universe.Climate.Climate.GetClimateType(new FloatRange(minTemperature, totalTemperature / (xLength * resolution), maxTemperature));
        var humidity = Universe.Climate.Climate.GetHumidityType(totalPrecipitation / (xLength * resolution) * planet.Atmosphere.MaxPrecipitation);
        Biome = Universe.Climate.Climate.GetBiomeType(Climate, humidity, totalElevation / (xLength * resolution) * planet.MaxElevation);
    }

    /// <inheritdoc />
    public bool Equals(WeatherMaps other)
        => Biome == other.Biome
        && Climate.Equals(other.Climate)
        && WeatherArraysEqual(BiomeMap, other.BiomeMap)
        && WeatherArraysEqual(ClimateMap, other.ClimateMap)
        && WeatherArraysEqual(SeaIceRangeMap, other.SeaIceRangeMap);

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(WeatherMaps? other)
        => other.HasValue && Equals(other.Value);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Biome.GetHashCode());
        hashCode.Add(Climate.GetHashCode());
        hashCode.Add(GetWeatherArrayHashCode(BiomeMap));
        hashCode.Add(GetWeatherArrayHashCode(ClimateMap));
        hashCode.Add(GetWeatherArrayHashCode(SeaIceRangeMap));
        return hashCode.ToHashCode();
    }

    private static int GetWeatherArrayHashCode<T>(T[][] array)
    {
        if (array is null)
        {
            return 0;
        }
        unchecked
        {
            var value = array.Length;
            var outerCount = Math.Min(5, array.Length);
            for (var i = 0; i < outerCount; i++)
            {
                value = (value * 367) ^ array[i].Length;
                var innerCount = Math.Min(5, array[i].Length);
                for (var j = 0; j < innerCount; j++)
                {
                    value = (value * 367) ^ (array[i][j]?.GetHashCode() ?? 0);
                }
            }
            return 367 * value;
        }
    }

    private static bool WeatherArraysEqual<T>(T[][] first, T[][] second)
    {
        if (first is null
            || second is null)
        {
            return false;
        }
        if (first.Length != second.Length)
        {
            return false;
        }
        for (var i = 0; i < first.Length; i++)
        {
            if (first[i].Length != second[i].Length
                || !first[i].SequenceEqual(second[i]))
            {
                return false;
            }
        }
        return true;
    }
}
