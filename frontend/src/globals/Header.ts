import { authenticated } from '@/access/authenticated'

import type { GlobalConfig } from 'payload'

export const Header: GlobalConfig = {
  slug: 'header',
  access: {
    read: () => true,
    update: authenticated,
  },
  admin: {
    group: 'Navigation',
    description: 'This is our header navigation',
  },
  fields: [
    {
      name: 'headerLinks',
      type: 'array',
      minRows: 1,
      maxRows: 5,
      fields: [
        {
          name: 'label',
          type: 'text',
          required: true,
        },
        {
          name: 'destination',
          type: 'relationship',
          relationTo: 'pages',
          required: true,
          // Header links are top-level only — nested pages are reached via links within their parent.
          filterOptions: {
            parent: {
              exists: false,
            },
          },
        },
      ],
    },
  ],
}
