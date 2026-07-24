namespace Espada.Tests.Domain.TestData
{
    internal static class TestIds
    {
        public static readonly WorkspaceId DefaultWorkspaceId = WorkspaceId.Create(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        public static readonly WorkspaceId AnotherWorkspaceId = WorkspaceId.Create(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        
        public static readonly ArtifactId DefaultArtifactId = ArtifactId.Create(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        public static readonly ArtifactId AnotherArtifactId = ArtifactId.Create(Guid.Parse("44444444-4444-4444-4444-444444444444"));
        
        public static readonly ArtifactRevisionId FirstRevisionId = ArtifactRevisionId.Create(Guid.Parse("55555555-5555-5555-5555-555555555555"));
        public static readonly ArtifactRevisionId SecondRevisionId = ArtifactRevisionId.Create(Guid.Parse("66666666-6666-6666-6666-666666666666"));
        
        public static readonly SourceId DefaultSourceId = SourceId.Create(Guid.Parse("77777777-7777-7777-7777-777777777777"));
        
        public static readonly ImportJobId DefaultImportJobId = ImportJobId.Create(Guid.Parse("88888888-8888-8888-8888-888888888888"));
        
        public static readonly ChunkId DefaultChunkId = ChunkId.Create(Guid.Parse("99999999-9999-9999-9999-999999999999"));
        public static readonly ChunkId SecondChunkId = ChunkId.Create(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        public static readonly ChunkBatchId DefaultChunkBatchId = ChunkBatchId.Create(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
    }
}