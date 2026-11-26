import { expect, type Locator, type Page } from '@playwright/test'

/** The edit page for a user's work email address, under
 * /portal/organisations/{id}/users/{userId}/details/edit-details/email. */
export class EditUserEmailPage {
  readonly heading: Locator
  readonly input: Locator
  readonly submit: Locator
  readonly backLink: Locator
  readonly pendingEmailAlert: Locator
  readonly pendingEmail: Locator
  readonly resendVerificationEmail: Locator
  readonly cancelEmailChange: Locator
  readonly verificationEmailSent: Locator

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', { name: 'Edit work email address', level: 1 })
    this.input = page.getByLabel('Work email address', { exact: true })
    this.submit = page.getByRole('button', { name: 'Submit', exact: true })
    this.backLink = page.getByRole('link', { name: 'Back', exact: true })
    this.pendingEmailAlert = page.getByRole('heading', { name: 'Email update pending' })
    this.pendingEmail = page.getByTestId('target-email')
    this.resendVerificationEmail = page.getByRole('button', { name: 'Resend email', exact: true })
    this.cancelEmailChange = page.getByRole('button', { name: 'Cancel email change', exact: true })
    this.verificationEmailSent = page
      .getByRole('status')
      .filter({ hasText: 'Verification email sent' })
  }

  async expectLoaded() {
    await expect(this.page).toHaveURL(
      /\/portal\/organisations\/\d+\/users\/\d+\/details\/edit-details\/email\?/,
    )
    await expect(this.heading).toBeVisible()
  }

  async fill(email: string) {
    // Hydration can reset an input filled against the server-rendered markup, so refill until
    // the value sticks.
    await expect(async () => {
      await this.input.fill(email)
      await expect(this.input).toHaveValue(email, { timeout: 1_000 })
    }).toPass()
  }

  /** Fills and submits the form. On success the app redirects to the page that linked here. */
  async submitEmail(email: string) {
    await this.fill(email)
    await this.submit.click()
  }

  async expectInvalid(message: string) {
    await expect(this.input).toHaveAttribute('aria-invalid', 'true')
    await expect(this.page.getByText(message)).toBeVisible()
  }

  /** Only shown while a change of email is waiting to be verified. */
  async expectPendingEmail(email: string) {
    await expect(this.pendingEmailAlert).toBeVisible()
    await expect(this.pendingEmail).toHaveText(email)
  }
}
