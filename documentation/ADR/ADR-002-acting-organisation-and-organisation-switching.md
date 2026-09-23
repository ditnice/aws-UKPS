# ADR-002: Acting organisation and organisation switching

**Status:** Accepted

## Context

Some users will be allowed access to more than one organisation, for
example a consultant working for several pharmaceutical companies. The
data each organisation holds in UKPS is commercially sensitive to that
organisation, so we do not want a multi-organisation user to see records
from two organisations side by side, or to act on one organisation's data
while believing they are working in another.


Organisation-scoped routes exist or are planned in the form
`/organisation/{id}/records` (manage an organisation's records) and
`/organisation/{id}` (manage its details and users). The new records
page for organisation users will be per organisation.

For the switching interaction we looked at the MoJ Design System
organisation switcher and the DfE "Publish teacher training courses"
implementation of the same idea:

- https://design-patterns.service.justice.gov.uk/components/organisation-switcher/
- https://becoming-a-teacher.design-history.education.gov.uk/publish-teacher-training-courses/adding-primary-navigation-to-the-service/primary-navigation--multiple-organisation-user-accredited-body.png

Both place a banner between the site header and the primary navigation
naming the current organisation, with a "Change organisation" link to a
page that lists the user's organisations. Everything below the banner
belongs to the named organisation.

## Decision

1. **Organisation-scoped routes keep the organisation id in the URL.**
   `/organisation/{id}/records` and `/organisation/{id}` are the routes
   for managing an organisation's records, details and users.

2. **Super users and admins reach those routes directly.** They use the
   same organisation-scoped routes as organisation users, with none of the
   acting-organisation scoping described below applied to them.

3. **A multi-organisation user has one acting organisation at a time, and
   must change it deliberately.** A user with access to organisations 1
   and 2 who is currently operating organisation 1 must switch to
   organisation 2 before they can reach `/organisation/2/records` or any
   other organisation 2 route.

4. **Direct navigation to another organisation's route is interrupted by a
   confirmation screen.** If the user navigates to `/organisation/2/...`
   while acting as organisation 1, they are shown a page along the lines
   of "You are currently operating as organisation 1. Confirm you are now
   operating as organisation 2" and must confirm before the organisation 2
   page is rendered. Confirming performs the switch.

5. **Switching follows the MoJ organisation switcher pattern.** We will
   build the banner (current organisation name plus a "Change organisation"
   link), the change-organisation page (a dropdown of the user's
   organisations and a submit button), and the `/me` endpoint will surface
   the user's organisations and current acting organisation so the
   frontend can render both.

6. **The acting organisation is stored in the database, not in a cookie.**
   A nullable attribute such as `ActingOrganisation` is held against the
   user entity. This enforces the same acting organisation across browsers,
   devices and sessions, and lets the backend validate it on every request
   alongside the membership lookup it already performs. The existing
   `selected_organisation` cookie handling is superseded by this.

7. **Non-organisation-scoped views are separate views and endpoints.**
   Horizon scanners, super users and admins get their own pages for
   cross-organisation tables and searches, for example `/admin/users`,
   `/scanners/records` or simply `/records`. These routes are illustrative
   only and are not being formalised here; the decision is that they are
   different views backed by different endpoints from the
   organisation-scoped ones.

## Consequences

- The backend's per-request organisation scope stays as the mechanism for
  isolation. `OrganisationAuthoriser` and every service that calls
  `CanPerformOperationOnOrganisation` continue to work unchanged; only the
  source of the acting organisation moves from the cookie to the user
  entity, and super users bypass it via the user-level attribute.
- A multi-organisation user can never see two organisations' data on one
  page, and cannot land on another organisation's page by following a
  bookmark or a shared link without an explicit confirmation step.
- Because the acting organisation lives in the database, a switch made in
  one tab or device applies to every other open tab or device on its next
  request. The banner on every organisation-scoped page is what tells the
  user which organisation they are in, so it must be present on all of
  them.
- The `/me` endpoint becomes the single source the portal layout needs to
  decide whether to show the banner and what to list on the
  change-organisation page.
- New work required: the `ActingOrganisation` attribute and migration,
  removal of the cookie path in `TokenValidationHandler`, an endpoint to
  set the acting organisation, extension of `/me`, the banner component,
  the change-organisation page, the confirmation interstitial for direct
  navigation, and the Confluence authentication page update.
- Cross-organisation views for central NICE users are built separately
  and are not constrained by the acting-organisation model.

## Future consideration

If the acting organisation resolves to a membership that has since been
deactivated, the user should be sent to the change-organisation page
rather than to sign-in. The current handler fails authentication outright
in the equivalent case, which would produce a sign-in loop; the
implementation should treat "no valid acting organisation" as
"authenticated, needs to choose" rather than as an authentication
failure.
