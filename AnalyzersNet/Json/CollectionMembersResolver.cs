using System.Text.Json.Serialization.Metadata;

namespace Specs.Json;

/// <summary>
/// Provides functionality to customize JSON serialization behavior for array properties,
/// specifically to ignore serialization of empty arrays.
/// </summary>
internal static class CollectionMembersResolver
{
    /// <summary>
    /// Configures the specified <see cref="JsonTypeInfo"/> to ignore properties of array type
    /// when they are empty during JSON serialization.
    /// </summary>
    /// <param name="typeInfo">
    /// The <see cref="JsonTypeInfo"/> to modify, typically representing a type being serialized.
    /// </param>
    public static void IgnoreEmpty(JsonTypeInfo typeInfo)
    {
        foreach (var prop in typeInfo.Properties.Where(p => IsCollection(p.PropertyType)))
        {
            prop.ShouldSerialize = ShouldSerialize;
        }
    }

    [Pure]
    private static bool IsCollection(Type type)
        => type == typeof(string)
        || type.IsArray
        || IsICollection(type)
        || type.GetInterfaces().Any(IsICollection);

    [Pure]
    private static bool IsICollection(Type type)
        => IsCollection_T(type)
        || IsReadOnlyCollection_T(type);

    [Pure]
    private static bool IsReadOnlyCollection_T(Type type)
        => type.IsGenericType
        && type.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>);

    [Pure]
    private static bool IsCollection_T(Type type)
        => type.IsGenericType
        && type.GetGenericTypeDefinition() == typeof(ICollection<>);

    [Pure]
    private static bool ShouldSerialize(object _, object? collection) => HasAny(collection as IEnumerable);

    [Pure]
    private static bool HasAny(IEnumerable? collection) => collection?.GetEnumerator() is { } iterator && iterator.MoveNext();
}
