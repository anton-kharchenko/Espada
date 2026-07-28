import { zodResolver } from '@hookform/resolvers/zod';
import { useForm, useWatch } from 'react-hook-form';
import { z } from 'zod';
import { useArtifacts, useBindings, useProjects, useTasks } from 'entities/workspace';
import { useRemoveBinding, useSetBinding } from 'features/workspace';
import { MutationFeedback, PageState, ResourceQueryState } from 'shared/ui';

const bindingFormSchema = z.object({
  artifactId: z.string().min(1, 'Select an artifact.'),
  projectId: z.string(),
  repositoryCanonicalUri: z.string(),
  repositoryRelativePathPrefix: z.string(),
  branch: z.string(),
  taskId: z.string(),
  agent: z.enum(['', 'codex', 'claude', 'gemini', 'generic']),
});

type BindingFormValues = z.infer<typeof bindingFormSchema>;

const optionalValue = (value: string): null | string => value.trim() || null;

interface BindingsPageProps {
  workspaceId: string;
  readOnly: boolean;
}

export const BindingsPage = ({ workspaceId, readOnly }: BindingsPageProps) => {
  const bindings = useBindings(workspaceId);
  const artifacts = useArtifacts(workspaceId, 'artifacts');
  const projects = useProjects(workspaceId);
  const tasks = useTasks(workspaceId);
  const setBinding = useSetBinding(workspaceId);
  const removeBinding = useRemoveBinding(workspaceId);
  const {
    control,
    formState: { errors },
    handleSubmit,
    register,
    reset,
  } = useForm<BindingFormValues>({
    resolver: zodResolver(bindingFormSchema),
    defaultValues: {
      artifactId: '',
      projectId: '',
      repositoryCanonicalUri: '',
      repositoryRelativePathPrefix: '',
      branch: '',
      taskId: '',
      agent: '',
    },
  });
  const selectedProjectId = useWatch({
    control,
    name: 'projectId',
  });
  const availableTasks = tasks.data?.items.filter((task) => !selectedProjectId || task.projectId === selectedProjectId);

  const submit = handleSubmit(async (values) => {
    try {
      await setBinding.mutateAsync({
        artifactId: values.artifactId,
        projectId: optionalValue(values.projectId),
        repositoryCanonicalUri: optionalValue(values.repositoryCanonicalUri),
        repositoryRelativePathPrefix: optionalValue(values.repositoryRelativePathPrefix),
        branch: optionalValue(values.branch),
        taskId: optionalValue(values.taskId),
        agent: optionalValue(values.agent),
      });
      reset();
    } catch {
      // MutationFeedback presents the transport or domain error.
    }
  });

  if (bindings.isPending || bindings.error) {
    return (
      <ResourceQueryState
        isPending={bindings.isPending}
        error={bindings.error}
        resourceName="bindings"
        onRetry={() => void bindings.refetch()}
      />
    );
  }

  return (
    <div className="resource-layout">
      <section className="resource-panel" aria-labelledby="binding-list-title">
        <div className="resource-panel-heading">
          <div>
            <p className="panel-label">SELECTOR SCOPES</p>
            <h2 id="binding-list-title">Bindings</h2>
          </div>
          <span>{bindings.data.items.length}</span>
        </div>
        {bindings.data.items.length === 0 ? (
          <PageState
            compact
            kind="empty"
            title="No bindings yet"
            description="Unbound artifacts are not selected by the canonical resolver."
          />
        ) : (
          <div className="resource-list">
            {bindings.data.items.map((binding) => {
              const selectors = [
                ['project', binding.projectId],
                ['repository', binding.repositoryCanonicalUri],
                ['path', binding.repositoryRelativePathPrefix],
                ['branch', binding.branch],
                ['task', binding.taskId],
                ['agent', binding.agent],
              ].filter((selector): selector is [string, string] => Boolean(selector[1]));

              return (
                <article className="resource-card" key={binding.id}>
                  <div className="resource-card-title">
                    <div>
                      <h3>Revision {binding.artifactRevisionId}</h3>
                      <p>{selectors.length === 0 ? 'Workspace-wide' : `${selectors.length} selectors`}</p>
                    </div>
                    <button
                      className="button button-danger"
                      disabled={readOnly || removeBinding.isPending}
                      onClick={() => removeBinding.mutate(binding.id)}
                      type="button"
                    >
                      Remove
                    </button>
                  </div>
                  <ul className="selector-list">
                    {selectors.length === 0 ? (
                      <li>
                        <span>workspace</span>
                        <code>{workspaceId}</code>
                      </li>
                    ) : (
                      selectors.map(([name, value]) => (
                        <li key={name}>
                          <span>{name}</span>
                          <code>{value}</code>
                        </li>
                      ))
                    )}
                  </ul>
                </article>
              );
            })}
          </div>
        )}
        <MutationFeedback error={removeBinding.error} />
      </section>

      <aside className="form-panel" aria-labelledby="set-binding-title">
        <p className="panel-label">NEW BINDING</p>
        <h2 id="set-binding-title">Bind current revision</h2>
        {artifacts.isPending || artifacts.error ? (
          <ResourceQueryState
            isPending={artifacts.isPending}
            error={artifacts.error}
            resourceName="artifacts"
            onRetry={() => void artifacts.refetch()}
          />
        ) : artifacts.data.items.length === 0 ? (
          <PageState
            compact
            kind="empty"
            title="An artifact is required"
            description="Create an instruction, policy, document, or memory first."
          />
        ) : (
          <form onSubmit={submit}>
            <label>
              Artifact
              <select {...register('artifactId')} aria-invalid={Boolean(errors.artifactId)} disabled={readOnly}>
                <option value="">Select artifact</option>
                {artifacts.data.items
                  .filter((artifact) => artifact.currentRevisionId)
                  .map((artifact) => (
                    <option key={artifact.id} value={artifact.id}>
                      {artifact.title} · {artifact.kindTypeName}
                    </option>
                  ))}
              </select>
              {errors.artifactId && <span className="field-error">{errors.artifactId.message}</span>}
            </label>
            <label>
              Project
              <select {...register('projectId')} disabled={readOnly}>
                <option value="">Any project</option>
                {projects.data?.items.map((project) => (
                  <option key={project.id} value={project.id}>
                    {project.name}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Repository canonical URI
              <input {...register('repositoryCanonicalUri')} disabled={readOnly} type="url" />
            </label>
            <div className="form-row">
              <label>
                Path prefix
                <input {...register('repositoryRelativePathPrefix')} disabled={readOnly} placeholder="src/Espada.Api" />
              </label>
              <label>
                Branch
                <input {...register('branch')} disabled={readOnly} placeholder="feature/*" />
              </label>
            </div>
            <div className="form-row">
              <label>
                Task
                <select {...register('taskId')} disabled={readOnly}>
                  <option value="">Any task</option>
                  {availableTasks?.map((task) => (
                    <option key={task.id} value={task.id}>
                      {task.title}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Agent
                <select {...register('agent')} disabled={readOnly}>
                  <option value="">Any agent</option>
                  <option value="codex">Codex</option>
                  <option value="claude">Claude</option>
                  <option value="gemini">Gemini</option>
                  <option value="generic">Generic MCP</option>
                </select>
              </label>
            </div>
            <button className="button button-primary" disabled={readOnly || setBinding.isPending} type="submit">
              {setBinding.isPending ? 'Binding…' : 'Set binding'}
            </button>
            <MutationFeedback
              error={setBinding.error}
              isSuccess={setBinding.isSuccess}
              successMessage="Binding saved."
            />
          </form>
        )}
      </aside>
    </div>
  );
};
