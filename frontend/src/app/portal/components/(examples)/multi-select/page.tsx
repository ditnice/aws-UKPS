import { ComponentPage } from '../../_components/ComponentPage'
import { getComponentDefinition } from '../../_data/components'

import { Examples } from './Examples'

const component = getComponentDefinition('multi-select')

export const metadata = {
  description: component.description,
  title: component.label,
}

export default function MultiSelectPage() {
  return (
    <ComponentPage marker="custom" title={component.label}>
      <Examples />
    </ComponentPage>
  )
}
