import { ComponentPage } from '../../_components/ComponentPage'
import { getComponentDefinition } from '../../_data/components'

import { Examples } from './Examples'

const component = getComponentDefinition('back-link')

export const metadata = {
  description: component.description,
  title: component.label,
}

export default function BackLinkPage() {
  return (
    <ComponentPage custom title={component.label}>
      <Examples />
    </ComponentPage>
  )
}
