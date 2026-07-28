using Espada.Domain.Rules;

namespace Espada.Application.ApplicationErrors
{
    public static class BindingApplicationErrors
    {
        public static readonly DomainError InvalidId = new(
            "Binding.Id.Invalid",
            "Binding ID cannot be empty.");

        public static DomainError NotFound(Guid bindingId)
        {
            return new DomainError(
                "Binding.NotFound",
                $"Binding with ID '{bindingId:D}' was not found.");
        }

        public static DomainError NotFoundInWorkspace(Guid bindingId, Guid workspaceId)
        {
            return new DomainError(
                "Binding.NotFoundInWorkspace",
                $"Binding with ID '{bindingId:D}' was not found in workspace '{workspaceId:D}'.");
        }
    }
}