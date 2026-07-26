import { renderToStaticMarkup } from 'react-dom/server';
import { describe, expect, it } from 'vitest';
import { App } from './App';

describe('App', () => {
  it('renders the verified product positioning and primary GitHub path', () => {
    const html = renderToStaticMarkup(<App />);

    expect(html).toContain('One context.');
    expect(html).toContain('Local work stays local. Cloud sync stays optional.');
    expect(html).toContain('https://github.com/anton-kharchenko/Espada');
    expect(html).toContain('src/Aspire/Aspire.csproj');
  });
});
