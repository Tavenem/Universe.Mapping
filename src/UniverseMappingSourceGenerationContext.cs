using System.Text.Json.Serialization;

namespace Tavenem.Universe.Maps;

/// <summary>
/// A <see cref="JsonSerializerContext"/> for <c>Tavenem.Universe.Maps</c>
/// </summary>
[JsonSerializable(typeof(HillShadingOptions))]
[JsonSerializable(typeof(MapProjectionOptions))]
[JsonSerializable(typeof(WeatherMaps))]
public partial class UniverseMappingSourceGenerationContext
    : JsonSerializerContext
{ }
