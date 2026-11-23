import { ComponentPage } from '../../_components/ComponentPage'
import { getComponentDefinition } from '../../_data/components'

import { Examples } from './Examples'

const component = getComponentDefinition('autocomplete')

export const metadata = {
  description: component.description,
  title: component.label,
}

export default function AutocompletePage() {
  return (
    <ComponentPage marker="custom" title={component.label}>
      <Examples />
    </ComponentPage>
  )
}
