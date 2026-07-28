using AutoMapper;
using Espada.Application.UseCases.Artifacts.Commands.AddArtifactRevision;
using Espada.Application.UseCases.Artifacts.Commands.CreateArtifact;
using Espada.Application.UseCases.Artifacts.Queries.GetArtifactById;
using Espada.Application.UseCases.Artifacts.Queries.ListArtifacts;
using Espada.Application.UseCases.Bindings.Commands.RemoveBinding;
using Espada.Application.UseCases.Bindings.Commands.SetBinding;
using Espada.Application.UseCases.Context.Queries.BuildContext;
using Espada.Application.UseCases.Imports.Commands.RequestImport;
using Espada.Application.UseCases.Memories.Commands.RememberMemory;
using Espada.Application.UseCases.Memories.Queries.SearchMemory;
using Espada.Application.UseCases.Sources.Commands.RegisterSource;
using Espada.Application.UseCases.Workspaces.Commands.CreateWorkspace;
using Espada.Application.UseCases.Workspaces.Queries.GetWorkspaceById;
using Espada.Domain.Constants;
using Espada.Domain.Enums;
using Espada.Domain.SeedWork;
using Espada.Domain.ValueObjects.SourceDefinitions;
using Espada.Protocol.Mcp.Contracts.Requests;
using System.Text.Json;

namespace Espada.Protocol.Mcp.Mappings
{
    public sealed class McpMappingProfile : Profile
    {
        private static readonly JsonSerializerOptions SourceJsonOptions = new(
            JsonSerializerDefaults.Web);

        public McpMappingProfile()
        {
            CreateMap<WorkspaceCreateMappingSource, CreateWorkspaceCommand>()
                .ConvertUsing(source => MapWorkspaceCreateCommand(source));
            CreateMap<WorkspaceGetRequest, GetWorkspaceByIdQuery>();
            CreateMap<MemoryRememberMappingSource, RememberMemoryCommand>()
                .ConvertUsing(source => MapRememberMemoryCommand(source));
            CreateMap<MemorySearchRequest, SearchMemoryQuery>();
            CreateMap<SourceRegisterRequest, RegisterSourceCommand>()
                .ConvertUsing(source => MapRegisterSourceCommand(source));
            CreateMap<ImportOptionsRequest, ImportOptions>();
            CreateMap<SourceImportRequest, RequestImportCommand>()
                .ForCtorParam(
                    nameof(RequestImportCommand.Options),
                    options => options.MapFrom(source => source.Options ?? new ImportOptionsRequest()));
            CreateMap<ArtifactCreateRequest, CreateArtifactCommand>()
                .ForCtorParam(
                    nameof(CreateArtifactCommand.AllowPolicyMutation),
                    options => options.MapFrom(_ => false));
            CreateMap<ArtifactReviseRequest, AddArtifactRevisionCommand>()
                .ForCtorParam(
                    nameof(AddArtifactRevisionCommand.AllowPolicyMutation),
                    options => options.MapFrom(_ => false))
                .ForCtorParam(
                    nameof(AddArtifactRevisionCommand.RequiredKindTypeId),
                    options => options.MapFrom(_ => (int?)null));
            CreateMap<ArtifactGetRequest, GetArtifactByIdQuery>();
            CreateMap<ArtifactListRequest, ListArtifactsQuery>();
            CreateMap<BindingSetRequest, SetBindingCommand>();
            CreateMap<BindingRemoveRequest, RemoveBindingCommand>();
            CreateMap<ContextBuildRequest, BuildContextQuery>();
        }

        private static CreateWorkspaceCommand MapWorkspaceCreateCommand(
            WorkspaceCreateMappingSource source)
        {
            WorkspaceType? workspaceType = Enumeration
                .GetAll<WorkspaceType>()
                .SingleOrDefault(type => type.Id == source.Request.TypeId);
            if (workspaceType is null)
            {
                throw new ArgumentException(
                    $"Workspace type ID '{source.Request.TypeId}' is not supported.",
                    nameof(source));
            }

            return new CreateWorkspaceCommand(
                source.Request.Name,
                workspaceType,
                source.Request.OrganizationId,
                source.Principal.IdentityIssuer,
                source.Principal.IdentitySubject);
        }

        private static RememberMemoryCommand MapRememberMemoryCommand(
            MemoryRememberMappingSource source)
        {
            return new RememberMemoryCommand(
                source.Request.WorkspaceId,
                source.Request.Title,
                source.Request.Content,
                source.Request.CategoryTypeId,
                source.Request.Confidence,
                source.Principal.ClientId,
                source.Request.SessionIdentity,
                source.Request.SupersededMemoryId);
        }

        private static RegisterSourceCommand MapRegisterSourceCommand(
            SourceRegisterRequest source)
        {
            return new RegisterSourceCommand(
                source.WorkspaceId,
                source.Name,
                DeserializeSourceDefinition(source.Definition));
        }

        private static SourceDefinition DeserializeSourceDefinition(
            JsonElement definition)
        {
            if (!definition.TryGetProperty(
                    SourceDefinitionDiscriminatorConstants.Property,
                    out JsonElement typeElement))
            {
                throw new JsonException("Source definition type is required.");
            }

            string json = definition.GetRawText();
            return typeElement.GetString() switch
            {
                SourceDefinitionDiscriminatorConstants.File =>
                    Deserialize<FileSourceDefinition>(json),
                SourceDefinitionDiscriminatorConstants.WebPage =>
                    Deserialize<WebPageSourceDefinition>(json),
                SourceDefinitionDiscriminatorConstants.PlainText =>
                    Deserialize<PlainTextSourceDefinition>(json),
                SourceDefinitionDiscriminatorConstants.Conversation =>
                    Deserialize<ConversationSourceDefinition>(json),
                SourceDefinitionDiscriminatorConstants.Connector =>
                    Deserialize<ConnectorSourceDefinition>(json),
                _ => throw new JsonException(
                    "Source definition type is not supported.")
            };
        }

        private static TDefinition Deserialize<TDefinition>(string json)
            where TDefinition : SourceDefinition
        {
            return JsonSerializer.Deserialize<TDefinition>(json, SourceJsonOptions)
                   ?? throw new JsonException("Source definition payload was empty.");
        }
    }
}
