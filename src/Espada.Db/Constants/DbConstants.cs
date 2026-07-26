namespace Espada.Db.Constants;

public static class DbConstants
{
    public const string SchemaName = "Espada";

    public static class Tables
    {
        public const string Workspaces = "Workspaces";
        public const string WorkspaceTypes = "WorkspaceTypes";
        public const string WorkspaceStatusTypes = "WorkspaceStatusTypes";
        public const string SourceTypes = "SourceTypes";
        public const string SourceStatusTypes = "SourceStatusTypes";
        public const string ImportStatusTypes = "ImportStatusTypes";
        public const string ArtifactTypes = "ArtifactTypes";
        public const string ArtifactStatusTypes = "ArtifactStatusTypes";
        public const string ChunkingStrategyTypes = "ChunkingStrategyTypes";
        public const string ChunkBatchStatusTypes = "ChunkBatchStatusTypes";
        public const string Sources = "Sources";
        public const string ImportJobs = "ImportJobs";
        public const string Artifacts = "Artifacts";
        public const string ArtifactRevisions = "ArtifactRevisions";
        public const string ChunkBatches = "ChunkBatches";
        public const string Chunks = "Chunks";
        public const string ChunkEmbeddings = "ChunkEmbeddings";
        public const string ChunkEmbeddingVectors = "ChunkEmbeddingVectors";
    }

    public static class Properties
    {
        public const string ChunkEmbeddingModelIdentifier = "_modelIdentifier";
        public const string ChunkEmbeddingModelVersion = "_modelVersion";
    }

    public static class Indexes
    {
        public const string ArtifactRevisionArtifactNumber = "UX_ArtifactRevisions_ArtifactId_RevisionNumber";
        public const string SourceWorkspaceLocator = "UX_Sources_WorkspaceId_Locator";
        public const string ChunkBatchNumber = "UX_Chunks_ChunkBatchId_ChunkNumber";
        public const string ChunkEmbeddingChunkModel = "UX_ChunkEmbeddings_ChunkId_ModelIdentifier_ModelVersion";
        public const string WorkspaceTypeName = "UX_WorkspaceTypes_Name";
        public const string WorkspaceStatusTypeName = "UX_WorkspaceStatusTypes_Name";
        public const string SourceTypeName = "UX_SourceTypes_Name";
        public const string SourceStatusTypeName = "UX_SourceStatusTypes_Name";
        public const string ImportStatusTypeName = "UX_ImportStatusTypes_Name";
        public const string ArtifactTypeName = "UX_ArtifactTypes_Name";
        public const string ArtifactStatusTypeName = "UX_ArtifactStatusTypes_Name";
        public const string ChunkingStrategyTypeName = "UX_ChunkingStrategyTypes_Name";
        public const string ChunkBatchStatusTypeName = "UX_ChunkBatchStatusTypes_Name";
    }

    public static class Validations
    {
        public static class MaxLengths
        {
            public const int L32 = 32;
            public const int L50 = 50;
            public const int L64 = 64;
            public const int L100 = 100;
            public const int L200 = 200;
            public const int L255 = 255;
            public const int L500 = 500;
            public const int L2000 = 2000;
            public const int L2048 = 2048;
            public const int L4000 = 4000;
        }
    }

    public static class ColumnTypes
    {
        public static class Text
        {
            public const string Varchar32 = "varchar(32)";
            public const string Varchar50 = "varchar(50)";
            public const string Varchar64 = "varchar(64)";
            public const string Varchar100 = "varchar(100)";
            public const string Varchar200 = "varchar(200)";
            public const string Varchar255 = "varchar(255)";
            public const string Varchar500 = "varchar(500)";
            public const string Varchar2000 = "varchar(2000)";
            public const string Varchar2048 = "varchar(2048)";
            public const string Varchar4000 = "varchar(4000)";
            public const string TextType = "text";
        }

        public static class Numeric
        {
            public const string Integer = "integer";
            public const string BigInt = "bigint";
            public const string RealArray = "real[]";
        }

        public static class DateTime
        {
            public const string TimestampTz = "timestamptz";
        }

        public static class Boolean
        {
            public const string BooleanType = "boolean";
        }

        public static class Json
        {
            public const string Jsonb = "jsonb";
        }

        public static class Identifier
        {
            public const string Uuid = "uuid";
        }

        public static class DefaultValueSql
        {
            public const string Now = "NOW()";
        }
    }
    
    public const string ConnectionString = "Espada";
    
    public const string ConnectionStringEnvironmentVariable = "ESPADA_CONNECTION_STRING";
}