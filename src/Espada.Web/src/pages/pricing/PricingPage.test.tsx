import { renderToStaticMarkup } from 'react-dom/server';
import { MemoryRouter } from 'react-router';
import { describe, expect, it } from 'vitest';
import { PricingPage } from './PricingPage';

describe('PricingPage', () => {
  it('states the local-free and managed-team-paid boundary without inventing prices', () => {
    const html = renderToStaticMarkup(
      <MemoryRouter>
        <PricingPage />
      </MemoryRouter>,
    );

    expect(html).toContain('Free on your machine.');
    expect(html).toContain('COMMUNITY · LOCAL');
    expect(html).toContain('TEAM · MANAGED');
    expect(html).toContain('Exact plans and prices will be published');
  });
});
