import { z } from 'zod';

export const importFormSchema = z
  .object({
    sourceId: z.string().min(1, 'Select a source.'),
    embeddingModel: z
      .string()
      .trim()
      .regex(/^$|^[^@\s]+@[^@\s]+$/, 'Use identifier@version format.'),
    chunkingStrategy: z.enum(['FixedSize', 'Recursive', 'Markdown', 'Semantic', 'Code', 'Custom']),
    maxCharacters: z.number().int().positive(),
    overlapCharacters: z.number().int().nonnegative(),
    semanticThreshold: z.number().min(0).max(1),
  })
  .refine((values) => values.overlapCharacters < values.maxCharacters, {
    message: 'Overlap must be smaller than max characters.',
    path: ['overlapCharacters'],
  });

export type ImportFormValues = z.infer<typeof importFormSchema>;
