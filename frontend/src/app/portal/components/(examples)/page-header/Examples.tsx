'use client'

import { Breadcrumb, Breadcrumbs } from '@nice-digital/nds-breadcrumbs'
import { Button } from '@nice-digital/nds-button'
import { PageHeader } from '@nice-digital/nds-page-header'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Heading">
        <PageHeader heading="Welcome to the page" />
      </Example>
      <Example title="Pre-heading">
        <PageHeader heading="Welcome to the page" preheading="Here's a pre-heading" />
      </Example>
      <Example title="Lead">
        <PageHeader heading="Welcome to the page" lead="This is the lead" />
      </Example>
      <Example title="Metadata">
        <PageHeader heading="Welcome to the page" metadata={['Item 1', 'Item 2']} />
      </Example>
      <Example title="Call-to-action">
        <PageHeader heading="Welcome to the page" cta={<Button>Do something</Button>} />
      </Example>
      <Example title="Header with description">
        <PageHeader heading="Header with description" description="I am a description" />
      </Example>
      <Example title="Header with breadcrumbs">
        <PageHeader
          heading="Header with breadcrumbs"
          breadcrumbs={
            <Breadcrumbs>
              <Breadcrumb to="https://www.nice.org.uk/">Home</Breadcrumb>
              <Breadcrumb to="https://www.nice.org.uk/guidance">NICE guidance</Breadcrumb>
              <Breadcrumb>Published</Breadcrumb>
            </Breadcrumbs>
          }
        />
      </Example>
      <Example title="verticalPadding">
        <PageHeader heading="I have vertical padding!" verticalPadding="loose" />
      </Example>
      <Example fullWidth title="Full width light variant">
        <PageHeader
          heading="I am a full width light header!"
          variant="fullWidthLight"
          verticalPadding="loose"
        />
      </Example>
      <Example fullWidth title="Full width dark variant">
        <PageHeader
          heading="I am a full width dark header!"
          variant="fullWidthDark"
          verticalPadding="loose"
        />
      </Example>
      <Example fullWidth title="Header with second section">
        <PageHeader
          heading="There's a second section here!"
          variant="fullWidthLight"
          verticalPadding="loose"
          secondSection={
            <aside>
              <h3>I am a second section</h3>
              <ol>
                <li>One</li>
                <li>Two</li>
                <li>Three</li>
              </ol>
            </aside>
          }
        />
      </Example>
    </>
  )
}
