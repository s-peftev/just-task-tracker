using System.Text.Json;
using System.Text.Json.Serialization;

namespace JustTaskTracker.Application.Common.Models;

[JsonConverter(typeof(PatchFieldJsonConverterFactory))]
public readonly struct PatchField<T> : IEquatable<PatchField<T>>
{
    public bool IsSpecified { get; init; }

    public T? Value { get; init; }

    public bool Equals(PatchField<T> other) =>
        IsSpecified == other.IsSpecified && EqualityComparer<T>.Default.Equals(Value, other.Value);

    public override bool Equals(object? obj) =>
        obj is PatchField<T> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(IsSpecified, Value);

    public static bool operator ==(PatchField<T> left, PatchField<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PatchField<T> left, PatchField<T> right)
    {
        return !(left == right);
    }
}

public sealed class PatchFieldJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType
        && typeToConvert.GetGenericTypeDefinition() == typeof(PatchField<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        return (JsonConverter)Activator.CreateInstance(
            typeof(PatchFieldJsonConverter<>).MakeGenericType(valueType))!;
    }
}

internal sealed class PatchFieldJsonConverter<T> : JsonConverter<PatchField<T>>
{
    public override PatchField<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        return new PatchField<T> { IsSpecified = true, Value = value };
    }

    public override void Write(Utf8JsonWriter writer, PatchField<T> value, JsonSerializerOptions options)
    {
        if (!value.IsSpecified)
            throw new JsonException("Cannot serialize an unspecified patch field.");

        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
