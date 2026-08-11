import type { CollectionConfig } from 'payload'

export const Pages: CollectionConfig = {
  slug: 'pages',
  admin: {
    useAsTitle: 'title',
    defaultColumns: ['title', 'path', 'navigationGroup'],
  },
  fields: [
    {
      name: 'title',
      type: 'text',
      required: true,
    },
    {
      name: 'slug',
      type: 'text',
      required: true,
      admin: {
        description: 'URL-safe identifier, e.g. "home" or "about-us"',
      },
    },
    {
      name: 'path',
      type: 'text',
      required: true,
      unique: true,
      admin: {
        description: 'Full path, e.g. "/" or "/about-us/what-is-uk-pharmascan"',
      },
    },
    {
      name: 'navigationGroup',
      type: 'text',
    },
    {
      name: 'navigationLabel',
      type: 'text',
    },
    {
      name: 'navigationOrder',
      type: 'number',
    },
    {
      name: 'layout',
      type: 'blocks',
      required: true,
      blocks: [
        {
          slug: 'textSection',
          fields: [
            { name: 'heading', type: 'text', required: true },
            { name: 'body', type: 'textarea', required: true },
            {
              name: 'variant',
              type: 'select',
              defaultValue: 'default',
              options: ['default', 'homeStandard'],
            },
          ],
        },
        {
          slug: 'tabs',
          fields: [
            {
              name: 'tabs',
              type: 'array',
              required: true,
              fields: [
                { name: 'title', type: 'text', required: true },
                { name: 'body', type: 'textarea', required: true },
              ],
            },
          ],
        },
      ],
    },
  ],
}
