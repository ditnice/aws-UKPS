import { expect, type Locator, type Page } from '@playwright/test'

import { buildMyDetailsHref } from '../../../src/app/portal/user/me/navigation'

import { EditEmailPage } from './edit-email.page'

export type PersonalDetail = 'Organisation' | 'Full name' | 'Email address' | 'Contact Number'

const editHeadings = {
  'Full name': 'Edit Full name',
  'Contact Number': 'Edit Contact number',
} as const satisfies Partial<Record<PersonalDetail, string>>

export type EditablePersonalDetail = keyof typeof editHeadings

export class MyDetailsPage {
  readonly heading: Locator
  readonly personalDetails: Locator
  readonly signInMethod: Locator
  readonly detailsChangedAlert: Locator
  readonly emailUpdateRequestedAlert: Locator
  readonly pendingEmailTag: Locator

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', { name: 'Manage details' })
    this.personalDetails = this.summaryListAfter('Personal details')
    this.signInMethod = this.summaryListAfter('Sign in method')
    this.detailsChangedAlert = page.getByRole('heading', { name: 'Details changed successfully' })
    this.emailUpdateRequestedAlert = page.getByRole('heading', { name: 'Email update requested' })
    this.pendingEmailTag = this.row('Email address').getByText('Update Pending')
  }

  async goto() {
    await this.page.goto(buildMyDetailsHref())
    await this.expectLoaded()
  }

  async expectLoaded() {
    await expect(this.heading).toBeVisible()
  }

  row(detail: PersonalDetail): Locator {
    return this.personalDetails
      .locator(':scope > div')
      .filter({ has: this.page.getByRole('term').filter({ hasText: detail }) })
  }

  /** The value cell of a "Personal details" row. */
  value(detail: PersonalDetail): Locator {
    return this.row(detail).getByRole('definition').first()
  }

  async openEdit(detail: EditablePersonalDetail) {
    await this.row(detail).getByRole('link', { name: 'Change', exact: true }).click()
    await expect(this.page.getByRole('heading', { name: editHeadings[detail] })).toBeVisible()
  }

  async openEditEmail(): Promise<EditEmailPage> {
    await this.row('Email address').getByRole('link', { name: 'Change', exact: true }).click()
    const editEmail = new EditEmailPage(this.page)
    await editEmail.expectLoaded()
    return editEmail
  }

  async expectEmailChangeRequested() {
    await expect(this.page).toHaveURL(buildMyDetailsHref({ requestedNewEmail: true }))
    await expect(this.emailUpdateRequestedAlert).toBeVisible()
    await expect(this.pendingEmailTag).toBeVisible()
  }

  private summaryListAfter(heading: string): Locator {
    return this.page
      .getByRole('heading', { name: heading, exact: true })
      .locator('xpath=following-sibling::dl[@data-component="summary-list"][1]')
  }
}
