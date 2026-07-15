import { ComponentPage } from '../../_components/ComponentPage'
import { getComponentDefinition } from '../../_data/components'

import { Examples } from './Examples'

const component = getComponentDefinition('table')

export const metadata = {
  description: component.description,
  title: component.label,
}

export default function TablePage() {
  return (
    <ComponentPage title={component.label}>
      <Examples />
    </ComponentPage>
  )
}
