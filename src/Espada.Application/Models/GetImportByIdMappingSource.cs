using Espada.Application.Contracts.Jobs;
using Espada.Domain.Aggregates;

namespace Espada.Application.Models;

internal sealed record GetImportByIdMappingSource(ImportJob ImportJob, IngestionJob? LatestJob, bool IsTerminal);