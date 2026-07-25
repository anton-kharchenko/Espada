using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.TestData
{
    internal static class TestIds
    {
        public static readonly WorkspaceId WorkspaceId = WorkspaceId.Create(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        public static readonly WorkspaceId AnotherWorkspaceId = WorkspaceId.Create(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        
        public static readonly SourceId SourceId = SourceId.Create(Guid.Parse("77777777-7777-7777-7777-777777777777"));

        public static readonly SourceId AnotherSourceId = SourceId.Create(Guid.Parse("78787878-7878-7878-7878-787878787878"));
        
        public static readonly ImportJobId DefaultImportJobId = ImportJobId.Create(Guid.Parse("88888888-8888-8888-8888-888888888888"));

        public static readonly ImportJobId AnotherImportJobId = ImportJobId.Create(Guid.Parse("89898989-8989-8989-8989-898989898989"));
    }
}