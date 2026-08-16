// InventoryModule.Packer/InstanceDataServiceProvider.cs
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace InventoryModule.Packer;

/// <summary>
/// Base service provider for instance data packing operations
/// Contains shared utilities and type detection logic
/// </summary>
public abstract class InstanceDataServiceProvider
{
    // Cache for supported types using ImmutableHashSet for performance
    private static readonly ImmutableHashSet<Type> _supportedPrimitives = new HashSet<Type>
    {
        typeof(bool), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
        typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float),
        typeof(double), typeof(decimal), typeof(char), typeof(DateTime),
        typeof(TimeSpan), typeof(Guid)
    }.ToImmutableHashSet();

    // Cache for collection types using Lazy initialization
    private static readonly ImmutableHashSet<Type> _collectionTypes = new HashSet<Type>
    {
        typeof(IEnumerable<>),
        typeof(IList<>),
        typeof(ICollection<>),
        typeof(IReadOnlyList<>),
        typeof(IReadOnlyCollection<>)
    }.ToImmutableHashSet();

    // Thread-safe type cache
    private readonly Dictionary<Type, DataType> _typeCache = new();

    /// <summary>
    /// Check if a type is a supported primitive
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsPrimitiveOrString(Type type) =>
        type.IsPrimitive || type == typeof(string) || _supportedPrimitives.Contains(type);

    /// <summary>
    /// Check if a type is a collection (List, Array, Dictionary)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsCollection(Type type) =>
        type.IsArray ||
        (type.IsGenericType && (
            type.GetGenericTypeDefinition() == typeof(List<>) ||
            type.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
            type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
            _collectionTypes.Any(ct => type.GetGenericTypeDefinition() == ct)
        ));

    /// <summary>
    /// Get the data type classification with caching
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected DataType GetDataType(Type type)
    {
        if (_typeCache.TryGetValue(type, out var dataType))
            return dataType;

        dataType = DataType.FromType(type);
        _typeCache[type] = dataType;
        return dataType;
    }

    /// <summary>
    /// Check if a type is supported for serialization
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsSupportedType(Type type) => GetDataType(type) switch
    {
        DataType.Custom => false,
        DataType.Unsupported => false,
        _ => true
    };

    /// <summary>
    /// Get the element type from an array or list type
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Type? GetElementType(Type type) => type switch
    {
        _ when type.IsArray => type.GetElementType(),
        _ when type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) 
            => type.GetGenericArguments()[0],
        _ => null
    };

    /// <summary>
    /// Get the key and value types from a dictionary
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected (Type? Key, Type? Value) GetDictionaryTypes(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var args = type.GetGenericArguments();
            return (args[0], args[1]);
        }
        return (null, null);
    }

    /// <summary>
    /// Clear the type cache (useful for memory management)
    /// </summary>
    protected void ClearTypeCache() => _typeCache.Clear();

    // Abstract methods to be implemented by derived classes
    protected abstract void WriteNull();
    protected abstract void WriteValueType<T>(T value) where T : struct;
    protected abstract void WriteRefType<T>(T value) where T : class;
    protected abstract T ReadValueType<T>() where T : struct;
    protected abstract T? ReadRefType<T>() where T : class;
}
