import { ComponentPage } from '../../_components/ComponentPage'
import { getComponentDefinition } from '../../_data/components'

import { Examples } from './Examples'

const component = getComponentDefinition('select')

export const metadata = {
  description: component.description,
  title: component.label,
}

export default function SelectPage() {
  return (
    <ComponentPage marker="custom" title={component.label}>
      <Examples />
    </ComponentPage>
  )
}
