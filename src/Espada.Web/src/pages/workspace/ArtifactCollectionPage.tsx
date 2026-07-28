import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import type {
  ConsoleCreateArtifactRequest,
  ConsoleReviseArtifactRequest,
  InstructionRuleInput,
  PolicyRuleInput,
} from 'shared/api/generated';
import { useArtifact, useArtifactRevisions, useArtifacts, type ArtifactArea } from 'entities/workspace';
import { useCreateArtifact, useReviseArtifact } from 'features/workspace';
import { MutationFeedback, PageState, ResourceQueryState } from 'shared/ui';

const ruleKeySchema = z
  .string()
  .trim()
  .max(100)
  .regex(/^[A-Za-z0-9._-]*$/, 'Use letters, numbers, dots, underscores, or hyphens.');

const artifactFormSchema = z.object({
  title: z.string().trim().min(1, 'Title is required.').max(200),
  content: z.string().trim().min(1, 'Content is required.'),
  ruleKey: ruleKeySchema,
  priority: z.number().int().min(-100).max(100),
  enforcement: z.enum(['hard', 'soft']),
});

const revisionFormSchema = artifactFormSchema.omit({ title: true });

type ArtifactFormValues = z.infer<typeof artifactFormSchema>;
type RevisionFormValues = z.infer<typeof revisionFormSchema>;

const areaConfiguration = {
  artifacts: {
    kindTypeId: 1,
    noun: 'document',
    createLabel: 'Create document',
  },
  instructions: {
    kindTypeId: 2,
    noun: 'instruction',
    createLabel: 'Create instruction',
  },
  policies: {
    kindTypeId: 3,
    noun: 'policy',
    createLabel: 'Create policy',
  },
} as const;

const buildRules = (
  area: ArtifactArea,
  values: Pick<ArtifactFormValues, 'content' | 'ruleKey' | 'priority' | 'enforcement'>,
): {
  instructionRules: null | InstructionRuleInput[];
  policyRules: null | PolicyRuleInput[];
} => {
  if (area === 'instructions') {
    return {
      instructionRules: [
        {
          ruleKey: values.ruleKey,
          text: values.content,
          priority: values.priority,
        },
      ],
      policyRules: null,
    };
  }

  if (area === 'policies') {
    return {
      instructionRules: null,
      policyRules: [
        {
          ruleKey: values.ruleKey,
          text: values.content,
          priority: values.priority,
          enforcementTypeId: values.enforcement === 'hard' ? 1 : 2,
        },
      ],
    };
  }

  return { instructionRules: null, policyRules: null };
};

interface ArtifactCollectionPageProps {
  workspaceId: string;
  area: ArtifactArea;
  readOnly: boolean;
}

export const ArtifactCollectionPage = ({ workspaceId, area, readOnly }: ArtifactCollectionPageProps) => {
  const configuration = areaConfiguration[area];
  const artifacts = useArtifacts(workspaceId, area);
  const [selectedArtifactId, setSelectedArtifactId] = useState('');
  const artifact = useArtifact(workspaceId, selectedArtifactId);
  const revisions = useArtifactRevisions(workspaceId, selectedArtifactId);
  const createArtifact = useCreateArtifact(workspaceId, area);
  const reviseArtifact = useReviseArtifact(workspaceId, area);
  const createForm = useForm<ArtifactFormValues>({
    resolver: zodResolver(artifactFormSchema),
    defaultValues: {
      title: '',
      content: '',
      ruleKey: '',
      priority: 0,
      enforcement: 'soft',
    },
  });
  const revisionForm = useForm<RevisionFormValues>({
    resolver: zodResolver(revisionFormSchema),
    defaultValues: {
      content: '',
      ruleKey: '',
      priority: 0,
      enforcement: 'soft',
    },
  });

  const validateRuleKey = (
    values: Pick<ArtifactFormValues, 'ruleKey'>,
    setError: typeof createForm.setError | typeof revisionForm.setError,
  ): boolean => {
    if (area !== 'artifacts' && values.ruleKey.length === 0) {
      setError('ruleKey', { message: 'A stable rule key is required.' });
      return false;
    }

    return true;
  };

  const submitCreate = createForm.handleSubmit(async (values) => {
    if (!validateRuleKey(values, createForm.setError)) return;

    const rules = buildRules(area, values);
    const request: ConsoleCreateArtifactRequest = {
      title: values.title,
      typeId: 2,
      content: values.content,
      kindTypeId: configuration.kindTypeId,
      ...rules,
    };

    try {
      await createArtifact.mutateAsync(request);
      createForm.reset();
    } catch {
      // MutationFeedback presents the transport or domain error.
    }
  });

  const submitRevision = revisionForm.handleSubmit(async (values) => {
    if (!selectedArtifactId || !validateRuleKey(values, revisionForm.setError)) return;

    const request: ConsoleReviseArtifactRequest = {
      content: values.content,
      ...buildRules(area, values),
    };

    try {
      await reviseArtifact.mutateAsync({ artifactId: selectedArtifactId, request });
      revisionForm.reset();
    } catch {
      // MutationFeedback presents the transport or domain error.
    }
  });

  const selectArtifact = (artifactId: string) => {
    setSelectedArtifactId(artifactId);
    const selected = artifacts.data?.items.find((item) => item.id === artifactId);
    revisionForm.reset({
      content: '',
      ruleKey: '',
      priority: 0,
      enforcement: 'soft',
    });
    if (selected?.kindTypeName.toLowerCase() !== configuration.noun) {
      reviseArtifact.reset();
    }
  };

  if (artifacts.isPending || artifacts.error) {
    return (
      <ResourceQueryState
        isPending={artifacts.isPending}
        error={artifacts.error}
        resourceName={area}
        onRetry={() => void artifacts.refetch()}
      />
    );
  }

  const selectedKind = artifact.data?.kindTypeName.toLowerCase();
  const canRevise = selectedKind === configuration.noun;

  return (
    <div className="artifact-workspace">
      <div className="resource-layout">
        <section className="resource-panel" aria-labelledby={`${area}-list-title`}>
          <div className="resource-panel-heading">
            <div>
              <p className="panel-label">CANONICAL</p>
              <h2 id={`${area}-list-title`}>{area[0].toUpperCase() + area.slice(1)}</h2>
            </div>
            <span>{artifacts.data.items.length}</span>
          </div>
          {artifacts.data.items.length === 0 ? (
            <PageState
              compact
              kind="empty"
              title={`No ${area} yet`}
              description={`Create the first ${configuration.noun} for this workspace.`}
            />
          ) : (
            <div className="resource-list">
              {artifacts.data.items.map((item) => (
                <article
                  className={`resource-card selectable-card${selectedArtifactId === item.id ? ' selected' : ''}`}
                  key={item.id}
                >
                  <button type="button" onClick={() => selectArtifact(item.id)}>
                    <span>
                      <strong>{item.title}</strong>
                      <small>{item.kindTypeName}</small>
                    </span>
                    <span>
                      r{item.currentRevisionNumber ?? '—'} · {item.revisionCount} total
                    </span>
                  </button>
                  <p className="resource-mono">{item.id}</p>
                </article>
              ))}
            </div>
          )}
        </section>

        <aside className="form-panel" aria-labelledby={`create-${area}-title`}>
          <p className="panel-label">NEW {configuration.noun.toUpperCase()}</p>
          <h2 id={`create-${area}-title`}>{configuration.createLabel}</h2>
          <form onSubmit={submitCreate}>
            <label>
              Title
              <input
                {...createForm.register('title')}
                aria-invalid={Boolean(createForm.formState.errors.title)}
                disabled={readOnly}
              />
              {createForm.formState.errors.title && (
                <span className="field-error">{createForm.formState.errors.title.message}</span>
              )}
            </label>
            {area !== 'artifacts' && (
              <label>
                Stable rule key
                <input
                  {...createForm.register('ruleKey')}
                  aria-invalid={Boolean(createForm.formState.errors.ruleKey)}
                  disabled={readOnly}
                  placeholder="security.secrets"
                />
                {createForm.formState.errors.ruleKey && (
                  <span className="field-error">{createForm.formState.errors.ruleKey.message}</span>
                )}
              </label>
            )}
            <label>
              Content
              <textarea
                {...createForm.register('content')}
                aria-invalid={Boolean(createForm.formState.errors.content)}
                disabled={readOnly}
                rows={8}
              />
              {createForm.formState.errors.content && (
                <span className="field-error">{createForm.formState.errors.content.message}</span>
              )}
            </label>
            {area !== 'artifacts' && (
              <div className="form-row">
                <label>
                  Priority
                  <input
                    {...createForm.register('priority', { valueAsNumber: true })}
                    disabled={readOnly}
                    max={100}
                    min={-100}
                    type="number"
                  />
                </label>
                {area === 'policies' && (
                  <label>
                    Enforcement
                    <select {...createForm.register('enforcement')} disabled={readOnly}>
                      <option value="soft">Soft</option>
                      <option value="hard">Hard</option>
                    </select>
                  </label>
                )}
              </div>
            )}
            <button className="button button-primary" disabled={readOnly || createArtifact.isPending} type="submit">
              {createArtifact.isPending ? 'Creating…' : configuration.createLabel}
            </button>
            <MutationFeedback
              error={createArtifact.error}
              isSuccess={createArtifact.isSuccess}
              successMessage={`${configuration.noun[0].toUpperCase() + configuration.noun.slice(1)} created.`}
            />
          </form>
          {area === 'policies' && (
            <p className="form-note">
              Policy mutations are owner-only. Hard policies cannot be weakened by narrower context.
            </p>
          )}
        </aside>
      </div>

      {selectedArtifactId && (
        <section className="artifact-inspector" aria-labelledby="artifact-inspector-title">
          <div className="resource-panel-heading">
            <div>
              <p className="panel-label">IMMUTABLE HISTORY</p>
              <h2 id="artifact-inspector-title">{artifact.data?.title ?? 'Artifact details'}</h2>
            </div>
            <button className="button button-quiet" onClick={() => setSelectedArtifactId('')} type="button">
              Close
            </button>
          </div>
          {artifact.isPending || artifact.error || revisions.isPending || revisions.error ? (
            <ResourceQueryState
              isPending={artifact.isPending || revisions.isPending}
              error={artifact.error ?? revisions.error}
              resourceName="artifact revisions"
              onRetry={() => {
                void artifact.refetch();
                void revisions.refetch();
              }}
            />
          ) : (
            <div className="artifact-inspector-grid">
              <div>
                <h3>Current revision</h3>
                <pre className="context-output">
                  <code>{artifact.data.currentRevision?.content ?? 'No revision content.'}</code>
                </pre>
                <ol className="revision-list">
                  {revisions.data.items.map((revision) => (
                    <li key={revision.id}>
                      <span>Revision {revision.number}</span>
                      <code>{revision.contentHash}</code>
                      <small>{revision.sizeInBytes} bytes</small>
                    </li>
                  ))}
                </ol>
              </div>
              {canRevise ? (
                <form className="inline-form" onSubmit={submitRevision}>
                  <h3>Create next revision</h3>
                  {area !== 'artifacts' && (
                    <label>
                      Stable rule key
                      <input {...revisionForm.register('ruleKey')} disabled={readOnly} />
                      {revisionForm.formState.errors.ruleKey && (
                        <span className="field-error">{revisionForm.formState.errors.ruleKey.message}</span>
                      )}
                    </label>
                  )}
                  <label>
                    Content
                    <textarea {...revisionForm.register('content')} disabled={readOnly} rows={8} />
                    {revisionForm.formState.errors.content && (
                      <span className="field-error">{revisionForm.formState.errors.content.message}</span>
                    )}
                  </label>
                  {area !== 'artifacts' && (
                    <div className="form-row">
                      <label>
                        Priority
                        <input
                          {...revisionForm.register('priority', { valueAsNumber: true })}
                          disabled={readOnly}
                          max={100}
                          min={-100}
                          type="number"
                        />
                      </label>
                      {area === 'policies' && (
                        <label>
                          Enforcement
                          <select {...revisionForm.register('enforcement')} disabled={readOnly}>
                            <option value="soft">Soft</option>
                            <option value="hard">Hard</option>
                          </select>
                        </label>
                      )}
                    </div>
                  )}
                  <button
                    className="button button-primary"
                    disabled={readOnly || reviseArtifact.isPending}
                    type="submit"
                  >
                    {reviseArtifact.isPending ? 'Saving…' : 'Create revision'}
                  </button>
                  <MutationFeedback
                    error={reviseArtifact.error}
                    isSuccess={reviseArtifact.isSuccess}
                    successMessage="Revision created."
                  />
                </form>
              ) : (
                <PageState
                  compact
                  kind="read-only"
                  title="Use the typed editor"
                  description={`This ${selectedKind} is visible here, but revisions belong in its dedicated workspace view.`}
                />
              )}
            </div>
          )}
        </section>
      )}
    </div>
  );
};
