import { renderToStaticMarkup } from 'react-dom/server';
import { MemoryRouter } from 'react-router';
import { describe, expect, it } from 'vitest';
import { LandingPage } from './LandingPage';

describe('LandingPage', () => {
  it('renders the verified product positioning and primary GitHub path', () => {
    const html = renderToStaticMarkup(
      <MemoryRouter>
        <LandingPage />
      </MemoryRouter>,
    );

    expect(html).toContain('One context.');
    expect(html).toContain('Local use is free. Managed team collaboration is paid.');
    expect(html).toContain('https://github.com/anton-kharchenko/Espada');
    expect(html).toContain('href="/app"');
    expect(html).toContain('href="/pricing"');
    expect(html).toContain('src/Aspire/Aspire.csproj');
  });
});
