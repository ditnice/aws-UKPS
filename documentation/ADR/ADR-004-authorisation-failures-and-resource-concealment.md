# ADR-004: Authorisation failures and resource concealment

**Status:** Accepted

## Context

An authenticated user can be denied access to an endpoint for two
different reasons: they are not allowed to perform the operation, or the
resource they addressed does not exist. Returning `403 Forbidden` for the
first case and `404 Not Found` for the second is an accurate description
of each outcome, but on an endpoint for a specific sensitive resource the
difference also tells the caller whether that resource exists.

For example, if `/users/123` returns `403` while `/users/456` returns
`404`, a caller who may not view either user can infer that user 123
exists. The response has disclosed information even though it has not
returned the resource itself. The same problem applies to records,
membership requests and other identifiers that should not be enumerable
by an unauthorised caller.

Not every forbidden response creates this problem. A collection or
generic endpoint such as `/users` does not identify one particular user,
so a `403` communicates that the caller lacks the required capability
without confirming the existence of a sensitive resource. An endpoint
scoped by a non-sensitive or already-known parent resource can do the
same, provided its response does not vary according to whether an
unauthorised child resource exists.

`401 Unauthorized` has a separate meaning: the request is not
authenticated. This decision only covers responses for authenticated
callers.

## Decision

1. **Resource-specific endpoints conceal sensitive resources with a
   `404`.** If an authenticated caller is not permitted to know whether
   the resource identified by the request exists, both "not found" and
   "not allowed" are returned as `404 Not Found` at the HTTP boundary.

2. **Generic and collection endpoints return `403` when the caller lacks
   the capability.** A request such as `GET /users` may return `403
   Forbidden` because it does not confirm that a particular sensitive
   user exists.

3. **A scoped collection may return `403` if it does not reveal child
   resource existence.** For example, a caller who cannot list an
   organisation's users may receive the same `403` whether or not the
   supplied organisation id exists. Authorisation should be checked
   before the existence lookup where this is needed to keep the response
   indistinguishable.

4. **A caller who is permitted to know that the resource exists receives
   the semantically accurate response.** Once the caller is authorised
   for the relevant scope, a missing resource returns `404`; a forbidden
   operation on a known resource may return `403` when its existence is
   not sensitive to that caller.

5. **The concealment happens at the HTTP boundary.** Application services
   may retain distinct `NotFound` and `NotAllowed` results for logging,
   testing and internal behaviour, but controllers map them to the same
   external `404` response when the resource must be concealed. Response
   bodies must not reintroduce the distinction.

6. **New and changed endpoints document and test both sides of the
   decision.** Tests should cover authorised, unauthorised and missing
   resources, including requests with guessed identifiers, and OpenAPI
   response declarations must match the controller behaviour.

## Consequences

- Callers cannot use status-code differences to enumerate sensitive
  resources that they are not authorised to view.
- Some `404` responses deliberately mean either "does not exist" or "you
  are not allowed to know whether it exists". Client-facing error text
  must remain neutral in these cases.
- `403` remains available for capability failures where no sensitive
  resource existence is disclosed, making generic authorisation failures
  clearer to API clients.
- Endpoint reviews need to consider the sensitivity of the addressed
  resource and what the caller is already entitled to know, rather than
  applying one status code to every authorisation failure.
- Existing resource-specific endpoints should be checked against this
  decision when they are changed. They do not all need to be migrated as
  part of adopting this ADR, but any known discrepancy should be recorded
  or corrected in the work that exposes it.
