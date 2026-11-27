import { expect, type Locator, type Page } from '@playwright/test'

import { buildMyDetailsHref, type TypedParams } from '../../../src/app/portal/user/me/navigation'

import { EditUserDetailValuePage, type UserDetailValue } from './edit-user-detail-value.page'
import { EditUserEmailPage } from './edit-user-email.page'

import type { UserDetail } from '../../../src/app/portal/organisations/[id]/users/[userId]/details/edit-details/[detail]/navigation'

export type PersonalDetailLabel = 'Organisation' | 'Full name' | 'Email address' | 'Contact Number'
export type SignInMethodLabel =
  'Password' | 'Primary authentication method' | 'Backup authentication method'

export class MePage {
  readonly heading: Locator
  readonly signInMethodHeading: Locator
  readonly personalDetailsHeading: Locator
  readonly detailsUpdatedAlert: Locator
  readonly emailUpdateRequestedAlert: Locator
  readonly mfaUpdatedAlert: Locator
  readonly pendingEmailTag: Locator

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', { name: 'Manage details', level: 1 })
    this.signInMethodHeading = page.getByRole('heading', { name: 'Sign in method' })
    this.personalDetailsHeading = page.getByRole('heading', { name: 'Personal details' })
    this.detailsUpdatedAlert = page.getByRole('heading', {
      name: 'Details changed successfully',
    })
    this.emailUpdateRequestedAlert = page.getByRole('heading', { name: 'Email update requested' })
    this.mfaUpdatedAlert = page.getByRole('heading', { name: 'Two-step verification updated' })
    this.pendingEmailTag = this.value('Email address').getByText('Update Pending', { exact: true })
  }

  async goto(params?: TypedParams) {
    await this.page.goto(buildMyDetailsHref(params))
    await this.expectLoaded()
  }

  /** For when an action returns to the page rather than the test navigating there. */
  async expectLoaded() {
    await expect(this.heading).toBeVisible()
    await expect(this.personalDetailsHeading).toBeVisible()
  }

  /** A summary list row is a dt/dd pair in a div, which has no ARIA role, so this finds the
   * div holding the labelled term. Labels are unique across both lists on the page. */
  row(label: PersonalDetailLabel | SignInMethodLabel): Locator {
    return this.page.locator('dl > div').filter({
      has: this.page.getByRole('term').getByText(label, { exact: true }),
    })
  }

  /** The row's value cell; the actions cell, when there is one, is the second definition. */
  value(label: PersonalDetailLabel | SignInMethodLabel): Locator {
    return this.row(label).getByRole('definition').first()
  }

  async expectValue(label: PersonalDetailLabel | SignInMethodLabel, value: string | RegExp) {
    await expect(this.value(label)).toContainText(value)
  }

  /** The personal details "Change" links share a name, so this picks one by where it goes.
   * Their hrefs carry the user's and organisation's ids, so tests don't need to know them. */
  changeDetailLink(detail: UserDetail): Locator {
    return this.page.locator(`a[href*="/details/edit-details/${detail}?"]`)
  }

  /** Opens the full name or contact number edit page. Saving there returns here. */
  async changeDetail(detail: UserDetailValue): Promise<EditUserDetailValuePage> {
    await this.changeDetailLink(detail).click()
    const editPage = new EditUserDetailValuePage(this.page, detail)
    await editPage.expectLoaded()
    return editPage
  }

  /** Opens the work email address edit page. Submitting there returns here. */
  async changeEmail(): Promise<EditUserEmailPage> {
    await this.changeDetailLink('email').click()
    const editPage = new EditUserEmailPage(this.page)
    await editPage.expectLoaded()
    return editPage
  }

  /** The sign in method actions have visually hidden text, so each has its own name. */
  get changePasswordLink(): Locator {
    return this.page.getByRole('link', { name: 'Change password', exact: true })
  }

  get changePrimaryMethodLink(): Locator {
    return this.page.getByRole('link', { name: 'Change update-primary-method', exact: true })
  }

  get changeBackupMethodLink(): Locator {
    return this.page.getByRole('link', { name: 'Change update-backup-method', exact: true })
  }

  get sendPrimaryRecoveryLink(): Locator {
    return this.page.getByRole('link', {
      name: 'Send recovery link primary-recovery-link',
      exact: true,
    })
  }

  get sendBackupRecoveryLink(): Locator {
    return this.page.getByRole('link', {
      name: 'Send recovery link backup-recovery-link',
      exact: true,
    })
  }
}
