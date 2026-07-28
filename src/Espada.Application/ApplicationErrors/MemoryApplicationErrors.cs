using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class MemoryApplicationErrors
    {
        public static readonly DomainError QueryEmpty = new(
            "Memory.Query.Empty",
            "Memory query text cannot be empty.");

        public static readonly DomainError TopKOutOfRange = new(
            "Memory.TopK.OutOfRange",
            "Memory topK must be between 1 and 50.");


        public static readonly DomainError InvalidEmbeddingModel = new(
            "Memory.EmbeddingModel.Invalid",
            "The default embedding model must use 'identifier@version' format.");

        public static DomainError UnsupportedCategoryType(int categoryTypeId)
        {
            return new DomainError(
                "Memory.CategoryType.Unsupported",
                $"Memory category type with ID '{categoryTypeId}' is not supported.");
        }

        public static DomainError NotFound(Guid memoryId)
        {
            return new DomainError(
                "Memory.NotFound",
                $"Memory with ID '{memoryId:D}' was not found.");
        }

        public static DomainError NotFoundInWorkspace(Guid memoryId, Guid workspaceId)
        {
            return new DomainError(
                "Memory.NotFoundInWorkspace",
                $"Memory with ID '{memoryId:D}' was not found in workspace '{workspaceId:D}'.");
        }

        public static DomainError AlreadySuperseded(Guid memoryId)
        {
            return new DomainError(
                "Memory.AlreadySuperseded",
                $"Memory with ID '{memoryId:D}' is already superseded.");
        }
    }
}