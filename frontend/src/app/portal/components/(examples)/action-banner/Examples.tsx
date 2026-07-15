'use client'

import { ActionBanner } from '@nice-digital/nds-action-banner'
import { Button } from '@nice-digital/nds-button'

import { Example } from '../../_components/Example'
import { illustration } from '../../_data/illustration'

const actionBannerVariants = [
  {
    body: 'Sign up to receive an email when this guidance is updated.',
    title: 'Keep up to date',
    variant: 'default',
  },
  {
    body: 'Read a concise overview of the recommendations for care teams.',
    title: 'Guidance at a glance',
    variant: 'subtle',
  },
  {
    body: 'Explore practical resources for putting this guidance into practice.',
    title: 'Tools and resources',
    variant: 'fullWidth',
  },
  {
    body: 'Tell us about your experience of using this service.',
    title: 'Help us improve',
    variant: 'fullWidthSubtle',
  },
] as const

export function Examples() {
  return actionBannerVariants.map(({ body, title, variant }) => (
    <Example fullWidth={variant.startsWith('fullWidth')} key={variant} title={`${variant} variant`}>
      <ActionBanner
        cta={
          <Button
            to="/portal/components/action-banner"
            variant={variant === 'subtle' || variant === 'fullWidthSubtle' ? 'primary' : 'inverse'}
          >
            Find out more
          </Button>
        }
        headingLevel={3}
        image={variant.startsWith('fullWidth') ? illustration : undefined}
        title={title}
        variant={variant}
      >
        <p>{body}</p>
      </ActionBanner>
    </Example>
  ))
}
