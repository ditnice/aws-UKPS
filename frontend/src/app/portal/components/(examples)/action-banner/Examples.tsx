'use client'

import { ActionBanner } from '@nice-digital/nds-action-banner'
import { Button } from '@nice-digital/nds-button'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Overview">
        <ActionBanner title="Sign up for NICE News" cta={<Button variant="cta">Sign up</Button>}>
          Our monthly newsletter, keeping you up to date with{' '}
          <a href="#">important developments at NICE</a>
        </ActionBanner>
      </Example>
      <Example title="Default action banner">
        <ActionBanner title="Default action banner" cta={<Button variant="cta">A CTA</Button>}>
          This is some content with <a href="#">a link</a>
        </ActionBanner>
      </Example>
      <Example title="Subtle action banner">
        <ActionBanner variant="subtle" title="Subtle action banner" cta={<Button>A button</Button>}>
          This is <a href="#">some content with a link</a>
        </ActionBanner>
      </Example>
      <Example fullWidth title="Full width action banner">
        <ActionBanner
          variant="fullWidth"
          title="Full width action banner"
          cta={<Button>A button</Button>}
        >
          This is <a href="#">some content with a link</a>
        </ActionBanner>
      </Example>
      <Example fullWidth title="Full width action banner with image">
        <ActionBanner
          variant="fullWidth"
          title="Full width action banner with image"
          cta={<Button>A button</Button>}
          image="https://placehold.co/800x1200"
        >
          This is <a href="#">some content with a link</a>
        </ActionBanner>
      </Example>
      <Example fullWidth title="Full width subtle action banner with image">
        <ActionBanner
          variant="fullWidthSubtle"
          title="Full width subtle action banner with image"
          cta={<Button>A button</Button>}
          image="https://placehold.co/800x1200"
        >
          This is <a href="#">some content with a link</a>
        </ActionBanner>
      </Example>
      <Example fullWidth title="Full width subtle action banner">
        <ActionBanner
          variant="fullWidthSubtle"
          title="Full width subtle action banner"
          cta={<Button>A button</Button>}
        >
          This is <a href="#">some content with a link</a>
        </ActionBanner>
      </Example>
    </>
  )
}
