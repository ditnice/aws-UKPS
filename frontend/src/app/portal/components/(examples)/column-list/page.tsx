import { ComponentPage } from '../../_components/ComponentPage'
import { getComponentDefinition } from '../../_data/components'

import { Examples } from './Examples'

const component = getComponentDefinition('column-list')

export const metadata = {
  description: component.description,
  title: component.label,
}

export default function ColumnListPage() {
  return (
    <ComponentPage title={component.label}>
      <Examples />
    </ComponentPage>
  )
}
