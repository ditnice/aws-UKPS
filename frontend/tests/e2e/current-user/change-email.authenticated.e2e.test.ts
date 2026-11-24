import { expect, test } from '../fixtures/test'
import { waitForQueuedEmail } from '../helpers/email-queue'
import { allRoles, forRoles } from '../helpers/roles'
import { uniqueEmail } from '../helpers/test-data'
import { mutatesDataTag } from '../helpers/test-environment'
import { MyDetailsPage } from '../pages/my-details.page'
import { SignInPage } from '../pages/sign-in.page'

test.describe('Changing your email address', { tag: forRoles(allRoles, mutatesDataTag) }, () => {
  // If a test fails partway, cancel any pending change so the seeded user is left as it was.
  test.afterEach(async ({ page, baseURL }) => {
    const meResponse = await page.request.get('/backend-api/users/me')
    if (!meResponse.ok()) return
    const me = (await meResponse.json()) as { userId: number; pendingWorkEmail?: unknown }
    if (!me.pendingWorkEmail) return

    // The backend proxy only accepts unsafe requests from the app's own origin.
    const response = await page.request.delete(`/backend-api/users/${me.userId}/email/pending`, {
      headers: { origin: new URL(baseURL!).origin },
    })
    expect(response.ok(), 'Cancelling the pending email change').toBe(true)
  })

  test('requests a new email address, verifies it, then cancels it', async ({ page }) => {
    const newEmail = uniqueEmail('change-email')
    const myDetails = new MyDetailsPage(page)
    const signIn = new SignInPage(page)

    await myDetails.goto()
    await expect(myDetails.pendingEmailTag).toBeHidden()

    const editEmail = await myDetails.openEditEmail()
    await editEmail.expectNoPendingChange()
    await editEmail.submit(newEmail)
    await myDetails.expectEmailChangeRequested()

    const verificationEmail = await waitForQueuedEmail(
      newEmail,
      'EmailChangeVerificationRequestEmail',
    )
    const verificationLink = verificationEmail.Email.EmailVerificationLink
    expect(verificationLink, 'The email has a verification link').toEqual(expect.any(String))

    await myDetails.openEditEmail()
    await editEmail.expectPendingUnverified(newEmail)

    await page.goto(verificationLink as string)
    await expect(page).toHaveURL(/\/auth\/sign-in\?.*emailVerified=true/)
    await expect(signIn.emailVerifiedAlert).toBeVisible()

    await editEmail.goto()
    await editEmail.expectPendingVerified(newEmail)

    await editEmail.cancelPendingEmailChange()
    await myDetails.expectLoaded()
    await expect(myDetails.pendingEmailTag).toBeHidden()
  })
})
