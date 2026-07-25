using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.TestData
{
    internal static class TestIds
    {
        public static readonly WorkspaceId WorkspaceId = WorkspaceId.Create(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        public static readonly WorkspaceId AnotherWorkspaceId = WorkspaceId.Create(Guid.Parse("22222222-2222-2222-2222-222222222222"));
    }
}