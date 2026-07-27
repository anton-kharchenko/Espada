namespace Espada.Infrastructure.Repositories;

public sealed class WorkspaceContextSearchOptions
{
    public const string SectionName = "WorkspaceContextSearch";

    public double VectorWeight { get; set; } = 0.60d;

    public double KeywordWeight { get; set; } = 0.25d;

    public double RecencyWeight { get; set; } = 0.10d;

    public double ArtifactPriorityWeight { get; set; } = 0.025d;

    public double SourcePriorityWeight { get; set; } = 0.025d;

    public double RecencyHalfLifeDays { get; set; } = 90d;

    internal bool IsValid()
    {
        double totalWeight = VectorWeight + KeywordWeight + RecencyWeight + ArtifactPriorityWeight + SourcePriorityWeight;

        return VectorWeight >= 0d &&
               KeywordWeight >= 0d &&
               RecencyWeight >= 0d &&
               ArtifactPriorityWeight >= 0d &&
               SourcePriorityWeight >= 0d &&
               RecencyHalfLifeDays > 0d &&
               Math.Abs(totalWeight - 1d) < 0.000001d;
    }
}