using System.ComponentModel.DataAnnotations;

namespace Espada.Tests.Api.Contracts.Validation;

internal static class ValidationTestHelper
{
    public static IReadOnlyList<ValidationResult> Validate(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        List<ValidationResult> results = [];
        ValidationContext context = new(instance);

        Validator.TryValidateObject(instance, context, results, validateAllProperties: true);

        return results;
    }

    public static bool HasErrorFor(this IEnumerable<ValidationResult> results, string memberName) =>
        results.Any(result => result.MemberNames.Contains(memberName, StringComparer.Ordinal));
}