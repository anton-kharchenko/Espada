import { zodResolver } from '@hookform/resolvers/zod';
import { useForm, useWatch } from 'react-hook-form';
import { z } from 'zod';
import { useSources, type SourceDefinitionRequest } from 'entities/workspace';
import { useRegisterSource } from 'features/workspace';
import { MutationFeedback, PageState, ResourceQueryState } from 'shared/ui';

const sourceTypes = ['file', 'webPage', 'plainText', 'conversation', 'connector'] as const;
type SourceType = (typeof sourceTypes)[number];

const sourceFormSchema = z
  .object({
    name: z.string().trim().min(1, 'Source name is required.').max(200),
    sourceType: z.enum(sourceTypes),
    primary: z.string().trim(),
    secondary: z.string().trim(),
    tertiary: z.string().trim(),
    content: z.string(),
  })
  .superRefine((values, context) => {
    if (!values.primary) {
      context.addIssue({ code: 'custom', message: 'This value is required.', path: ['primary'] });
    }
    if (values.sourceType === 'webPage' && values.primary) {
      try {
        const uri = new URL(values.primary);
        if (uri.protocol !== 'https:') throw new Error();
      } catch {
        context.addIssue({ code: 'custom', message: 'Enter an absolute HTTPS URL.', path: ['primary'] });
      }
    }
    if (values.sourceType === 'file' && !values.secondary) {
      context.addIssue({ code: 'custom', message: 'File name is required.', path: ['secondary'] });
    }
    if (values.sourceType === 'file' && !values.tertiary) {
      context.addIssue({ code: 'custom', message: 'Media type is required.', path: ['tertiary'] });
    }
    if (values.sourceType === 'connector' && (!values.secondary || !values.tertiary)) {
      context.addIssue({ code: 'custom', message: 'Version and resource are required.', path: ['secondary'] });
    }
    if (
      (values.sourceType === 'plainText' ||
        values.sourceType === 'conversation' ||
        values.sourceType === 'connector') &&
      !values.content.trim()
    ) {
      context.addIssue({ code: 'custom', message: 'Content is required.', path: ['content'] });
    }
  });

type SourceFormValues = z.infer<typeof sourceFormSchema>;

const fieldCopy: Record<SourceType, { primary: string; secondary?: string; tertiary?: string; content?: string }> = {
  file: {
    primary: 'Local absolute path',
    secondary: 'File name',
    tertiary: 'Media type',
  },
  webPage: {
    primary: 'HTTPS URL',
  },
  plainText: {
    primary: 'Document title',
    content: 'Text content',
  },
  conversation: {
    primary: 'Conversation title',
    secondary: 'Message role',
    tertiary: 'Author',
    content: 'Message content',
  },
  connector: {
    primary: 'Plugin ID',
    secondary: 'Version',
    tertiary: 'Resource',
    content: 'Arguments JSON',
  },
};

const buildDefinition = (values: SourceFormValues): SourceDefinitionRequest => {
  switch (values.sourceType) {
    case 'file':
      return {
        type: 'file',
        localPath: values.primary,
        blob: null,
        fileName: values.secondary,
        mediaType: values.tertiary,
      };
    case 'webPage':
      return { type: 'webPage', uri: values.primary };
    case 'plainText':
      return { type: 'plainText', title: values.primary, content: values.content };
    case 'conversation':
      return {
        type: 'conversation',
        title: values.primary,
        messages: [
          {
            role: values.secondary || 'user',
            author: values.tertiary || null,
            content: values.content,
            timestamp: null,
          },
        ],
      };
    case 'connector':
      return {
        type: 'connector',
        pluginId: values.primary,
        version: values.secondary,
        resource: values.tertiary,
        arguments: z.record(z.string(), z.unknown()).parse(JSON.parse(values.content)),
      };
  }
};

interface SourcesPageProps {
  workspaceId: string;
  readOnly: boolean;
}

export const SourcesPage = ({ workspaceId, readOnly }: SourcesPageProps) => {
  const sources = useSources(workspaceId);
  const registerSource = useRegisterSource(workspaceId);
  const form = useForm<SourceFormValues>({
    resolver: zodResolver(sourceFormSchema),
    defaultValues: {
      name: '',
      sourceType: 'plainText',
      primary: '',
      secondary: '',
      tertiary: '',
      content: '',
    },
  });
  const sourceType = useWatch({
    control: form.control,
    name: 'sourceType',
  });
  const copy = fieldCopy[sourceType];

  const submit = form.handleSubmit(async (values) => {
    let definition: SourceDefinitionRequest;
    try {
      definition = buildDefinition(values);
    } catch {
      form.setError('content', { message: 'Connector arguments must be a JSON object.' });
      return;
    }

    try {
      await registerSource.mutateAsync({ name: values.name, definition });
      form.reset();
    } catch {
      // MutationFeedback presents the transport or domain error.
    }
  });

  if (sources.isPending || sources.error) {
    return (
      <ResourceQueryState
        isPending={sources.isPending}
        error={sources.error}
        resourceName="sources"
        onRetry={() => void sources.refetch()}
      />
    );
  }

  return (
    <div className="resource-layout">
      <section className="resource-panel" aria-labelledby="source-list-title">
        <div className="resource-panel-heading">
          <div>
            <p className="panel-label">INGESTION INPUTS</p>
            <h2 id="source-list-title">Sources</h2>
          </div>
          <span>{sources.data.items.length}</span>
        </div>
        {sources.data.items.length === 0 ? (
          <PageState
            compact
            kind="empty"
            title="No sources registered"
            description="Register a file, web page, plain text, conversation, or connector source."
          />
        ) : (
          <div className="resource-list">
            {sources.data.items.map((source) => (
              <article className="resource-card" key={source.id}>
                <div className="resource-card-title">
                  <div>
                    <h3>{source.name}</h3>
                    <p>{source.typeName}</p>
                  </div>
                  <span className={`status-pill status-${source.statusName.toLowerCase()}`}>{source.statusName}</span>
                </div>
                <p className="resource-mono">{source.locator}</p>
                <dl className="resource-meta">
                  <div>
                    <dt>Priority</dt>
                    <dd>{source.priority}</dd>
                  </div>
                  <div>
                    <dt>Source ID</dt>
                    <dd>{source.id}</dd>
                  </div>
                </dl>
              </article>
            ))}
          </div>
        )}
      </section>

      <aside className="form-panel" aria-labelledby="register-source-title">
        <p className="panel-label">NEW SOURCE</p>
        <h2 id="register-source-title">Register source</h2>
        <form onSubmit={submit}>
          <label>
            Name
            <input {...form.register('name')} aria-invalid={Boolean(form.formState.errors.name)} disabled={readOnly} />
            {form.formState.errors.name && <span className="field-error">{form.formState.errors.name.message}</span>}
          </label>
          <label>
            Source type
            <select {...form.register('sourceType')} disabled={readOnly}>
              <option value="file">Local file</option>
              <option value="webPage">Web page</option>
              <option value="plainText">Plain text</option>
              <option value="conversation">Conversation</option>
              <option value="connector">Connector</option>
            </select>
          </label>
          <label>
            {copy.primary}
            <input
              {...form.register('primary')}
              aria-invalid={Boolean(form.formState.errors.primary)}
              disabled={readOnly}
              type={sourceType === 'webPage' ? 'url' : 'text'}
            />
            {form.formState.errors.primary && (
              <span className="field-error">{form.formState.errors.primary.message}</span>
            )}
          </label>
          {copy.secondary && (
            <label>
              {copy.secondary}
              <input {...form.register('secondary')} disabled={readOnly} />
              {form.formState.errors.secondary && (
                <span className="field-error">{form.formState.errors.secondary.message}</span>
              )}
            </label>
          )}
          {copy.tertiary && (
            <label>
              {copy.tertiary}
              <input {...form.register('tertiary')} disabled={readOnly} />
              {form.formState.errors.tertiary && (
                <span className="field-error">{form.formState.errors.tertiary.message}</span>
              )}
            </label>
          )}
          {copy.content && (
            <label>
              {copy.content}
              <textarea
                {...form.register('content')}
                aria-invalid={Boolean(form.formState.errors.content)}
                disabled={readOnly}
                placeholder={sourceType === 'connector' ? '{}' : undefined}
                rows={6}
              />
              {form.formState.errors.content && (
                <span className="field-error">{form.formState.errors.content.message}</span>
              )}
            </label>
          )}
          <button className="button button-primary" disabled={readOnly || registerSource.isPending} type="submit">
            {registerSource.isPending ? 'Registering…' : 'Register source'}
          </button>
          <MutationFeedback
            error={registerSource.error}
            isSuccess={registerSource.isSuccess}
            successMessage="Source registered."
          />
        </form>
      </aside>
    </div>
  );
};
