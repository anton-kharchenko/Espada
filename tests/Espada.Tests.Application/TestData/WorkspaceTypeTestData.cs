using Espada.Domain.Enums;
using System.Reflection;

namespace Espada.Tests.Application.TestData
{
    internal static class WorkspaceTypeTestData
    {
        public static WorkspaceType Any { get; } = ResolveAnyWorkspaceType();

        private static WorkspaceType ResolveAnyWorkspaceType()
        {
            WorkspaceType? workspaceType = typeof(WorkspaceType)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(WorkspaceType))
                .Select(field => field.GetValue(null))
                .OfType<WorkspaceType>()
                .FirstOrDefault();

            return workspaceType ??
                   throw new InvalidOperationException("WorkspaceType must declare at least one public static value.");
        }
    }
}