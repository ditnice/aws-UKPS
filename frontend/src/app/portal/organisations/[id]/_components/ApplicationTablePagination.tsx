import Link from 'next/link'
import { ComponentProps } from 'react'

import { EnhancedPagination } from '@nice-digital/nds-enhanced-pagination'
import { Grid, GridItem } from '@nice-digital/nds-grid'

import { pageSizeOptions } from '@/lib/search-and-filter/pagination'

import styles from './ApplicationTablePagination.module.scss'

const PaginationLink = ({ children, ...props }: ComponentProps<typeof Link>) => {
  return (
    <Link {...props} scroll={false}>
      {children}
    </Link>
  )
}

type ApplicationTablePaginationProps<TQuery> = {
  result: {
    totalCount: number
    page: number
    pageSize: number
  }
  query: TQuery
  queryToSearchParams: (query: TQuery) => URLSearchParams
}
export const ApplicationTablePagination = <TQuery,>({
  query,
  result,
  queryToSearchParams,
}: ApplicationTablePaginationProps<TQuery>) => {
  const buildUserListHref = (query: TQuery) => {
    const searchParams = queryToSearchParams(query)
    return `?${searchParams.toString()}`
  }

  return (
    <Grid verticalAlignment="middle">
      <GridItem cols={12} sm={6}>
        <EnhancedPagination
          currentPage={result.page}
          elementType={PaginationLink}
          mapPageNumberToHref={(pageNumber) => buildUserListHref({ ...query, page: pageNumber })}
          totalPages={Math.ceil(result.totalCount / result.pageSize)}
        />
      </GridItem>
      <GridItem cols={12} sm={6} className="text-right">
        <p className={styles.resultsPerPageHeading}>Results per page</p>
        <ol className={`list list--piped ${styles.resultsPerPageList}`}>
          {pageSizeOptions.map((count) => (
            <li key={count}>
              {result.pageSize === count ? (
                count
              ) : (
                <PaginationLink href={buildUserListHref({ ...query, page: 1, pageSize: count })}>
                  {count}
                </PaginationLink>
              )}
            </li>
          ))}
        </ol>
      </GridItem>
    </Grid>
  )
}
