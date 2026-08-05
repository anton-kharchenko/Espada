using Espada.Domain.Rules;

namespace Espada.Domain.Errors
{
    public static class DeviceErrors
    {
        public static DomainError NameEmpty { get; } = new("Device.NameEmpty", "Device name cannot be empty.");

        public static DomainError NameTooLong { get; } = new("Device.NameTooLong",
            "Device name cannot exceed 200 characters.");
    }
}