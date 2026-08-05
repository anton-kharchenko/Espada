import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { useMemories } from 'entities/workspace';
import { useRememberMemory } from 'features/workspace';
import { MutationFeedback, PageState, ResourceQueryState } from 'shared/ui';

const memoryFormSchema = z.object({
  title: z.string().trim().min(1, 'Title is required.').max(200),
  content: z.string().trim().min(1, 'Memory content is required.'),
  categoryTypeId: z.number().int().min(1).max(7),
  confidence: z.number().min(0).max(1),
  supersededMemoryId: z.string(),
});

const searchFormSchema = z.object({
  query: z.string().trim().min(1, 'Enter a memory search query.'),
});

type MemoryFormValues = z.infer<typeof memoryFormSchema>;
type SearchFormValues = z.infer<typeof searchFormSchema>;

const categories = [
  [1, 'Fact'],
  [2, 'Decision'],
  [3, 'Preference'],
  [4, 'Episode'],
  [5, 'Summary'],
  [6, 'Observation'],
  [7, 'Warning'],
] as const;

interface MemoriesPageProps {
  workspaceId: string;
  readOnly: boolean;
}

export const MemoriesPage = ({ workspaceId, readOnly }: MemoriesPageProps) => {
  const [query, setQuery] = useState('');
  const memories = useMemories(workspaceId, query);
  const rememberMemory = useRememberMemory(workspaceId);
  const memoryForm = useForm<MemoryFormValues>({
    resolver: zodResolver(memoryFormSchema),
    defaultValues: {
      title: '',
      content: '',
      categoryTypeId: 1,
      confidence: 0.7,
      supersededMemoryId: '',
    },
  });
  const searchForm = useForm<SearchFormValues>({
    resolver: zodResolver(searchFormSchema),
    defaultValues: { query: '' },
  });

  const submitMemory = memoryForm.handleSubmit(async (values) => {
    try {
      await rememberMemory.mutateAsync({
        title: values.title,
        content: values.content,
        categoryTypeId: values.categoryTypeId,
        confidence: values.confidence,
        supersededMemoryId: values.supersededMemoryId.trim() || null,
      });
      memoryForm.reset();
      searchForm.setValue('query', values.title);
      setQuery(values.title);
    } catch {
      // MutationFeedback presents the transport or domain error.
    }
  });

  const submitSearch = searchForm.handleSubmit((values) => setQuery(values.query));

  return (
    <div className="resource-layout">
      <section className="resource-panel" aria-labelledby="memory-search-title">
        <div className="resource-panel-heading">
          <div>
            <p className="panel-label">SHARED CANONICAL MEMORY</p>
            <h2 id="memory-search-title">Search memories</h2>
          </div>
          <span>{memories.data?.items.length ?? 0}</span>
        </div>
        <form className="search-form" onSubmit={submitSearch}>
          <label>
            <span className="sr-only">Search memories</span>
            <input
              {...searchForm.register('query')}
              aria-invalid={Boolean(searchForm.formState.errors.query)}
              placeholder="Search facts, decisions, or preferences"
              type="search"
            />
          </label>
          <button className="button button-secondary" disabled={memories.isFetching} type="submit">
            {memories.isFetching ? 'Searching…' : 'Search'}
          </button>
        </form>
        {searchForm.formState.errors.query && (
          <p className="field-error">{searchForm.formState.errors.query.message}</p>
        )}
        {!query ? (
          <PageState
            compact
            kind="empty"
            title="Search shared memory"
            description="Results include provenance, confidence, confirmation state, and semantic score."
          />
        ) : memories.isPending || memories.error ? (
          <ResourceQueryState
            isPending={memories.isPending}
            error={memories.error}
            resourceName="memories"
            onRetry={() => void memories.refetch()}
          />
        ) : memories.data.items.length === 0 ? (
          <PageState
            compact
            kind="empty"
            title="No matching memories"
            description="Try a different phrase or remember a new item."
          />
        ) : (
          <div className="resource-list">
            {memories.data.items.map((memory) => (
              <article className="resource-card memory-card" key={memory.memoryId}>
                <div className="resource-card-title">
                  <div>
                    <h3>{memory.title}</h3>
                    <p>{memory.categoryTypeName}</p>
                  </div>
                  <span className="status-pill status-unconfirmed">[unconfirmed]</span>
                </div>
                <p className="memory-content">{memory.content}</p>
                <dl className="resource-meta">
                  <div>
                    <dt>Confidence</dt>
                    <dd>{Number(memory.confidence).toFixed(2)}</dd>
                  </div>
                  <div>
                    <dt>Search score</dt>
                    <dd>{Number(memory.score).toFixed(3)}</dd>
                  </div>
                  <div>
                    <dt>Client</dt>
                    <dd>{memory.provenance.clientIdentity}</dd>
                  </div>
                  <div>
                    <dt>Session</dt>
                    <dd>{memory.provenance.sessionIdentity ?? 'Not supplied'}</dd>
                  </div>
                </dl>
              </article>
            ))}
          </div>
        )}
      </section>

      <aside className="form-panel" aria-labelledby="remember-memory-title">
        <p className="panel-label">NEW MEMORY</p>
        <h2 id="remember-memory-title">Remember for every agent</h2>
        <form onSubmit={submitMemory}>
          <label>
            Title
            <input
              {...memoryForm.register('title')}
              aria-invalid={Boolean(memoryForm.formState.errors.title)}
              disabled={readOnly}
            />
            {memoryForm.formState.errors.title && (
              <span className="field-error">{memoryForm.formState.errors.title.message}</span>
            )}
          </label>
          <label>
            Category
            <select {...memoryForm.register('categoryTypeId', { valueAsNumber: true })} disabled={readOnly}>
              {categories.map(([id, name]) => (
                <option key={id} value={id}>
                  {name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Content
            <textarea
              {...memoryForm.register('content')}
              aria-invalid={Boolean(memoryForm.formState.errors.content)}
              disabled={readOnly}
              rows={7}
            />
            {memoryForm.formState.errors.content && (
              <span className="field-error">{memoryForm.formState.errors.content.message}</span>
            )}
          </label>
          <label>
            Confidence
            <input
              {...memoryForm.register('confidence', { valueAsNumber: true })}
              disabled={readOnly}
              max={1}
              min={0}
              step={0.05}
              type="number"
            />
          </label>
          <label>
            Supersedes memory ID
            <input {...memoryForm.register('supersededMemoryId')} disabled={readOnly} placeholder="Optional UUID" />
          </label>
          <p className="form-note">MCP memories are always stored as unconfirmed with client and session provenance.</p>
          <button className="button button-primary" disabled={readOnly || rememberMemory.isPending} type="submit">
            {rememberMemory.isPending ? 'Remembering…' : 'Remember'}
          </button>
          <MutationFeedback
            error={rememberMemory.error}
            isSuccess={rememberMemory.isSuccess}
            successMessage="Memory saved as unconfirmed."
          />
        </form>
      </aside>
    </div>
  );
};
