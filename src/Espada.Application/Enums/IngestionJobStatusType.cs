namespace Espada.Application.Enums
{
    public enum IngestionJobStatusType
    {
        Pending = 1,
        Running = 2,
        Succeeded = 3,
        Failed = 4,
        Cancelled = 5,
        Poisoned = 6
    }
}