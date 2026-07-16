'use client'

import { EnhancedPagination } from '@nice-digital/nds-enhanced-pagination'
import { PrevNext } from '@nice-digital/nds-prev-next'
import { SimplePagination } from '@nice-digital/nds-simple-pagination'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Example: previous/next links">
        <PrevNext
          previousPageLink={{ text: 'Overview', destination: '#' }}
          nextPageLink={{
            text: 'The condition, current treatments and procedure',
            destination: '#',
          }}
        />
      </Example>

      <Example title="Example: simple pagination">
        <SimplePagination
          totalPages={7}
          currentPage={2}
          nextPageLink={{ destination: '#', elementType: 'a' }}
          previousPageLink={{ destination: '#', elementType: 'a' }}
        />
      </Example>

      <Example title="Example: enhanced pagination">
        <EnhancedPagination
          currentPage={2}
          mapPageNumberToHref={(pageNumber) => `#${pageNumber}`}
          totalPages={7}
        />
      </Example>
    </>
  )
}
