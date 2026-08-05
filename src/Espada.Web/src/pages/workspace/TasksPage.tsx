import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { useProjects, useTasks } from 'entities/workspace';
import { useArchiveTask, useCompleteTask, useCreateTask } from 'features/workspace';
import { MutationFeedback, PageState, ResourceQueryState } from 'shared/ui';

const taskFormSchema = z.object({
  projectId: z.string().min(1, 'Select a project.'),
  title: z.string().trim().min(1, 'Task title is required.').max(300),
});

type TaskFormValues = z.infer<typeof taskFormSchema>;

interface TasksPageProps {
  workspaceId: string;
  readOnly: boolean;
}

export const TasksPage = ({ workspaceId, readOnly }: TasksPageProps) => {
  const projects = useProjects(workspaceId);
  const tasks = useTasks(workspaceId);
  const createTask = useCreateTask(workspaceId);
  const completeTask = useCompleteTask(workspaceId);
  const archiveTask = useArchiveTask(workspaceId);
  const {
    formState: { errors },
    handleSubmit,
    register,
    reset,
  } = useForm<TaskFormValues>({
    resolver: zodResolver(taskFormSchema),
    defaultValues: { projectId: '', title: '' },
  });

  const submit = handleSubmit(async (values) => {
    try {
      await createTask.mutateAsync(values);
      reset();
    } catch {
      // MutationFeedback presents the transport or domain error.
    }
  });

  if (tasks.isPending || tasks.error) {
    return (
      <ResourceQueryState
        isPending={tasks.isPending}
        error={tasks.error}
        resourceName="tasks"
        onRetry={() => void tasks.refetch()}
      />
    );
  }

  return (
    <div className="resource-layout">
      <section className="resource-panel" aria-labelledby="task-list-title">
        <div className="resource-panel-heading">
          <div>
            <p className="panel-label">WORKSPACE CONTEXT</p>
            <h2 id="task-list-title">Tasks</h2>
          </div>
          <span>{tasks.data.items.length}</span>
        </div>
        {tasks.data.items.length === 0 ? (
          <PageState
            compact
            kind="empty"
            title="No tasks yet"
            description="Create a project-scoped task to resolve focused agent context."
          />
        ) : (
          <div className="resource-list">
            {tasks.data.items.map((task) => {
              const project = projects.data?.items.find((item) => item.id === task.projectId);
              const status = task.statusTypeName.toLowerCase();
              const isActive = status === 'active';
              const isArchived = status === 'archived';
              const actionPending =
                (completeTask.isPending && completeTask.variables === task.id) ||
                (archiveTask.isPending && archiveTask.variables === task.id);

              return (
                <article className="resource-card" key={task.id}>
                  <div className="resource-card-title">
                    <div>
                      <h3>{task.title}</h3>
                      <p>{project?.name ?? task.projectId}</p>
                    </div>
                    <span className={`status-pill status-${task.statusTypeName.toLowerCase()}`}>
                      {task.statusTypeName}
                    </span>
                  </div>
                  <p className="resource-mono">{task.id}</p>
                  {!isArchived && (
                    <div className="card-actions">
                      {isActive && (
                        <button
                          className="button button-secondary"
                          disabled={readOnly || actionPending}
                          onClick={() => completeTask.mutate(task.id)}
                          type="button"
                        >
                          Complete
                        </button>
                      )}
                      <button
                        className="button button-quiet"
                        disabled={readOnly || actionPending}
                        onClick={() => archiveTask.mutate(task.id)}
                        type="button"
                      >
                        Archive
                      </button>
                    </div>
                  )}
                </article>
              );
            })}
          </div>
        )}
        <MutationFeedback error={completeTask.error ?? archiveTask.error} />
      </section>

      <aside className="form-panel" aria-labelledby="create-task-title">
        <p className="panel-label">NEW TASK</p>
        <h2 id="create-task-title">Create task context</h2>
        {projects.isPending || projects.error ? (
          <ResourceQueryState
            isPending={projects.isPending}
            error={projects.error}
            resourceName="projects"
            onRetry={() => void projects.refetch()}
          />
        ) : projects.data.items.length === 0 ? (
          <PageState
            compact
            kind="empty"
            title="A project is required"
            description="Register a project before creating task context."
          />
        ) : (
          <form onSubmit={submit}>
            <label>
              Project
              <select {...register('projectId')} aria-invalid={Boolean(errors.projectId)} disabled={readOnly}>
                <option value="">Select project</option>
                {projects.data.items.map((project) => (
                  <option key={project.id} value={project.id}>
                    {project.name}
                  </option>
                ))}
              </select>
              {errors.projectId && <span className="field-error">{errors.projectId.message}</span>}
            </label>
            <label>
              Title
              <input
                {...register('title')}
                aria-invalid={Boolean(errors.title)}
                autoComplete="off"
                disabled={readOnly}
              />
              {errors.title && <span className="field-error">{errors.title.message}</span>}
            </label>
            <button className="button button-primary" disabled={readOnly || createTask.isPending} type="submit">
              {createTask.isPending ? 'Creating…' : 'Create task'}
            </button>
            <MutationFeedback
              error={createTask.error}
              isSuccess={createTask.isSuccess}
              successMessage="Task created."
            />
          </form>
        )}
      </aside>
    </div>
  );
};
