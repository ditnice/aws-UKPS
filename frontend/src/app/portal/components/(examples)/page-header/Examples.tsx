'use client'

import { PageHeader } from '@nice-digital/nds-page-header'

import { Example } from '../../_components/Example'

const pageHeaderUrl = '/portal/components/page-header'

export function Examples() {
  return (
    <>
      <p>
        Page Header always renders a page-level heading, so this catalogue intentionally contains
        additional H1 elements.
      </p>
      <Example title="Normal with all content regions">
        <PageHeader
          breadcrumbs={<a href={pageHeaderUrl}>Guidance</a>}
          cta={<a href={pageHeaderUrl}>Download guidance</a>}
          description={<p>Recommendations for identifying and managing hypertension in adults.</p>}
          heading="Hypertension in adults"
          lead="Diagnosis and management"
          metadata={['NICE guideline', 'NG136', 'Last updated: 21 November 2023']}
          preheading="Clinical guidance"
          secondSection={
            <div>
              <h2>Information for the public</h2>
              <a href={pageHeaderUrl}>Read the overview</a>
            </div>
          }
          useAltHeading
        />
      </Example>
      <Example dark fullWidth title="Full-width dark with loose padding">
        <PageHeader
          heading="Medicines optimisation"
          lead="Help people get the best outcomes from medicines"
          variant="fullWidthDark"
          verticalPadding="loose"
        />
      </Example>
      <Example fullWidth title="Full-width light">
        <PageHeader
          heading="Shared decision making"
          lead="Make decisions together using the best available evidence"
          variant="fullWidthLight"
        />
      </Example>
    </>
  )
}
