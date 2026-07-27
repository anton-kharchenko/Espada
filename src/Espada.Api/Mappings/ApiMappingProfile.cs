using AutoMapper;
using Espada.Api.Contracts.Models;
using Espada.Api.Contracts.Requests.Imports;
using Espada.Api.Contracts.Responses.Billing;
using Espada.Api.Extensions;
using Espada.Application.UseCases.Context.Queries.SearchWorkspaceContext;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Application.UseCases.Sources.Commands.RegisterSource;
using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Billing.Enums;
using Espada.Billing.Models;
using Espada.Billing.UseCases.Checkout;
using Espada.Domain.Enums;

namespace Espada.Api.Mappings;

public sealed class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        CreateMap<CreateWorkspaceMappingSource, CreateWorkspaceCommand>()
            .ConvertUsing(source => MapCreateWorkspaceCommand(source));

        CreateMap<CreateCheckoutMappingSource, CreateCheckoutCommand>()
            .ConvertUsing(source => new CreateCheckoutCommand(
                source.WorkspaceId,
                Enum.Parse<CloudBillingPlanType>(
                    source.Request.Plan,
                    ignoreCase: true),
                source.IdempotencyKey));

        CreateMap<BillingStatusSnapshot, BillingStatusResponse>()
            .ForCtorParam(
                nameof(BillingStatusResponse.Plan),
                options => options.MapFrom(source => source.Plan.ToString()))
            .ForCtorParam(
                nameof(BillingStatusResponse.AccessState),
                options => options.MapFrom(source => source.AccessState.ToString()));

        CreateMap<StripeWebhookReceipt, StripeWebhookReceiptResponse>();

        CreateMap<RegisterSourceMappingSource, RegisterSourceCommand>()
            .ConvertUsing(source => new RegisterSourceCommand(
                source.WorkspaceId,
                source.Request.Name,
                source.Request.Definition!));

        CreateMap<RequestImportMappingSource, RequestImportCommand>()
            .ConvertUsing(source => MapRequestImportCommand(source));

        CreateMap<SearchWorkspaceContextMappingSource, SearchWorkspaceContextQuery>()
            .ConvertUsing(source => new SearchWorkspaceContextQuery(
                source.WorkspaceId,
                source.Request.QueryText,
                source.Request.QueryVector,
                source.Request.ModelIdentifier,
                source.Request.ModelVersion,
                source.Request.TopK,
                source.Request.ArtifactIds,
                source.Request.RevisionIds,
                source.Request.SourceIds,
                source.Request.ArtifactTypeIds,
                source.Request.SourceTypeIds,
                source.Request.CreatedAfterUtc,
                source.Request.MinimumSimilarity,
                source.Request.MinimumArtifactPriority,
                source.Request.MinimumSourcePriority));
    }

    private static RequestImportCommand MapRequestImportCommand(
        RequestImportMappingSource source)
    {
        ImportOptionsRequest options = source.Request.Options
            ?? new ImportOptionsRequest();
        return new RequestImportCommand(
            source.WorkspaceId,
            source.Request.SourceId,
            source.IdempotencyKey,
            new ImportOptions(
                options.EmbeddingModel,
                options.ChunkingStrategy,
                options.MaxCharacters,
                options.OverlapCharacters,
                options.SemanticThreshold,
                options.Separators,
                options.CodeLanguage));
    }

    private static CreateWorkspaceCommand MapCreateWorkspaceCommand(
        CreateWorkspaceMappingSource source) =>
        new(
            source.Request.Name,
            source.Request.TypeId.ToEnumeration<WorkspaceType>()
                ?? throw new InvalidOperationException(
                    $"Workspace type ID '{source.Request.TypeId}' passed validation but could not be resolved."),
            source.IdentityIssuer,
            source.IdentitySubject);
}