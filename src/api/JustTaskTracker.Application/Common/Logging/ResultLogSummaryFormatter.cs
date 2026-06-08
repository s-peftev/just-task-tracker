using JustTaskTracker.Domain.Common;
using JustTaskTracker.Domain.Common.Results;
using System.Collections;
using System.Reflection;

namespace JustTaskTracker.Application.Common.Logging;

internal static class ResultLogSummaryFormatter
{
    public static string FormatSuccessResponse(Result response)
    {
        if (!response.GetType().IsGenericType)
            return "Success";

        var valueProperty = response.GetType().GetProperty(
            nameof(Result<object>.Value),
            BindingFlags.Public | BindingFlags.Instance);

        if (valueProperty?.GetValue(response) is not { } value)
            return "Success";

        return FormatValue(value);
    }

    private static string FormatValue(object value)
    {
        var type = value.GetType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PagedList<>))
            return FormatPagedList(type, value);

        if (value is IEnumerable enumerable and not string)
            return $"{type.Name}, Count={CountElements(enumerable)}";

        if (TryGetGuidId(value, out var id))
            return $"{type.Name}, Id={id}";

        return type.Name;
    }

    private static string FormatPagedList(Type type, object value)
    {
        var items = type.GetProperty(nameof(PagedList<object>.Items))?.GetValue(value) as IEnumerable;
        var metadata = type.GetProperty(nameof(PagedList<object>.Metadata))?.GetValue(value);
        var totalCount = metadata?.GetType().GetProperty(nameof(PaginationMetadata.TotalCount))?.GetValue(metadata);

        var itemCount = items is null ? 0 : CountElements(items);
        var elementType = type.GetGenericArguments()[0].Name;

        return $"PagedList<{elementType}>, Items={itemCount}, Total={totalCount}";
    }

    private static bool TryGetGuidId(object value, out Guid id)
    {
        id = default;

        var idProperty = value.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idProperty?.PropertyType != typeof(Guid))
            return false;

        id = (Guid)idProperty.GetValue(value)!;
        return true;
    }

    private static int CountElements(IEnumerable enumerable)
    {
        if (enumerable is ICollection collection)
            return collection.Count;

        var count = 0;
        foreach (var _ in enumerable)
            count++;

        return count;
    }
}
