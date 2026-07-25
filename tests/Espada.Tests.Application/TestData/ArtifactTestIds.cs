using Espada.Domain.ValueObjects;

namespace Espada.Tests.Application.TestData
{
    internal static class ArtifactTestIds
    {
        public static readonly ArtifactId DefaultArtifactId =
            ArtifactId.Create(
                Guid.Parse("33333333-3333-3333-3333-333333333333"));

        public static readonly ArtifactId AnotherArtifactId =
            ArtifactId.Create(
                Guid.Parse("44444444-4444-4444-4444-444444444444"));

        public static readonly ArtifactRevisionId FirstRevisionId =
            ArtifactRevisionId.Create(
                Guid.Parse("55555555-5555-5555-5555-555555555555"));

        public static readonly ArtifactRevisionId SecondRevisionId =
            ArtifactRevisionId.Create(
                Guid.Parse("66666666-6666-6666-6666-666666666666"));
    }
}