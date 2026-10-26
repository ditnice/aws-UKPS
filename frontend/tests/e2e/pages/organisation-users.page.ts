import { expect, type Locator, type Page } from '@playwright/test'

import {
  organisationUserTableHeaders,
  type OrganisationUserTableHeaderKey,
} from '../../../src/app/portal/organisations/[id]/_lib/userLabels'

import type { SortDirection } from '../../../src/client/generated/types.gen'

// ARIA has no way to select a column, so column cells are the one CSS selector.
function columnCell(key: OrganisationUserTableHeaderKey): string {
  const column = organisationUserTableHeaders.findIndex((header) => header.key === key) + 1
  return `td:nth-child(${column})`
}

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
    return this.table.locator(`tbody ${columnCell(key)}`)
  }

  /** The row's cell in one column, found from the app's own column order. */
  cell(row: Locator, key: OrganisationUserTableHeaderKey): Locator {
    return row.locator(columnCell(key))
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

  /** Opens the first listed user with the `action` link, or returns undefined if no user has it. */
  async openUser(
    organisationId: string,
    query: Record<string, string>,
    action: string,
  ): Promise<string | undefined> {
    await this.goto(organisationId, query)
    await expect(this.noUsersMessage.or(this.cells('email').first())).toBeVisible()

    const actionLink = this.page.getByRole('link', { name: action, exact: true })
    const row = this.table.getByRole('row').filter({ has: actionLink }).first()
    if ((await row.count()) === 0) {
      return undefined
    }
    const email = (await this.cell(row, 'email').innerText()).trim()
    await row.getByRole('link', { name: action, exact: true }).click()
    return email
  }

  /* Specs that change a seeded user each claim theirs from opposite ends of the active standard
   * users sorted by email, so specs running in parallel never pick the same user. Organisation 1
   * is seeded with three active standard users. Returns undefined if there is none. */
  async openActiveStandardUser(
    organisationId: string,
    sortDirection: SortDirection,
  ): Promise<string | undefined> {
    const email = await this.openUser(
      organisationId,
      { status: 'Active', role: 'Standard', sortBy: 'Email', sortDirection },
      'Edit',
    )
    if (!email) {
      return undefined
    }

    await expect(this.page.getByRole('heading', { name: "Manage user's access" })).toBeVisible()
    return email
  }

  /** Chooses an action on the manage user's access page and continues. The NDS radio's label
   * covers its input, so this clicks the label as a user would, then checks the radio took it. */
  async chooseAction(label: string) {
    await this.page.getByText(label, { exact: true }).click()
    await expect(this.page.getByRole('radio', { name: label, exact: true })).toBeChecked()
    await this.page.getByRole('button', { name: 'Continue' }).click()
  }

  /** Every confirm page has a Cancel link or button. Some go back in history, so this doesn't
   * wait for a particular page; check the confirm page has gone. */
  async cancel() {
    await this.page
      .getByRole('button', { name: 'Cancel', exact: true })
      .or(this.page.getByRole('link', { name: 'Cancel', exact: true }))
      .click()
  }
}
