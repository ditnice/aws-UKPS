'use client'

import { useState } from 'react'

import { InPageNav } from '@nice-digital/nds-in-page-nav'

import { Example } from '../../_components/Example'

type InPageNavVariant = 'default' | 'no-scroll' | 'two-columns'

export function Examples() {
  const [inPageNavVariant, setInPageNavVariant] = useState<InPageNavVariant>('default')
  const noScroll = inPageNavVariant !== 'default'

  return (
    <Example title="Default, no-scroll and two-column variants">
      <fieldset>
        <legend>In-page navigation variant</legend>
        <label>
          <input
            checked={inPageNavVariant === 'default'}
            name="in-page-nav-variant"
            onChange={() => setInPageNavVariant('default')}
            type="radio"
          />{' '}
          Default scrolling
        </label>{' '}
        <label>
          <input
            checked={inPageNavVariant === 'no-scroll'}
            name="in-page-nav-variant"
            onChange={() => setInPageNavVariant('no-scroll')}
            type="radio"
          />{' '}
          No scroll
        </label>{' '}
        <label>
          <input
            checked={inPageNavVariant === 'two-columns'}
            name="in-page-nav-variant"
            onChange={() => setInPageNavVariant('two-columns')}
            type="radio"
          />{' '}
          No scroll, two columns
        </label>
      </fieldset>

      <InPageNav
        key={inPageNavVariant}
        headingsContainerSelector="#in-page-nav-example-content"
        headingsSelector="h3, h4"
        noScroll={noScroll}
        twoColumns={inPageNavVariant === 'two-columns'}
      />

      <div id="in-page-nav-example-content">
        <h3 id="in-page-overview">Overview</h3>
        <p>The links above are generated from the headings in this example only.</p>
        <h3 id="in-page-details">Details</h3>
        <h4 id="in-page-eligibility">Eligibility</h4>
        <p>Lower-level headings are displayed as nested navigation items.</p>
        <h4 id="in-page-next-steps">Next steps</h4>
        <p>Each generated link uses the corresponding heading ID.</p>
      </div>
    </Example>
  )
}
