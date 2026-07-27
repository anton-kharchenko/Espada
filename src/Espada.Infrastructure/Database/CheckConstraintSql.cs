using System.Globalization;
using DomainContextPriority = Espada.Domain.ValueObjects.ContextPriority;

namespace Espada.Infrastructure.Database;

internal static class CheckConstraintSql
{
    public static string ContextPriority(string columnName) => InclusiveRange(columnName, DomainContextPriority.Minimum, DomainContextPriority.Maximum);

    private static string InclusiveRange(string columnName, int minimum, int maximum)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        string quotedColumn = $"\"{columnName.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        return string.Create(CultureInfo.InvariantCulture, $"{quotedColumn} BETWEEN {minimum} AND {maximum}");
    }
}