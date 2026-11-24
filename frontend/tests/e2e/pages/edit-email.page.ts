import { expect, type Locator, type Page } from '@playwright/test'

import { buildUpdateUserDetailsHref } from '../../../src/app/portal/user/me/edit-details/[detail]/navigation'
import { buildMyDetailsHref } from '../../../src/app/portal/user/me/navigation'

export class EditEmailPage {
  readonly heading: Locator
  readonly emailInput: Locator
  readonly submitButton: Locator
  readonly backLink: Locator
  readonly formError: Locator

  readonly pendingAlert: {
    heading: Locator
    targetEmail: Locator
    targetEmailNotVerified: Locator
    resend: Locator
    cancel: Locator
    resentStatus: Locator
  }

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', { name: 'Edit work email address' })
    this.emailInput = page.getByLabel('Work email address')
    this.submitButton = page.getByRole('button', { name: 'Submit' })
    this.backLink = page.getByRole('link', { name: 'Back', exact: true })
    this.formError = page.locator('form').filter({ has: this.emailInput }).getByRole('alert')

    this.pendingAlert = {
      heading: page.getByRole('heading', { name: 'Email update pending' }),
      targetEmail: page.getByTestId('target-email'),
      targetEmailNotVerified: page.getByTestId('target-email-not-verified'),
      resend: page.getByRole('button', { name: 'Resend email' }),
      cancel: page.getByRole('button', { name: 'Cancel email change' }),
      resentStatus: page.getByRole('status').filter({ hasText: 'Verification email sent' }),
    }
  }

  async goto() {
    await this.page.goto(buildUpdateUserDetailsHref({ detail: 'email' }))
    await this.expectLoaded()
  }

  async expectLoaded() {
    await expect(this.heading).toBeVisible()
  }

  async submit(newEmail: string) {
    await this.emailInput.fill(newEmail)
    await this.submitButton.click()
  }

  async expectEmailError(message: string) {
    await expect(this.emailInput).toHaveAttribute('aria-invalid', 'true')
    await expect(this.emailInput).toHaveAccessibleDescription(`Error: ${message}`)
  }

  async expectPendingUnverified(newEmail: string) {
    await expect(this.pendingAlert.heading).toBeVisible()
    await expect(this.pendingAlert.targetEmail).toHaveText(newEmail)
    await expect(this.pendingAlert.targetEmailNotVerified).toHaveText(newEmail)
    await expect(this.pendingAlert.resend).toBeVisible()
  }

  async expectPendingVerified(newEmail: string) {
    await expect(this.pendingAlert.heading).toBeVisible()
    await expect(this.pendingAlert.targetEmail).toHaveText(newEmail)
    await expect(this.pendingAlert.targetEmailNotVerified).toBeHidden()
    await expect(this.pendingAlert.resend).toBeHidden()
  }

  async expectNoPendingChange() {
    await expect(this.pendingAlert.heading).toBeHidden()
  }

  async resendVerificationEmail() {
    await this.pendingAlert.resend.click()
    await expect(this.page).toHaveURL(
      buildUpdateUserDetailsHref({ detail: 'email', emailLinkResent: true }),
    )
    await expect(this.pendingAlert.resentStatus).toBeVisible()
  }

  async cancelPendingEmailChange() {
    await this.pendingAlert.cancel.click()
    await expect(this.page).toHaveURL(buildMyDetailsHref())
  }
}
