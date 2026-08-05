import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { useImports, useSources } from 'entities/workspace';
import { useCancelImport, useRequestImport } from 'features/workspace';
import { MutationFeedback, PageState, ResourceQueryState } from 'shared/ui';
import { importFormSchema, type ImportFormValues } from './importsValidation';

interface ImportsPageProps {
  workspaceId: string;
  readOnly: boolean;
}

export const ImportsPage = ({ workspaceId, readOnly }: ImportsPageProps) => {
  const imports = useImports(workspaceId);
  const sources = useSources(workspaceId);
  const requestImport = useRequestImport(workspaceId);
  const cancelImport = useCancelImport(workspaceId);
  const form = useForm<ImportFormValues>({
    resolver: zodResolver(importFormSchema),
    defaultValues: {
      sourceId: '',
      embeddingModel: '',
      chunkingStrategy: 'Recursive',
      maxCharacters: 2_000,
      overlapCharacters: 200,
      semanticThreshold: 0.75,
    },
  });

  const submit = form.handleSubmit(async (values) => {
    try {
      await requestImport.mutateAsync({
        sourceId: values.sourceId,
        options: {
          embeddingModel: values.embeddingModel || undefined,
          chunkingStrategy: values.chunkingStrategy,
          maxCharacters: values.maxCharacters,
          overlapCharacters: values.overlapCharacters,
          semanticThreshold: values.semanticThreshold,
        },
      });
    } catch {
      // MutationFeedback presents the transport or domain error.
    }
  });

  if (imports.isPending || imports.error) {
    return (
      <ResourceQueryState
        isPending={imports.isPending}
        error={imports.error}
        resourceName="imports"
        onRetry={() => void imports.refetch()}
      />
    );
  }

  return (
    <div className="resource-layout">
      <section className="resource-panel" aria-labelledby="import-list-title">
        <div className="resource-panel-heading">
          <div>
            <p className="panel-label">DURABLE PIPELINE</p>
            <h2 id="import-list-title">Imports</h2>
          </div>
          <span>{imports.data.items.length}</span>
        </div>
        {imports.data.items.length === 0 ? (
          <PageState
            compact
            kind="empty"
            title="No imports requested"
            description="Import a registered source through the chunk and search pipeline."
          />
        ) : (
          <div className="resource-list">
            {imports.data.items.map((item) => {
              const status = item.statusName.toLowerCase();
              const canCancel = !['completed', 'failed', 'cancelled', 'canceled'].includes(status);
              const source = sources.data?.items.find((candidate) => candidate.id === item.sourceId);

              return (
                <article className="resource-card" key={item.id}>
                  <div className="resource-card-title">
                    <div>
                      <h3>{source?.name ?? item.sourceId}</h3>
                      <p>{item.stage}</p>
                    </div>
                    <span className={`status-pill status-${status}`}>{item.statusName}</span>
                  </div>
                  <dl className="resource-meta">
                    <div>
                      <dt>Import ID</dt>
                      <dd>{item.id}</dd>
                    </div>
                    <div>
                      <dt>Artifact</dt>
                      <dd>{item.artifactId ?? 'Pending'}</dd>
                    </div>
                  </dl>
                  {item.failureReason && <p className="form-feedback form-feedback-error">{item.failureReason}</p>}
                  {canCancel && (
                    <div className="card-actions">
                      <button
                        className="button button-quiet"
                        disabled={readOnly || cancelImport.isPending}
                        onClick={() => cancelImport.mutate(item.id)}
                        type="button"
                      >
                        Cancel
                      </button>
                    </div>
                  )}
                </article>
              );
            })}
          </div>
        )}
        <MutationFeedback error={cancelImport.error} />
      </section>

      <aside className="form-panel" aria-labelledby="request-import-title">
        <p className="panel-label">NEW IMPORT</p>
        <h2 id="request-import-title">Import source</h2>
        {sources.isPending || sources.error ? (
          <ResourceQueryState
            isPending={sources.isPending}
            error={sources.error}
            resourceName="sources"
            onRetry={() => void sources.refetch()}
          />
        ) : sources.data.items.length === 0 ? (
          <PageState
            compact
            kind="empty"
            title="A source is required"
            description="Register a source before requesting an import."
          />
        ) : (
          <form onSubmit={submit}>
            <label>
              Source
              <select
                {...form.register('sourceId')}
                aria-invalid={Boolean(form.formState.errors.sourceId)}
                disabled={readOnly}
              >
                <option value="">Select source</option>
                {sources.data.items.map((source) => (
                  <option key={source.id} value={source.id}>
                    {source.name} · {source.typeName}
                  </option>
                ))}
              </select>
              {form.formState.errors.sourceId && (
                <span className="field-error">{form.formState.errors.sourceId.message}</span>
              )}
            </label>
            <label>
              Embedding model
              <input
                {...form.register('embeddingModel')}
                aria-describedby="embedding-model-hint"
                aria-invalid={Boolean(form.formState.errors.embeddingModel)}
                disabled={readOnly}
                placeholder="identifier@version"
              />
              <span className="field-hint" id="embedding-model-hint">
                Leave blank to use the deployment default.
              </span>
              {form.formState.errors.embeddingModel && (
                <span className="field-error">{form.formState.errors.embeddingModel.message}</span>
              )}
            </label>
            <label>
              Chunking strategy
              <select {...form.register('chunkingStrategy')} disabled={readOnly}>
                <option value="Recursive">Recursive</option>
                <option value="Markdown">Markdown</option>
                <option value="Code">Code</option>
                <option value="Semantic">Semantic</option>
                <option value="FixedSize">Fixed size</option>
                <option value="Custom">Custom</option>
              </select>
            </label>
            <div className="form-row">
              <label>
                Max characters
                <input
                  {...form.register('maxCharacters', { valueAsNumber: true })}
                  disabled={readOnly}
                  min={1}
                  type="number"
                />
              </label>
              <label>
                Overlap
                <input
                  {...form.register('overlapCharacters', { valueAsNumber: true })}
                  disabled={readOnly}
                  min={0}
                  type="number"
                />
                {form.formState.errors.overlapCharacters && (
                  <span className="field-error">{form.formState.errors.overlapCharacters.message}</span>
                )}
              </label>
            </div>
            <label>
              Semantic threshold
              <input
                {...form.register('semanticThreshold', { valueAsNumber: true })}
                disabled={readOnly}
                max={1}
                min={0}
                step={0.05}
                type="number"
              />
            </label>
            <button className="button button-primary" disabled={readOnly || requestImport.isPending} type="submit">
              {requestImport.isPending ? 'Requesting…' : 'Request import'}
            </button>
            <MutationFeedback
              error={requestImport.error}
              isSuccess={requestImport.isSuccess}
              successMessage="Import accepted."
            />
          </form>
        )}
      </aside>
    </div>
  );
};
