export const pageSizeOptions = [10, 25, 50]

export const defaultPageSize = 10

export function parsePage(page: string | undefined): number {
  const parsedPage = Number(page)

  return Number.isInteger(parsedPage) && parsedPage >= 1 ? parsedPage : 1
}

export function parsePageSize(pageSize: string | undefined): number {
  const parsedPageSize = Number(pageSize)

  return pageSizeOptions.includes(parsedPageSize) ? parsedPageSize : defaultPageSize
}
