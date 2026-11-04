import { ComponentPage } from '../../_components/ComponentPage'
import { getComponentDefinition } from '../../_data/components'

import { Examples } from './Examples'

const component = getComponentDefinition('badge')

export const metadata = {
  description: component.description,
  title: component.label,
}

export default function BadgePage() {
  return (
    <ComponentPage marker="custom" title={component.label}>
      <Examples />
    </ComponentPage>
  )
}
