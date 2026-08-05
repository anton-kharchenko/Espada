namespace Espada.Db.Constants
{
    public static class DbIndexConstants
    {
        public const string ArtifactRevisionArtifactNumber = "UX_ArtifactRevisions_ArtifactId_RevisionNumber";
        public const string BindingTaskRequiresProject = "CK_Bindings_TaskRequiresProject";
        public const string InstructionRuleKind = "CK_InstructionRules_Kind";
        public const string PolicyRuleKind = "CK_PolicyRules_Kind";
        public const string MemoryMetadataKind = "CK_MemoryMetadata_Kind";
        public const string MemoryMetadataSupersededMemory = "UX_MemoryMetadata_SupersededMemoryId";
        public const string OrganizationMembershipIdentity = "UX_OrganizationMemberships_OrganizationId_Issuer_Subject";
        public const string OneTimeBootstrapCodeHash = "UX_OneTimeBootstrapCodes_CodeHash";
        public const string ProjectWorkspaceRemote = "UX_Projects_WorkspaceId_CanonicalRemoteUri";
        public const string InstructionRuleRevisionKey = "PK_InstructionRules_ArtifactRevisionId_RuleKey";
        public const string PolicyRuleRevisionKey = "PK_PolicyRules_ArtifactRevisionId_RuleKey";
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
        public const string AgentVendorTypeName = "UX_AgentVendorTypes_Name";
        public const string AgentSessionStatusTypeName = "UX_AgentSessionStatusTypes_Name";
        public const string AgentSessionEventTypeName = "UX_AgentSessionEventTypes_Name";
        public const string AgentApprovalStatusTypeName = "UX_AgentApprovalStatusTypes_Name";
        public const string SyncConflictStatusTypeName = "UX_SyncConflictStatusTypes_Name";
        public const string AgentProfileWorkspaceVendorName = "UX_AgentProfiles_WorkspaceId_Vendor_Name";
        public const string AgentInstallationDeviceVendorPath = "UX_AgentInstallations_DeviceId_Vendor_ExecutablePath";
        public const string AgentSessionEventSequence = "UX_AgentSessionEvents_AgentSessionId_Sequence";
        public const string AgentApprovalRequestEvent = "UX_AgentApprovals_RequestEventId";
        public const string SyncEventDeviceSequence = "UX_SyncEvents_DeviceId_Sequence";
        public const string SyncEventServerSequence = "UX_SyncEvents_ServerSequence";
        public const string SyncCursorDeviceWorkspace = "UX_SyncCursors_DeviceId_WorkspaceId";
        public const string SyncConflictEvents = "UX_SyncConflicts_LocalEventId_RemoteEventId";
    }
}