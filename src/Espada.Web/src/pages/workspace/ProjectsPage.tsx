import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { useProjects } from 'entities/workspace';
import { useCreateProject } from 'features/workspace';
import { MutationFeedback, PageState, ResourceQueryState } from 'shared/ui';

const projectFormSchema = z.object({
  name: z.string().trim().min(1, 'Project name is required.').max(200),
  canonicalRemoteUri: z
    .string()
    .trim()
    .refine((value) => value.length === 0 || z.url().safeParse(value).success, 'Enter an absolute repository URL.'),
  localAliases: z.string(),
});

type ProjectFormValues = z.infer<typeof projectFormSchema>;

interface ProjectsPageProps {
  workspaceId: string;
  readOnly: boolean;
}

export const ProjectsPage = ({ workspaceId, readOnly }: ProjectsPageProps) => {
  const projects = useProjects(workspaceId);
  const createProject = useCreateProject(workspaceId);
  const {
    formState: { errors },
    handleSubmit,
    register,
    reset,
  } = useForm<ProjectFormValues>({
    resolver: zodResolver(projectFormSchema),
    defaultValues: {
      name: '',
      canonicalRemoteUri: '',
      localAliases: '',
    },
  });

  const submit = handleSubmit(async (values) => {
    try {
      await createProject.mutateAsync({
        name: values.name,
        canonicalRemoteUri: values.canonicalRemoteUri || null,
        localAliases: values.localAliases
          .split(/\r?\n/)
          .map((value) => value.trim())
          .filter(Boolean),
      });
      reset();
    } catch {
      // MutationFeedback presents the transport or domain error.
    }
  });

  const queryState = (
    <ResourceQueryState
      isPending={projects.isPending}
      error={projects.error}
      resourceName="projects"
      onRetry={() => void projects.refetch()}
    />
  );

  return (
    <div className="resource-layout">
      <section className="resource-panel" aria-labelledby="project-list-title">
        <div className="resource-panel-heading">
          <div>
            <p className="panel-label">REGISTERED</p>
            <h2 id="project-list-title">Projects</h2>
          </div>
          <span>{projects.data?.items.length ?? 0}</span>
        </div>
        {projects.isPending || projects.error ? (
          queryState
        ) : projects.data.items.length === 0 ? (
          <PageState
            compact
            kind="empty"
            title="No projects registered"
            description="Add a canonical repository and optional local path aliases."
          />
        ) : (
          <div className="resource-list">
            {projects.data.items.map((project) => (
              <article className="resource-card" key={project.id}>
                <div>
                  <h3>{project.name}</h3>
                  <p className="resource-mono">{project.canonicalRemoteUri ?? 'Local-only project'}</p>
                </div>
                <dl className="resource-meta">
                  <div>
                    <dt>Local aliases</dt>
                    <dd>{project.localAliases.length || 'None'}</dd>
                  </div>
                  <div>
                    <dt>Project ID</dt>
                    <dd>{project.id}</dd>
                  </div>
                </dl>
                {project.localAliases.length > 0 && (
                  <ul className="tag-list" aria-label={`${project.name} local aliases`}>
                    {project.localAliases.map((alias) => (
                      <li key={alias}>{alias}</li>
                    ))}
                  </ul>
                )}
              </article>
            ))}
          </div>
        )}
      </section>

      <aside className="form-panel" aria-labelledby="create-project-title">
        <p className="panel-label">NEW PROJECT</p>
        <h2 id="create-project-title">Register a repository</h2>
        <form onSubmit={submit}>
          <label>
            Name
            <input {...register('name')} aria-invalid={Boolean(errors.name)} autoComplete="off" disabled={readOnly} />
            {errors.name && <span className="field-error">{errors.name.message}</span>}
          </label>
          <label>
            Canonical remote URI (optional)
            <input
              {...register('canonicalRemoteUri')}
              aria-invalid={Boolean(errors.canonicalRemoteUri)}
              autoComplete="url"
              disabled={readOnly}
              placeholder="https://github.com/owner/repository.git"
              type="url"
            />
            {errors.canonicalRemoteUri && <span className="field-error">{errors.canonicalRemoteUri.message}</span>}
          </label>
          <label>
            Local aliases
            <textarea
              {...register('localAliases')}
              disabled={readOnly}
              placeholder={'C:\\Startups\\Espada\nD:\\src\\Espada'}
              rows={3}
            />
            <span className="field-hint">One absolute path per line.</span>
          </label>
          <button className="button button-primary" disabled={readOnly || createProject.isPending} type="submit">
            {createProject.isPending ? 'Registering…' : 'Register project'}
          </button>
          <MutationFeedback
            error={createProject.error}
            isSuccess={createProject.isSuccess}
            successMessage="Project registered."
          />
        </form>
      </aside>
    </div>
  );
};
