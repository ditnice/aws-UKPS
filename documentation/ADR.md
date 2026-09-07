# Architecture Decision Records

## ADR-001: Stale setup token after a sign-up link resend

**Status:** Accepted

### Context

`/auth/sign-up/initiate?setupToken=...` lets a user with an expired setup
link request a new one (`POST /auth/resend-setup-token`). On success, the
backend rotates the token: the existing `UserOnboardingRecord` row is
deleted and a new one is inserted with a fresh `Guid.CreateVersion7()`
token (`IdentityAdministrationService.ReplaceOnboardingRecord`), and the new
link is emailed to the user's registered address. The old token is no
longer valid for anything from this point on.

This means the setup token embedded in the page the user is currently
looking at becomes stale the moment they click "Send a new link". If they
click the button again from the same tab — without first opening the new
email and following the new link — the request is made with the old,
now-deleted token, and `POST /auth/resend-setup-token` returns `404 Not
Found` (`ResendSetupTokenError.DoesNotExist`).

In practice this is a plausible path, not just a double-click edge case:
the new email can land in spam/junk, or simply take a few minutes to
arrive, and a user who doesn't see it appear will often just hit the
button on the page they still have open rather than go looking for it.

We considered fixing this by returning the new setup token itself in the
`200` response body, so the frontend could transparently retry with it, but
immediately realised this was a security issue and defeats the purpose of using
email as MFA, since the user could in theory find the returned token in dev tools.


### Decision

For now, we accept the rough edge: a same-tab resend against a
since-rotated token fails, and `RequestNewLink.tsx` shows a dedicated
`notFound` state telling the user to check their email (including
spam/junk) for the most recent link, rather than a generic error. The
resend-attempt cap (`EmailOptions.MaxResendSignUpLinkAttempts`, currently
3) is still enforced correctly regardless of this, since it's tracked
server-side on `UserOnboardingRecord.ResendCount` and carried forward
across rotations.

### Consequences

- A user who resends more than once without reopening their inbox has to
  leave the page and follow the email to make further progress. This is a
  usability wrinkle, not a dead end — the 404 state gives clear next steps
  — but it is inconsistent with the rest of the page, where "click the
  button again" is the expected recovery action.
- No change was needed to the backend or to `/auth/sign-up/initiate/page.tsx`;
  the handling is scoped entirely to `RequestNewLink.tsx`.

### Future consideration

Instead of returning the new *token*, the resend endpoint could return the
new `UserOnboardingRecord`'s row id (a value with no standalone power to
authenticate — unlike the token, it can't be used to reach
`validate-setup-token` or `setup-user`). The frontend would hold onto that
id and send it as a request parameter on the *next* resend click, letting
the backend correlate "this browser tab" with the row it should rotate
next, without the tab ever needing to hold a currently-valid setup token.
This would let the same-tab-resend case keep working seamlessly, while
preserving the property that only the emailed link itself can advance the
user through `validate-setup-token` / `setup-user`. Not implemented here;
flagged for whoever picks this up next.
