import type { Locator, Page } from '@playwright/test'

export class SignInPage {
  readonly email: Locator
  readonly password: Locator
  readonly submit: Locator

  constructor(private readonly page: Page) {
    this.email = page.getByLabel('Email address')
    this.password = page.getByLabel('Password', { exact: true })
    this.submit = page.getByRole('button', { name: 'Continue' })
  }

  async goto(returnTo?: string) {
    const query = returnTo ? `?${new URLSearchParams({ returnTo }).toString()}` : ''
    await this.page.goto(`/auth/sign-in${query}`)
  }

  async signIn(email: string, password: string) {
    await this.email.fill(email)
    await this.password.fill(password)
    await this.submit.click()
  }
}
