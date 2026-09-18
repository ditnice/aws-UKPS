import { Grid, GridItem } from '@nice-digital/nds-grid'

export type TableAndFiltersGridProps = {
  title: string
  filters: React.ReactNode
  table: React.ReactNode
}
export const TableAndFiltersGrid = ({ title, filters, table }: TableAndFiltersGridProps) => {
  return (
    <>
      <h2>{title}</h2>
      <Grid gutter="loose">
        <GridItem cols={12} md={4} lg={3} elementType="section" aria-label="Filter results">
          {filters}
        </GridItem>
        <GridItem cols={12} md={8} lg={9} elementType="section" aria-labelledby="filter-summary">
          {table}
        </GridItem>
      </Grid>
    </>
  )
}
