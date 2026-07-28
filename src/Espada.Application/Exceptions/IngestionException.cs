using Espada.Application.Enums;

namespace Espada.Application.Exceptions
{
    public sealed class IngestionException(
        JobFailureCategoryType category,
        string code,
        string message,
        Exception? innerException = null) : Exception(message, innerException)
    {
        public JobFailureCategoryType Category { get; } = category;

        public string Code { get; } = code;
    }
}