import { ComponentPage } from '../../_components/ComponentPage'
import { getComponentDefinition } from '../../_data/components'

import { Examples } from './Examples'

const component = getComponentDefinition('button')

export const metadata = {
  description: component.description,
  title: component.label,
}

export default function ButtonPage() {
  return (
    <ComponentPage marker="wrapper" title={component.label}>
      <Examples />
    </ComponentPage>
  )
}
