type PaginatedResultSummaryProps<T> = {
  result?: {
    items: Array<T>
    totalCount: number
    page: number
    pageSize: number
  }
}
export const PaginatedResultSummary = <T,>({ result }: PaginatedResultSummaryProps<T>) => {
  const totalCount = result?.totalCount ?? 0

  const getFirstResult = (totalCount: number, currentPage: number, pageSize: number): number => {
    return totalCount === 0 ? 0 : (currentPage - 1) * pageSize + 1
  }

  const getLastResult = (totalCount: number, currentPage: number, pageSize: number): number => {
    return Math.min(currentPage * pageSize, totalCount)
  }
  return (
    <>
      {result
        ? `Showing results ${getFirstResult(totalCount, result.page, result.pageSize)} to ${getLastResult(totalCount, result.page, result.pageSize)} of ${totalCount}`
        : 'Showing results'}
    </>
  )
}
