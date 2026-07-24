namespace Espada.Tests.Domain.TestData
{
    internal static class TestIds
    {
        public static readonly WorkspaceId DefaultWorkspaceId = WorkspaceId.Create(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        public static readonly WorkspaceId AnotherWorkspaceId = WorkspaceId.Create(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        
        public static readonly ArtifactId DefaultArtifactId = ArtifactId.Create(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        public static readonly ArtifactId AnotherArtifactId = ArtifactId.Create(Guid.Parse("44444444-4444-4444-4444-444444444444"));
    }
}