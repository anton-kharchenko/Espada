using Espada.Domain.Errors;
using Espada.Domain.Rules;

namespace Espada.Domain.ValueObjects
{
    public sealed record WorkspaceName
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
                return (DomainResult<WorkspaceName>)DomainResult.Failure(WorkspaceErrors.NameEmpty);
            }

            string normalized = value.Trim();

            if (normalized.Length > MaxLength)
            {
                return (DomainResult<WorkspaceName>)DomainResult.Failure(WorkspaceErrors.NameTooLong);
            }

            return DomainResult.Success(new WorkspaceName(normalized));
        }

        public override string ToString()
        {
            return Value;
        }
    }
}