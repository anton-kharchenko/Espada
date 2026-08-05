using Espada.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Espada.Api.Contracts.Requests.Common
{
    public sealed class SetContextPriorityRequest
    {
        [Range(ContextPriority.Minimum, ContextPriority.Maximum)]
        public int Priority { get; init; }
    }
}