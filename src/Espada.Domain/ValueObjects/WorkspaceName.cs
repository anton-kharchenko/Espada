using Espada.Domain.Errors;
using Espada.Domain.Rules;
using Espada.Domain.SeedWork;

namespace Espada.Domain.ValueObjects
{
    public sealed class WorkspaceName : ValueObject
    {
        public const int MaxLength = 100;

        private WorkspaceName(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static DomainResult<WorkspaceName> Create(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DomainResult<WorkspaceName>.Failure(WorkspaceErrors.NameEmpty);
            }

            string normalized = value.Trim();

            return normalized.Length > MaxLength
                ? DomainResult<WorkspaceName>.Failure(WorkspaceErrors.NameTooLong)
                : DomainResult<WorkspaceName>.Success(new WorkspaceName(normalized));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}