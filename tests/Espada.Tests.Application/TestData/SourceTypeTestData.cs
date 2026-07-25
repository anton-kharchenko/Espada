using Espada.Domain.Enums;
using System.Reflection;

namespace Espada.Tests.Application.TestData;

internal static class SourceTypeTestData
{
    public static SourceType Any { get; } = ResolveAnySourceType();

    private static SourceType ResolveAnySourceType()
    {
        SourceType? sourceType = typeof(SourceType)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(SourceType))
                .Select(field => field.GetValue(null))
                .OfType<SourceType>()
                .FirstOrDefault();

        return sourceType ?? throw new InvalidOperationException("SourceType must declare at least one public static value.");
    }
}