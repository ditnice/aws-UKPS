import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { Button, ButtonGroup } from '@/components/Button/Button'

import { ActionBanner } from './ActionBanner'

afterEach(cleanup)

describe('ActionBanner', () => {
  it('renders the title, content and call to action', () => {
    const { asFragment } = render(
      <ActionBanner variant="subtle" title="Manage record" cta={<Button>Update record</Button>}>
        Update record information.
      </ActionBanner>,
    )

    expect(screen.getByRole('heading', { name: 'Manage record' })).toBeDefined()
    expect(screen.getByText('Update record information.')).toBeDefined()
    expect(screen.getByRole('button', { name: 'Update record' })).toBeDefined()
    expect(asFragment()).toMatchSnapshot()
  })

  it('applies the banner class alongside a supplied className', () => {
    const { container } = render(
      <ActionBanner className="custom-banner" title="Manage record" cta={<Button>Go</Button>}>
        Content
      </ActionBanner>,
    )

    const banner = container.firstElementChild
    expect(banner?.className).toContain('banner')
    expect(banner?.classList.contains('custom-banner')).toBe(true)
    expect(banner?.classList.contains('action-banner')).toBe(true)
  })

  it('renders a button group as its call to action', () => {
    render(
      <ActionBanner
        title="Manage record"
        cta={
          <ButtonGroup>
            <Button>First</Button>
            <Button>Second</Button>
          </ButtonGroup>
        }
      >
        Content
      </ActionBanner>,
    )

    const group = screen.getByRole('button', { name: 'First' }).parentElement
    expect(group?.dataset.component).toBe('button-group')
    expect(group?.children).toHaveLength(2)
  })
})
