import { BackLink } from '@/components/BackLink/BackLink'
import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Example: back link">
        <BackLink href="#">Back</BackLink>
      </Example>
      <Example title="Example: browser back link">
        <BackLinkBrowser />
      </Example>
      <Example dark title="Example: inverse back link">
        <BackLink href="#" variant="inverse">
          Back
        </BackLink>
      </Example>
    </>
  )
}
