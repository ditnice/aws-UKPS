import { slugField } from 'payload'

import { authenticated } from '@/access/authenticated'
import { authenticatedOrPublished } from '@/access/authenticatedOrPublished'

import type { CollectionConfig } from 'payload'

export const Pages: CollectionConfig = {
  slug: 'pages',
  access: {
    create: authenticated,
    delete: authenticated,
    read: authenticatedOrPublished,
    update: authenticated,
  },
  admin: {
    useAsTitle: 'title',
    defaultColumns: ['title', 'slug', '_status'],
  },
  fields: [
    {
      name: 'title',
      type: 'text',
      required: true,
    },
    slugField({ useAsSlug: 'title' }),
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
        {
          slug: 'accordion',
          fields: [
            {
              name: 'items',
              type: 'array',
              required: true,
              fields: [
                { name: 'title', type: 'text', required: true },
                { name: 'body', type: 'textarea', required: true },
              ],
            },
          ],
        },
        {
          slug: 'columnList',
          fields: [
            { name: 'heading', type: 'text', required: true },
            {
              name: 'columns',
              type: 'select',
              required: true,
              options: ['2', '3'],
            },
            {
              name: 'items',
              type: 'array',
              required: true,
              fields: [{ name: 'text', type: 'text', required: true }],
            },
          ],
        },
      ],
    },
  ],
  versions: { drafts: { autosave: true } },
}
