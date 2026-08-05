import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router';
import { z } from 'zod';
import { useCreateWorkspace } from 'features/workspace';
import { getWorkspaceRoute, ROUTES } from 'shared/config';
import { LogoMark, MutationFeedback } from 'shared/ui';

const workspaceFormSchema = z.object({
  name: z.string().trim().min(1, 'Workspace name is required.').max(200),
});

type WorkspaceFormValues = z.infer<typeof workspaceFormSchema>;

interface WorkspaceStartPageProps {
  mode: 'local' | 'cloud';
}

export const WorkspaceStartPage = ({ mode }: WorkspaceStartPageProps) => {
  const navigate = useNavigate();
  const createWorkspace = useCreateWorkspace();
  const form = useForm<WorkspaceFormValues>({
    resolver: zodResolver(workspaceFormSchema),
    defaultValues: { name: '' },
  });

  const submit = form.handleSubmit(async (values) => {
    try {
      const result = await createWorkspace.mutateAsync({
        name: values.name,
        typeId: 1,
        organizationId: null,
      });
      navigate(getWorkspaceRoute(result.workspaceId));
    } catch {
      // MutationFeedback presents the transport or domain error.
    }
  });

  return (
    <main className="workspace-start">
      <Link className="console-brand" to={ROUTES.home}>
        <LogoMark />
        <span>Espada</span>
      </Link>
      <section>
        <p className="eyebrow">
          <span /> {mode === 'local' ? 'Local Community' : 'Espada Cloud'}
        </p>
        <h1>Create your first workspace.</h1>
        <p>
          A workspace keeps canonical instructions, memory, sources, and context shared across Codex, Claude, and
          Gemini.
        </p>
        <form className="form-panel" onSubmit={submit}>
          <label>
            Workspace name
            <input
              {...form.register('name')}
              aria-invalid={Boolean(form.formState.errors.name)}
              autoFocus
              placeholder="Espada"
            />
            {form.formState.errors.name && <span className="field-error">{form.formState.errors.name.message}</span>}
          </label>
          <button className="button button-primary" disabled={createWorkspace.isPending} type="submit">
            {createWorkspace.isPending ? 'Creating…' : 'Create local workspace'}
          </button>
          <MutationFeedback error={createWorkspace.error} />
        </form>
        <p className="workspace-start-note">
          Local use is free. Team collaboration is a paid managed capability.{' '}
          <Link to={ROUTES.pricing}>Compare options</Link>.
        </p>
      </section>
    </main>
  );
};
