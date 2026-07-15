import { ComponentPage } from '../../_components/ComponentPage'
import { getComponentDefinition } from '../../_data/components'

import { Examples } from './Examples'

const component = getComponentDefinition('alphabet')

export const metadata = {
  description: component.description,
  title: component.label,
}

export default function AlphabetPage() {
  return (
    <ComponentPage title={component.label}>
      <Examples />
    </ComponentPage>
  )
}
