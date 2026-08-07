'use client'

import { Details } from '@/components/Details/Details'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Default">
        <Details summary="Help with nationality">
          We need to know your nationality so we can work out which elections you&apos;re entitled
          to vote in. If you cannot provide your nationality, you&apos;ll have to send copies of
          identity documents through the post.
        </Details>
      </Example>
      <Example title="Open by default">
        <Details open summary="Why we ask for this information">
          This helps us make sure the service is meeting the needs of everyone who uses it.
        </Details>
      </Example>
      <Example title="With a link in the content">
        <Details summary="Help with your reference number">
          You can find this on any letter we&apos;ve sent you. If you cannot find it,{' '}
          <a href="#">contact us</a>.
        </Details>
      </Example>
    </>
  )
}
