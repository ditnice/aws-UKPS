'use client'

import { EnhancedPagination } from '@nice-digital/nds-enhanced-pagination'
import { PrevNext } from '@nice-digital/nds-prev-next'
import { SimplePagination } from '@nice-digital/nds-simple-pagination'

import { Example } from '../../_components/Example'

const paginationPath = '/portal/components/pagination'
const mapPageNumberToHref = (pageNumber: number) => `${paginationPath}?page=${pageNumber}`

export function Examples() {
  return (
    <>
      <Example title="Previous and next pages">
        <PrevNext
          nextPageLink={{
            destination: mapPageNumberToHref(4),
            text: 'Monitoring and review',
          }}
          previousPageLink={{
            destination: mapPageNumberToHref(2),
            text: 'Recommendations',
          }}
        />
      </Example>

      <Example title="Previous page only with custom introduction">
        <PrevNext
          previousPageLink={{
            destination: mapPageNumberToHref(2),
            intro: 'Back to',
            text: 'Recommendations',
          }}
        />
      </Example>

      <Example title="Simple pagination">
        <SimplePagination
          currentPage={3}
          nextPageLink={{ destination: mapPageNumberToHref(4) }}
          previousPageLink={{ destination: mapPageNumberToHref(2) }}
          totalPages={8}
        />
      </Example>

      <Example title="Enhanced pagination: first-page edge">
        <EnhancedPagination
          aria-label="Search results pagination, first page"
          currentPage={1}
          mapPageNumberToHref={mapPageNumberToHref}
          totalPages={12}
        />
      </Example>

      <Example title="Enhanced pagination: compact result set">
        <EnhancedPagination
          aria-label="Search results pagination, compact result set"
          currentPage={3}
          mapPageNumberToHref={mapPageNumberToHref}
          totalPages={6}
        />
      </Example>

      <Example title="Enhanced pagination: middle pages">
        <EnhancedPagination
          aria-label="Search results pagination, middle page"
          currentPage={6}
          mapPageNumberToHref={mapPageNumberToHref}
          totalPages={12}
        />
      </Example>

      <Example title="Enhanced pagination: last-page edge">
        <EnhancedPagination
          aria-label="Search results pagination, last page"
          currentPage={12}
          mapPageNumberToHref={mapPageNumberToHref}
          totalPages={12}
        />
      </Example>
    </>
  )
}
