namespace Espada.Application.Models
{
    public sealed record ContextSpecificity(
        int Agent,
        int Task,
        int Branch,
        int PathSegments,
        int PathBytes,
        int Repository,
        int Project,
        int Organization) : IComparable<ContextSpecificity>
    {
        public int CompareTo(ContextSpecificity? other)
        {
            if (other is null)
            {
                return 1;
            }

            int comparison = Agent.CompareTo(other.Agent);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = Task.CompareTo(other.Task);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = Branch.CompareTo(other.Branch);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = PathSegments.CompareTo(other.PathSegments);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = PathBytes.CompareTo(other.PathBytes);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = Repository.CompareTo(other.Repository);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = Project.CompareTo(other.Project);
            return comparison != 0
                ? comparison
                : Organization.CompareTo(other.Organization);
        }
    }
}