import { expect, type Locator, type Page } from '@playwright/test'

import {
  organisationUserTableHeaders,
  type OrganisationUserTableHeaderKey,
} from '../../../src/app/portal/organisations/[id]/_lib/userLabels'
import { type SortDirection } from '../../../src/client/generated/types.gen'

export class OrganisationUsersPage {
  readonly heading: Locator
  readonly filterPanel: Locator
  readonly table: Locator
  readonly emailFilter: Locator
  readonly applyFilter: Locator
  readonly noUsersMessage: Locator

  constructor(private readonly page: Page) {
    this.heading = page.getByRole('heading', { name: 'Organisation details' })
    // The region stretches to the table's height, so this is the NDS panel inside it.
    this.filterPanel = page
      .getByRole('region', { name: 'Filter results' })
      .locator('[data-component="filter-panel"]')
    this.table = page.getByRole('table', { name: 'Organisation Users' })
    this.emailFilter = page.getByRole('textbox', { name: 'Filter users' })
    this.applyFilter = page.getByRole('button', { name: 'Apply filter', exact: true })
    this.noUsersMessage = page.getByText('No users found for this organisation')
  }

  async goto(organisationId: string, query?: Record<string, string>) {
    const search = query ? `?${new URLSearchParams(query).toString()}` : ''
    await this.page.goto(`/portal/organisations/${organisationId}${search}`)
    await this.expectLoaded()
  }

  /** For when an action returns to the page rather than the test navigating there. */
  async expectLoaded() {
    await expect(this.heading).toBeVisible()
  }

  async filterByEmail(email: string) {
    await this.emailFilter.fill(email)
    await this.applyFilter.click()
  }

  /** The active filters render with the table, so once one shows, the rows are the filtered results. */
  activeFilter(label: string): Locator {
    return this.page
      .getByRole('list', { name: 'Active filters' })
      .getByRole('listitem')
      .filter({ hasText: label })
  }

  /** Every row's cell in one column, found from the app's own column order. */
  cells(key: OrganisationUserTableHeaderKey): Locator {
    const column = organisationUserTableHeaders.findIndex((header) => header.key === key) + 1
    // ARIA has no way to select a column, so this is the one CSS selector.
    return this.table.locator(`tbody td:nth-child(${column})`)
  }

  row(email: string): Locator {
    return this.table.getByRole('row').filter({
      has: this.page.getByRole('cell', { name: email, exact: true }),
    })
  }

  /** Filters to the user and checks their row shows `value`, such as a role or status. */
  async expectUser(email: string, value: string) {
    await this.filterByEmail(email)
    await expect(this.row(email).getByRole('cell', { name: value, exact: true })).toBeVisible()
  }

  async openUserAction(email: string, action: string) {
    await this.filterByEmail(email)
    await this.row(email).getByRole('link', { name: action, exact: true }).click()
  }

  /* Specs that change a seeded user each claim theirs from opposite ends of the active standard
   * users sorted by email, so specs running in parallel never pick the same user. Organisation 1
   * is seeded with three active standard users. */
  async openActiveStandardUser(organisationId: string, sortDirection: SortDirection) {
    await this.goto(organisationId, {
      status: 'Active',
      role: 'Standard',
      sortBy: 'Email',
      sortDirection,
    })

    const row = this.table
      .getByRole('row')
      .filter({ has: this.page.getByRole('link', { name: 'Edit', exact: true }) })
      .first()
    const email = (await row.getByRole('cell').first().innerText()).trim()
    await row.getByRole('link', { name: 'Edit', exact: true }).click()

    await expect(this.page.getByRole('heading', { name: "Manage user's access" })).toBeVisible()
    return email
  }
}
