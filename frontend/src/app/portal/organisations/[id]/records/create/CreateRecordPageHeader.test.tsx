import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { router } from '@/test-utils/nextNavigation'

import { CreateRecordPageHeader } from './CreateRecordPageHeader'

vi.mock('next/navigation', () => import('@/test-utils/nextNavigation'))

const exitWarningHeading = 'If you continue, this record will not be saved'

describe('CreateRecordPageHeader', () => {
  it('does not show the exit warning before Back is selected', () => {
    render(<CreateRecordPageHeader />)

    expect(screen.queryByText(exitWarningHeading)).toBeNull()
  })

  it('shows the exit warning without leaving the page when Back is selected', () => {
    render(<CreateRecordPageHeader />)

    fireEvent.click(screen.getByRole('link', { name: 'Back' }))

    expect(screen.getByText(exitWarningHeading)).toBeDefined()
    expect(router.back).not.toHaveBeenCalled()
  })

  it('goes back when Back is selected while the exit warning is showing', () => {
    render(<CreateRecordPageHeader />)
    fireEvent.click(screen.getByRole('link', { name: 'Back' }))

    fireEvent.click(screen.getByRole('link', { name: 'Back' }))

    expect(router.back).toHaveBeenCalledOnce()
  })
})
