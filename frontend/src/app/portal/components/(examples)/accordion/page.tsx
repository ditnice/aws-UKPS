import { ComponentPage } from '../../_components/ComponentPage'
import { getComponentDefinition } from '../../_data/components'

import { Examples } from './Examples'

const component = getComponentDefinition('accordion')

export const metadata = {
  description: component.description,
  title: component.label,
}

export default function AccordionPage() {
  return (
    <ComponentPage title={component.label}>
      <Examples />
    </ComponentPage>
  )
}
