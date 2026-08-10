import { ComponentPage } from '../../_components/ComponentPage'
import { getComponentDefinition } from '../../_data/components'

import { Examples } from './Examples'

const component = getComponentDefinition('details')

export const metadata = {
  description: component.description,
  title: component.label,
}

export default function DetailsPage() {
  return (
    <ComponentPage marker="custom" title={component.label}>
      <Examples />
    </ComponentPage>
  )
}
