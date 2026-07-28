import { zodResolver } from '@hookform/resolvers/zod';
import { useForm, useWatch } from 'react-hook-form';
import { z } from 'zod';
import { useProjects, useTasks } from 'entities/workspace';
import { useBuildContext } from 'features/workspace';
import { MutationFeedback, PageState } from 'shared/ui';

const contextFormSchema = z
  .object({
    projectId: z.string(),
    taskId: z.string(),
    repositoryRelativePath: z.string(),
    branch: z.string(),
    agent: z.enum(['codex', 'claude', 'gemini', 'generic']),
    tokenBudget: z.number().int().positive().max(1_048_576),
  })
  .refine(
    (values) =>
      Boolean(values.projectId) || (!values.taskId && !values.repositoryRelativePath.trim() && !values.branch.trim()),
    {
      message: 'Select a project when task, path, or branch is supplied.',
      path: ['projectId'],
    },
  );

type ContextFormValues = z.infer<typeof contextFormSchema>;

interface ContextPageProps {
  workspaceId: string;
  view: 'preview' | 'explain';
}

export const ContextPage = ({ workspaceId, view }: ContextPageProps) => {
  const projects = useProjects(workspaceId);
  const tasks = useTasks(workspaceId);
  const buildContext = useBuildContext(workspaceId);
  const form = useForm<ContextFormValues>({
    resolver: zodResolver(contextFormSchema),
    defaultValues: {
      projectId: '',
      taskId: '',
      repositoryRelativePath: '',
      branch: '',
      agent: 'codex',
      tokenBudget: 32_768,
    },
  });
  const selectedProjectId = useWatch({
    control: form.control,
    name: 'projectId',
  });
  const availableTasks = tasks.data?.items.filter(
    (task) => task.statusTypeName.toLowerCase() === 'active' && task.projectId === selectedProjectId,
  );

  const submit = form.handleSubmit(async (values) => {
    try {
      await buildContext.mutateAsync({
        projectId: values.projectId || null,
        taskId: values.taskId || null,
        repositoryRelativePath: values.repositoryRelativePath.trim() || null,
        branch: values.branch.trim() || null,
        agent: values.agent,
        tokenBudget: values.tokenBudget,
      });
    } catch {
      // MutationFeedback presents the transport or domain error.
    }
  });

  const result = buildContext.data;

  return (
    <div className="context-builder">
      <form className="context-form" onSubmit={submit}>
        <div className="resource-panel-heading">
          <div>
            <p className="panel-label">CANONICAL RESOLVER</p>
            <h2>Build context</h2>
          </div>
          <span>{view === 'preview' ? 'Projection' : 'Explain'}</span>
        </div>
        <div className="context-form-grid">
          <label>
            Project
            <select {...form.register('projectId')} aria-invalid={Boolean(form.formState.errors.projectId)}>
              <option value="">Workspace only</option>
              {projects.data?.items.map((project) => (
                <option key={project.id} value={project.id}>
                  {project.name}
                </option>
              ))}
            </select>
            {form.formState.errors.projectId && (
              <span className="field-error">{form.formState.errors.projectId.message}</span>
            )}
          </label>
          <label>
            Task
            <select {...form.register('taskId')} disabled={!selectedProjectId}>
              <option value="">Any task</option>
              {availableTasks?.map((task) => (
                <option key={task.id} value={task.id}>
                  {task.title}
                </option>
              ))}
            </select>
          </label>
          <label>
            Repository-relative path
            <input {...form.register('repositoryRelativePath')} placeholder="src/Espada.Api" />
          </label>
          <label>
            Branch
            <input {...form.register('branch')} placeholder="feature/stage-11-mcp-runtime" />
          </label>
          <label>
            Agent projection
            <select {...form.register('agent')}>
              <option value="codex">Codex · AGENTS.md style</option>
              <option value="claude">Claude · CLAUDE.md style</option>
              <option value="gemini">Gemini · GEMINI.md style</option>
              <option value="generic">Generic MCP · canonical JSON</option>
            </select>
          </label>
          <label>
            UTF-8 byte budget
            <input {...form.register('tokenBudget', { valueAsNumber: true })} max={1_048_576} min={1} type="number" />
          </label>
        </div>
        <button className="button button-primary" disabled={buildContext.isPending} type="submit">
          {buildContext.isPending ? 'Resolving…' : 'Build context'}
        </button>
        <MutationFeedback error={buildContext.error} />
      </form>

      {!result ? (
        <PageState
          compact
          kind="empty"
          title="No context built yet"
          description="Choose selectors and an agent, then run the deterministic resolver."
        />
      ) : view === 'preview' ? (
        <div className="context-result">
          <div className="budget-grid" aria-label="Context byte budget">
            <div>
              <span>Used</span>
              <strong>{result.context.budget.usedBytes}</strong>
              <small>bytes</small>
            </div>
            <div>
              <span>Remaining</span>
              <strong>{result.context.budget.remainingBytes}</strong>
              <small>bytes</small>
            </div>
            <div>
              <span>Included</span>
              <strong>{result.context.budget.includedItemCount}</strong>
              <small>items</small>
            </div>
            <div>
              <span>Excluded</span>
              <strong>{result.context.budget.excludedItemCount}</strong>
              <small>items</small>
            </div>
          </div>
          <section aria-labelledby="projection-title">
            <div className="resource-panel-heading">
              <div>
                <p className="panel-label">{result.projection.mediaType}</p>
                <h2 id="projection-title">{result.projection.format}</h2>
              </div>
              <span>{result.projection.sizeInBytes} bytes</span>
            </div>
            <pre className="context-output">
              <code>{result.projection.content}</code>
            </pre>
          </section>
          <section aria-labelledby="included-title">
            <div className="resource-panel-heading">
              <div>
                <p className="panel-label">CANONICAL ITEMS</p>
                <h2 id="included-title">Included</h2>
              </div>
              <span>{result.context.includedItems.length}</span>
            </div>
            <div className="resource-list">
              {result.context.includedItems.map((item) => (
                <article className="resource-card" key={`${item.bindingId}:${item.ruleKey ?? item.revisionId}`}>
                  <div className="resource-card-title">
                    <div>
                      <h3>{item.title}</h3>
                      <p>
                        {item.artifactKind}
                        {item.ruleKey ? ` · ${item.ruleKey}` : ''}
                      </p>
                    </div>
                    {item.userConfirmed === false && (
                      <span className="status-pill status-unconfirmed">[unconfirmed]</span>
                    )}
                  </div>
                  <p className="memory-content">{item.content}</p>
                </article>
              ))}
            </div>
          </section>
        </div>
      ) : (
        <div className="context-result">
          <section aria-labelledby="explanations-title">
            <div className="resource-panel-heading">
              <div>
                <p className="panel-label">DECISIONS</p>
                <h2 id="explanations-title">Explanations</h2>
              </div>
              <span>{result.context.explanations.length}</span>
            </div>
            {result.context.explanations.length === 0 ? (
              <PageState
                compact
                kind="empty"
                title="No resolver decisions"
                description="No bound context matched this selector set."
              />
            ) : (
              <ol className="explanation-list">
                {result.context.explanations.map((explanation) => (
                  <li key={`${explanation.bindingId}:${explanation.decisionCode}`}>
                    <span>{explanation.decisionCode}</span>
                    <p>{explanation.explanation}</p>
                    <code>{explanation.artifactId}</code>
                  </li>
                ))}
              </ol>
            )}
          </section>
          <section aria-labelledby="conflicts-title">
            <div className="resource-panel-heading">
              <div>
                <p className="panel-label">RULE RESOLUTION</p>
                <h2 id="conflicts-title">Conflicts</h2>
              </div>
              <span>{result.context.conflicts.length}</span>
            </div>
            {result.context.conflicts.length === 0 ? (
              <PageState
                compact
                kind="empty"
                title="No conflicts"
                description="All matched rule keys resolved without a conflict."
              />
            ) : (
              <div className="resource-list">
                {result.context.conflicts.map((conflict) => (
                  <article className="resource-card" key={`${conflict.ruleKey}:${conflict.conflictCode}`}>
                    <h3>{conflict.ruleKey}</h3>
                    <p>{conflict.explanation}</p>
                    <code>{conflict.winnerArtifactId ?? 'No winner'}</code>
                  </article>
                ))}
              </div>
            )}
          </section>
          <section aria-labelledby="excluded-title">
            <div className="resource-panel-heading">
              <div>
                <p className="panel-label">BUDGET AND SELECTORS</p>
                <h2 id="excluded-title">Excluded</h2>
              </div>
              <span>{result.context.excludedItems.length}</span>
            </div>
            <div className="resource-list">
              {result.context.excludedItems.map((item) => (
                <article className="resource-card" key={`${item.bindingId}:${item.decisionCode}`}>
                  <div className="resource-card-title">
                    <div>
                      <h3>{item.title}</h3>
                      <p>{item.decisionCode}</p>
                    </div>
                    <span>{item.sizeInBytes} bytes</span>
                  </div>
                  <ul className="selector-list">
                    {item.selectors.map((selector) => (
                      <li key={selector.selector}>
                        <span>{selector.selector}</span>
                        <code>
                          {selector.matched ? 'matched' : `${selector.actual ?? 'none'} ≠ ${selector.expected}`}
                        </code>
                      </li>
                    ))}
                  </ul>
                </article>
              ))}
            </div>
          </section>
        </div>
      )}
    </div>
  );
};
