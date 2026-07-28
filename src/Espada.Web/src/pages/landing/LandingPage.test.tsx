import { renderToStaticMarkup } from 'react-dom/server';
import { describe, expect, it } from 'vitest';
import { LandingPage } from './LandingPage';

describe('LandingPage', () => {
  it('renders the verified product positioning and primary GitHub path', () => {
    const html = renderToStaticMarkup(<LandingPage />);

    expect(html).toContain('One context.');
    expect(html).toContain('Local work stays local. Cloud sync stays optional.');
    expect(html).toContain('https://github.com/anton-kharchenko/Espada');
    expect(html).toContain('href="/app"');
    expect(html).toContain('src/Aspire/Aspire.csproj');
  });
});
