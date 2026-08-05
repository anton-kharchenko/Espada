import { describe, expect, it } from 'vitest';
import { importFormSchema } from './importsValidation';

describe('importFormSchema', () => {
  const validValues = {
    sourceId: 'source-one',
    chunkingStrategy: 'Recursive' as const,
    maxCharacters: 2_000,
    overlapCharacters: 200,
    semanticThreshold: 0.75,
  };

  it.each(['', 'local-embedding@v1'])('accepts a deployment default or explicit model: %s', (embeddingModel) => {
    expect(importFormSchema.safeParse({ ...validValues, embeddingModel }).success).toBe(true);
  });

  it('rejects an explicit model without identifier@version format', () => {
    expect(importFormSchema.safeParse({ ...validValues, embeddingModel: 'local-embedding' }).success).toBe(false);
  });
});
