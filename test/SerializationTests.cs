using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;
using Tavenem.Universe.Maps;
using Tavenem.Universe.Space;

namespace Tavenem.Universe.Test;

[TestClass]
public class SerializationTests
{
    [TestMethod]
    public void HillShadingOptionsTest()
    {
        var value = new HillShadingOptions(true, true);

        var json = JsonSerializer.Serialize(value);
        Console.WriteLine();
        Console.WriteLine(json);
        var deserialized = JsonSerializer.Deserialize<HillShadingOptions>(json);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(value, deserialized);
        Assert.AreEqual(json, JsonSerializer.Serialize(deserialized));

        json = JsonSerializer.Serialize(value, UniverseMappingSourceGenerationContext.Default.HillShadingOptions);
        Console.WriteLine();
        Console.WriteLine(json);
        deserialized = JsonSerializer.Deserialize(json, UniverseMappingSourceGenerationContext.Default.HillShadingOptions);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(value, deserialized);
        Assert.AreEqual(
            json,
            JsonSerializer.Serialize(deserialized, UniverseMappingSourceGenerationContext.Default.HillShadingOptions));
    }

    [TestMethod]
    public void MapProjectionOptionsTest()
    {
        var value = new MapProjectionOptions(
            0.5,
            0.2,
            0.1,
            0.2,
            true);

        var json = JsonSerializer.Serialize(value);
        Console.WriteLine();
        Console.WriteLine(json);
        var deserialized = JsonSerializer.Deserialize<MapProjectionOptions>(json);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(value, deserialized);
        Assert.AreEqual(json, JsonSerializer.Serialize(deserialized));

        json = JsonSerializer.Serialize(value, UniverseMappingSourceGenerationContext.Default.MapProjectionOptions);
        Console.WriteLine();
        Console.WriteLine(json);
        deserialized = JsonSerializer.Deserialize(json, UniverseMappingSourceGenerationContext.Default.MapProjectionOptions);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(value, deserialized);
        Assert.AreEqual(
            json,
            JsonSerializer.Serialize(deserialized, UniverseMappingSourceGenerationContext.Default.MapProjectionOptions));
    }

    [TestMethod]
    public void WeatherMapsTest()
    {
        const int ProjectionResolution = 640;
        const int PrecipitationMapResolution = 180;
        const int Seasons = 4;
        var projectionXResolution = (int)Math.Floor(ProjectionResolution * MapProjectionOptions.Default.AspectRatio);

        var planet = Planetoid.GetPlanetForSunlikeStar(out _);
        Assert.IsNotNull(planet);

        var elevationMap = planet.GetElevationMap(ProjectionResolution);
        var (winterTemperatureMap, summerTemperatureMap) = planet
            .GetTemperatureMaps(elevationMap, ProjectionResolution);
        var (precipitationMaps, snowfallMaps) = planet
            .GetPrecipitationAndSnowfallMaps(
                winterTemperatureMap,
                summerTemperatureMap,
                PrecipitationMapResolution,
                Seasons);
        for (var i = 0; i < snowfallMaps.Length; i++)
        {
            snowfallMaps[i].Dispose();
        }
        if (PrecipitationMapResolution < ProjectionResolution)
        {
            for (var i = 0; i < Seasons; i++)
            {
                precipitationMaps[i].Mutate(x => x.Resize(projectionXResolution, ProjectionResolution));
            }
        }
        var precipitationMap = SurfaceMapImage.AverageImages(precipitationMaps);

        var value = new WeatherMaps(
            planet,
            elevationMap,
            winterTemperatureMap,
            summerTemperatureMap,
            precipitationMap,
            ProjectionResolution);

        elevationMap.Dispose();
        winterTemperatureMap.Dispose();
        summerTemperatureMap.Dispose();
        for (var i = 0; i < precipitationMaps.Length; i++)
        {
            precipitationMaps[i].Dispose();
        }
        precipitationMap.Dispose();

        var json = JsonSerializer.Serialize(value);
        Console.WriteLine();
        Console.WriteLine(json);
        var deserialized = JsonSerializer.Deserialize<WeatherMaps>(json);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(value, deserialized);
        Assert.AreEqual(json, JsonSerializer.Serialize(deserialized));

        json = JsonSerializer.Serialize(value, UniverseMappingSourceGenerationContext.Default.WeatherMaps);
        Console.WriteLine();
        Console.WriteLine(json);
        deserialized = JsonSerializer.Deserialize(json, UniverseMappingSourceGenerationContext.Default.WeatherMaps);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(value, deserialized);
        Assert.AreEqual(
            json,
            JsonSerializer.Serialize(deserialized, UniverseMappingSourceGenerationContext.Default.WeatherMaps));
    }
}
