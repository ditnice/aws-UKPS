export const componentDefinitions = [
  { description: 'Present pages in an alphabetical list.', label: 'A-Z List', slug: 'a-z-list' },
  {
    description: 'Show and hide related sections of content.',
    label: 'Accordion',
    slug: 'accordion',
  },
  {
    description: 'Highlight and give context to a call to action.',
    label: 'Action Banner',
    slug: 'action-banner',
  },
  {
    description: 'Notify users of important information or changes.',
    label: 'Alert',
    slug: 'alert',
  },
  {
    description: 'Display a horizontal A-Z list with optional links.',
    label: 'Alphabet',
    slug: 'alphabet',
  },
  {
    description: 'Show a page location within a hierarchy.',
    label: 'Breadcrumbs',
    slug: 'breadcrumbs',
  },
  {
    description: 'Trigger an action or present a prominent link.',
    label: 'Button',
    slug: 'button',
  },
  { description: 'Summarise and link to related content.', label: 'Card', slug: 'card' },
  { description: 'Let users select one or more options.', label: 'Checkbox', slug: 'checkbox' },
  {
    description: 'Display a responsive list in up to three columns.',
    label: 'Column List',
    slug: 'column-list',
  },
  { description: 'Keep page content aligned and readable.', label: 'Container', slug: 'container' },
  { description: 'Narrow a long list of results.', label: 'Filters', slug: 'filters' },
  {
    description: 'Show a relationship between form controls.',
    label: 'Form Group',
    slug: 'form-group',
  },
  {
    description: 'Make content span the full viewport width.',
    label: 'Full Bleed',
    slug: 'full-bleed',
  },
  { description: 'Introduce a site or major section with impact.', label: 'Hero', slug: 'hero' },
  {
    description: 'Navigate between high-level pages or sections.',
    label: 'Horizontal Nav',
    slug: 'horizontal-nav',
  },
  {
    description: 'Navigate to headings within a long page.',
    label: 'In-page Nav',
    slug: 'in-page-nav',
  },
  {
    description: 'Provide the principal introduction to a page.',
    label: 'Page Header',
    slug: 'page-header',
  },
  {
    description: 'Move between pages or sequential content.',
    label: 'Pagination',
    slug: 'pagination',
  },
  {
    description: 'Highlight or separate related supporting content.',
    label: 'Panel',
    slug: 'panel',
  },
  {
    description: 'Identify an alpha or beta service.',
    label: 'Phase Banner',
    slug: 'phase-banner',
  },
  { description: 'Let users select one option from a set.', label: 'Radio', slug: 'radio' },
  {
    description: 'Navigate between subpages in a section.',
    label: 'Stacked Nav',
    slug: 'stacked-nav',
  },
  {
    description: 'Summarise key-value information with optional actions.',
    label: 'Summary List',
    slug: 'summary-list',
  },
  { description: 'Present data for comparison and scanning.', label: 'Table', slug: 'table' },
  { description: 'Switch between related panes of content.', label: 'Tabs', slug: 'tabs' },
  { description: 'Label, categorise or highlight an item.', label: 'Tag', slug: 'tag' },
  {
    description: 'Accept a single line of user-entered text.',
    label: 'Text input',
    slug: 'text-input',
  },
  {
    description: 'Accept multiple lines of user-entered text.',
    label: 'Textarea',
    slug: 'textarea',
  },
] as const

export type ComponentSlug = (typeof componentDefinitions)[number]['slug']

export function getComponentDefinition(slug: ComponentSlug) {
  return componentDefinitions.find((component) => component.slug === slug)!
}
