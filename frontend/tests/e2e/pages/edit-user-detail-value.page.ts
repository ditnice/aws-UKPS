import { expect, type Locator, type Page } from '@playwright/test'

import type { UserDetail } from '../../../src/app/portal/organisations/[id]/users/[userId]/details/edit-details/[detail]/navigation'

export type UserDetailValue = Exclude<UserDetail, 'email'>

const labels: Record<UserDetailValue, string> = {
  fullName: 'Full name',
  contactNumber: 'Contact number',
}

/** The edit page for a user's full name or contact number, under
 * /portal/organisations/{id}/users/{userId}/details/edit-details/{detail}. */
export class EditUserDetailValuePage {
  readonly heading: Locator
  readonly input: Locator
  readonly save: Locator
  readonly backLink: Locator

  constructor(
    private readonly page: Page,
    readonly detail: UserDetailValue,
  ) {
    const label = labels[detail]
    // The heading lower-cases the label, e.g. "Edit full name".
    this.heading = page.getByRole('heading', { name: `Edit ${label}`, level: 1 })
    this.input = page.getByLabel(label, { exact: true })
    this.save = page.getByRole('button', { name: 'Save', exact: true })
    this.backLink = page.getByRole('link', { name: 'Back', exact: true })
  }

  async expectLoaded() {
    await expect(this.page).toHaveURL(
      new RegExp(`/portal/organisations/\\d+/users/\\d+/details/edit-details/${this.detail}\\?`),
    )
    await expect(this.heading).toBeVisible()
  }

  async fill(value: string) {
    // Hydration can reset an input filled against the server-rendered markup, so refill until
    // the value sticks.
    await expect(async () => {
      await this.input.fill(value)
      await expect(this.input).toHaveValue(value, { timeout: 1_000 })
    }).toPass()
  }

  /** Fills and saves the form. On success the app redirects to the page that linked here. */
  async saveValue(value: string) {
    await this.fill(value)
    await this.save.click()
  }

  async expectInvalid(message: string) {
    await expect(this.input).toHaveAttribute('aria-invalid', 'true')
    await expect(this.page.getByText(message)).toBeVisible()
  }
}
