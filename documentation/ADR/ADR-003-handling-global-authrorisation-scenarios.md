# ADR-003: Handling organisation context authentication failures

**Status:** Accepted

## Context

Users can be authenticated successfully but still be unable to access a particular piece of content because their current organisation context does not allow the requested operation.

There are several cases where this can occur:

* **The user has access to the organisation, but it is not their current organisation.** For example, the user is currently operating as organisation 456 but requests `/organisations/123/users`, and they have a valid membership for organisation 123.
* **The user has multiple organisation memberships but has not selected a current organisation.** The user is authenticated and has organisations they can operate as, but there is no current organisation against which an organisation-scoped request can be authorised.
* **The user's membership for the requested organisation has been deactivated.** The user may previously have been authorised to access the organisation, but their membership is no longer active.

These cases are distinct from an unauthenticated user. The user has successfully authenticated, but additional organisation context is required before the requested operation can proceed.

When a user logs into the application, they will always be prompted to select the organisation they wish to operate as when they have access to more than one organisation. This establishes the user's current organisation for the session/application before they begin using organisation-scoped functionality. The exception to this is super users who will not be prompted to select an organisation.

The same checks are required across many organisation-scoped data calls. Handling each case independently in individual controllers, pages or services would duplicate logic and could result in inconsistent behaviour between endpoints.

The acting organisation is established by the mechanism described in ADR-002. The backend already has the information required to determine the user's organisation memberships and current acting organisation as part of request authentication and authorisation.

## Decision

1. **Organisation context failures are handled generically by the authentication pipeline.**

   The authentication middleware/handler will determine whether the request cannot proceed because of an organisation context issue. This check will be performed as part of the existing authentication processing rather than being implemented separately by each data endpoint.

2. **Users with multiple organisations are prompted to select their current organisation when they log in.**

   Selecting an organisation is part of the normal login flow for users with access to more than one organisation. The user must explicitly select the organisation they wish to operate as before accessing organisation-scoped functionality.

   The selected organisation becomes the user's current organisation as described in ADR-002.

   This does not remove the need for the backend organisation-context checks. The current organisation can subsequently become invalid, be changed, or no longer correspond to the organisation being requested.

3. **Organisation context failures return HTTP 401 with a machine-readable error code.**

   When one of the recognised organisation context failures occurs, the backend returns an HTTP 401 response containing an appropriate error code.

   The initial codes are:

   * `WrongCurrentOrganisation` — the user has access to the requested organisation, but it is not their current organisation.
   * `CurrentOrganisationRequired` — the user has multiple organisation memberships but no current organisation has been selected.
   * `MembershipDeactivated` — the user's membership for the requested organisation has been deactivated.

4. **The client handles these authentication errors centrally.**

   The portal's API/client wrapper will inspect authentication errors returned by the backend and, where a recognised error code is present, redirect the user to the appropriate page.

   Individual data-fetching components and pages therefore do not need to implement their own checks for these conditions.

5. **Each error code has a corresponding user-facing flow.**

   * `WrongCurrentOrganisation` → a confirmation page explaining that the user is currently operating as another organisation and providing an action to change the current organisation.
   * `CurrentOrganisationRequired` → a page allowing the user to select their current organisation.
   * `MembershipDeactivated` → a page explaining that their membership for the requested organisation has been deactivated.

6. **The original destination is retained where appropriate.**

   Where the user is redirected because of an organisation context error, the client should retain the originally requested route so that, after the user resolves the issue, they can be returned to the content they originally requested.

   For example:

   ```text
   /organisations/123/users
       ↓
   401 WrongCurrentOrganisation
       ↓
   Confirm organisation change
       ↓
   User confirms organisation 123
       ↓
   Current organisation updated
       ↓
   /organisations/123/users
   ```

7. **Changing the current organisation remains an explicit user action.**

   The authentication error does not itself change the user's current organisation. It only identifies that a change is required.

   The user must explicitly confirm the change on the relevant page. The endpoint responsible for changing the current organisation then performs the update described in ADR-002.

8. **These errors are distinct from ordinary authorisation failures.**

   A user who does not have a membership for the requested organisation should not be directed through the organisation-switching flow. They do not have access to that organisation and therefore cannot resolve the request by changing their current organisation.

   Similarly, permissions such as user roles remain an authorisation concern and are not converted into organisation-context authentication errors.

9. **The mechanism applies consistently to organisation-scoped data calls.**

   Controllers and services should not need to duplicate checks for the current organisation, missing current organisation, or deactivated membership. The authentication pipeline provides a consistent response, allowing the same client-side handling to work across the portal.

## Consequences

* Users with access to multiple organisations explicitly choose their current organisation as part of every login flow.
* Organisation context failures have a consistent representation across the API.
* The frontend has a single place to translate backend authentication errors into user-facing navigation.
* Individual API consumers do not need to duplicate logic for determining whether the user needs to change or select their current organisation.
* The backend remains responsible for enforcing organisation isolation; the client-side redirect is only a user-experience mechanism and must not be relied upon for security.
* A user cannot resolve `WrongCurrentOrganisation` simply by manipulating the URL. The backend continues to validate the current organisation on every relevant request.
* A user with no current organisation can be directed to a selection page without being treated as an unauthenticated user.
* A deactivated membership can be communicated explicitly to the user rather than presenting the generic sign-in experience.
* The approach allows additional authentication-context error codes to be introduced later without requiring each data endpoint to implement its own redirect behaviour.
* The client wrapper must preserve enough information about the failed request to return the user to the originally requested page after the required action has completed.
* The implementation must ensure that server-side API calls and client-side navigation handle these responses consistently, particularly where multiple API requests are made as part of rendering a page.

## Future consideration

The distinction between authentication and organisation-context failures may need to be revisited if the API's use of HTTP status codes becomes inconsistent with the semantics of the wider platform.

In particular, `401` is being used here to represent a request that cannot currently be authenticated in the required organisation context, even though the user's identity itself has been successfully authenticated. If the API later adopts a more granular error/status model, these error codes and their corresponding client behaviour should be reviewed together.

The set of organisation context error codes should also remain deliberately small. New codes should only be introduced where the client needs to take a different user-facing action.
