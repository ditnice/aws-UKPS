# ADR-005: Frontend testing strategy

**Status:** Proposed

## Context

The frontend is a Next.js App Router application using React and Payload.
It contains synchronous and asynchronous Server Components, Client
Components, Server Actions, Payload access rules and generated clients for
the backend API. These categories execute in different environments and
carry different risks, so they cannot all be tested meaningfully at one
test layer.

Frontend testing must provide confidence in:

- business, validation and authorisation rules;
- parsing, querying and response mapping;
- user-visible component behaviour and accessibility;
- mutation boundaries such as Server Actions;
- integration with Payload and the backend API; and
- Next.js runtime behaviour, including routing, authentication, redirects,
  hydration, streaming and asynchronous Server Components.

Vitest can exercise pure modules and synchronous React components quickly,
but jsdom is not a browser and Vitest does not provide the full Next.js
runtime. In particular, Next.js does not support async Server Component
rendering with Vitest and recommends end-to-end testing for those
components. Conversely, browser tests are slower and are not a suitable
place to exhaustively test permutations of pure business logic.

The testing strategy therefore needs to assign responsibility to each test
layer, define how Next.js-specific boundaries are handled, and use coverage
without encouraging tests that exist only to increase a percentage.

## Decision

### Test layers

Each behaviour is tested at the lowest layer that can exercise it with
meaningful confidence. The layers have the following responsibilities:

| Layer                                                        | Responsibility                                                                                                                                 |
| ------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| TypeScript, ESLint, generated-client checks and `next build` | Type correctness, lint rules, generated API compatibility, server/client boundary violations and production compilation                        |
| Vitest in a Node environment                                 | Pure functions, parsing, mapping, validation, Payload access functions and Server Action orchestration                                         |
| Vitest, React Testing Library and jsdom                      | Synchronous Server Components and observable Client Component behaviour                                                                        |
| Vitest integration tests with Payload                        | Collection configuration, hooks, access integration and behaviour that requires a real Payload runtime                                         |
| Playwright                                                   | Async Server Components, routing, authentication, redirects, hydration, streaming, browser behaviour, accessibility and critical user journeys |

Tests do not duplicate the same confidence at every layer. Unit tests cover
edge cases and input permutations; Playwright covers a smaller number of
representative journeys through the deployed application boundary.

### Testable application structure

Route files remain thin. Parsing `params` and `searchParams`, constructing
queries, validating input, mapping responses and making business decisions
are placed in co-located modules and unit tested independently. A
`page.tsx`, `layout.tsx` or `route.ts` file should primarily connect the
Next.js runtime to those modules and to data-fetching boundaries.

Code is not extracted merely to expose private implementation details. An
extraction should create a meaningful unit with a stable responsibility.

### Server Components and route behaviour

Synchronous Server Components may be rendered with React Testing Library.
Async Server Components are covered through Playwright because Vitest does
not reproduce their Next.js rendering semantics. This includes nested async
components, Suspense streaming, layouts, metadata and framework handling of
`notFound()` and `redirect()`.

Directly awaiting an async component may be used as a narrow test of its
returned synchronous React tree when this is straightforward, but it is not
required and does not count as coverage of Next.js runtime behaviour. An
async component is not exported or restructured solely to enable this
technique. Branch-heavy logic is instead moved into testable functions or
synchronous presentation components, while representative route outcomes
are exercised with Playwright.

### Server Actions

Server Actions are treated as externally invokable mutation boundaries.
They do not rely on a page-level check or on the UI preventing an invalid
call. Each action validates untrusted input and ensures that authentication
and authorisation are enforced either within the action, within a
server-only data-access layer, or by the authoritative backend endpoint it
invokes.

Server Actions are unit tested as async functions with their external
boundaries mocked. Tests cover:

- validation and rejection of invalid input before mutation;
- use of the authenticated server client;
- the path and minimal request body sent to the backend;
- successful and unsuccessful backend responses;
- safe, user-facing return values that do not expose backend details; and
- cache revalidation or navigation occurring only after success.

Where the backend is the authorisation boundary, its contract and
integration tests own the resource-level authorisation decision. The
frontend test verifies that the action cannot bypass that boundary and
correctly handles its response.

### Component tests

Component tests describe behaviour through the interface available to the
user. Testing Library queries follow its recommended priority, preferring
roles and accessible names, then labels and visible text. Test IDs are used
only when no meaningful accessible query exists.

Interactions use an instance returned by `userEvent.setup()` and await the
interaction. `fireEvent` is reserved for low-level events or browser
conditions that `user-event` cannot model, such as resize, scroll or a
specific native event. Assertions use `@testing-library/jest-dom` matchers
to express visibility, value, selection, validity and enabled state.

Tests assert observable output and calls to genuine application boundaries,
not hook calls, internal state or component implementation. Asynchronous UI
updates use `findBy*` queries or `waitFor`; arbitrary delays and manual
`act()` calls are not used to hide scheduling problems.

Components derive values from props and state during rendering where
possible. Logic caused by a user interaction belongs in its event handler;
effects are reserved for synchronisation with systems outside React. This
follows React's guidance and reduces cascading renders and timing-dependent
tests.

### Mocking and isolation

Mocks replace external I/O, nondeterministic dependencies and framework
boundaries that the selected test environment cannot provide. Typical
examples include the generated API SDK, authenticated server-client
creation, Next.js navigation, headers, cookies and cache operations.

Internal child components and ordinary React components remain real by
default. They are mocked only when they perform unrelated I/O, are async
Server Components that jsdom cannot render, or would prevent the test from
exercising its stated responsibility. Framework components such as
`next/link` and `next/image` are not mocked automatically when their real
behaviour works in the test environment.

Reusable framework mocks and render helpers live in `src/test-utils/`.
Global setup performs common DOM cleanup and mock isolation. Shared mutable
state, stubbed globals, environment variables and mock implementations are
reset between tests as well as mock call histories. A test must pass when
run alone and must not depend on execution order.

### Test data

Fixtures are deterministic, minimal and type checked against the generated
API types. Tests use explicit values for every field relevant to the
behaviour under test. Shared fixture builders may wrap generated faker
factories, but random values are not allowed to influence assertions or
branches; any randomness used is seeded.

This keeps tests readable while ensuring that fixtures fail to compile when
the API contract changes.

### Payload access and configuration

Every decision returned by a Payload access function is unit tested. Access
functions are small security-sensitive units and must have 100% line and
branch coverage. Tests verify the exact constraint returned to Payload, not
only its truthiness.

Unit tests of an access function do not prove that Payload applies it to the
correct collection or that the resulting query excludes protected content.
Integration tests cover security-significant collection wiring, hooks and
representative authenticated and anonymous requests using a real Payload
runtime.

### Snapshots

Snapshots are limited to small, stable presentational output where the full
markup is itself the intended contract. A snapshot is never the only
assertion for business logic, branching, interaction, accessibility or a
complete page. Prefer focused assertions that explain which behaviour is
important.

Snapshot changes are reviewed as behavioural changes and are not updated
blindly.

### Test organisation and naming

To align with NICE's other services, every test filename ends in `.test.ts`
or `.test.tsx`. Unit and component tests are co-located with their source as
`*.test.ts` or `*.test.tsx`. Integration tests use `*.int.test.ts`, and
end-to-end tests use `*.e2e.test.ts`; additional qualifiers, such as
`*.authenticated.e2e.test.ts`, appear before the `.test.ts` suffix.
Integration and end-to-end tests live in their respective test directories.
Test names state the observable outcome and, where relevant, its condition.
Tests use clear arrange, act and assert phases and cover one coherent
behaviour.

Pure and server-side unit tests use a Node environment. DOM component tests
use jsdom. A test does not use jsdom merely because its source belongs to
the frontend.

### Coverage and CI

Coverage is a diagnostic and non-regression mechanism, not a substitute for
reviewing test quality. Coverage includes unimported application files so
that missing tests remain visible.

Generated code, type-only files, migrations, declarative framework
configuration and framework glue containing no application decision may be
excluded. Code is not excluded merely because it is difficult to test.

Global statement, branch, function and line thresholds are set to the
measured baseline when this ADR is adopted. They form a ratchet: thresholds
are raised as meaningful coverage improves and are not lowered to make a
build pass. Risk-based thresholds also apply:

- `src/access/**` has 100% lines and branches per file; and
- pure business logic and Server Actions under `src/lib/**`, `**/_lib/**`
  and `**/_actions/**` have at least 90% lines and branches across each
  configured scope.

Threshold configuration states explicitly whether a glob is aggregate or
per-file; it does not claim to enforce changed-file coverage unless CI has a
separate diff-coverage check.

CI runs static checks, unit tests, integration tests and coverage before the
relevant Playwright suites. Critical Playwright journeys run against a
production build. Local Playwright runs may use the development server.
Cross-browser smoke tests cover the supported browser engines, while
expensive authenticated permutations may use the primary supported browser.
Automated axe checks complement, but do not replace, semantic component
tests and manual accessibility review.

## Consequences

- Fast tests concentrate on business decisions, validation, access rules
  and mutation orchestration, while a smaller browser suite verifies the
  framework and deployment boundary.
- Async Server Component files may have lower unit coverage than pure
  modules. Their confidence comes from extracted logic tests and Playwright,
  not from forcing them through an unsupported renderer.
- Route and component design will favour small, meaningful modules and
  explicit external boundaries.
- Server Actions require security-focused tests even when they are only
  called from trusted-looking UI.
- Role- and label-based component tests provide an additional accessibility
  signal, but they do not replace browser accessibility testing.
- Payload access needs both exhaustive function tests and selected runtime
  integration tests.
- Coverage thresholds can fail a build even when all tests pass. Such a
  failure requires adding meaningful tests or correcting an intentional
  exclusion, not lowering the threshold.
- Existing tests are migrated to this strategy when their behaviour is
  changed or when the relevant risk area is prioritised; adopting the ADR
  does not require a single wholesale rewrite.

## Alternatives considered

### Test every route with Vitest

Rejected because Vitest does not implement the full Next.js runtime or
support async Server Component rendering. Direct invocation can give false
confidence about routing, streaming, caching and framework error handling.

### Test all behaviour through Playwright

Rejected because browser tests are slower, more expensive to diagnose and
poorly suited to exhaustive input and branch permutations.

### Mock every child component

Rejected because it couples tests to component structure and can allow
broken compositions or inaccessible markup to pass.

### Use snapshots as the default component assertion

Rejected because large snapshots obscure intent, are costly to review and
do not clearly describe user-visible behaviour.

### Use a single coverage percentage as the quality target

Rejected because equal coverage percentages do not imply equal confidence.
Risk-based expectations and review of uncovered decisions are more useful
than maximising one aggregate number.

## References

- [Next.js: Vitest](https://nextjs.org/docs/app/guides/testing/vitest)
- [Next.js: Playwright](https://nextjs.org/docs/app/guides/testing/playwright)
- [Next.js: Data security](https://nextjs.org/docs/app/guides/data-security)
- [Testing Library: About queries](https://testing-library.com/docs/queries/about)
- [Testing Library: User interactions](https://testing-library.com/docs/user-event/intro)
- [React: You Might Not Need an Effect](https://react.dev/learn/you-might-not-need-an-effect)
- [Vitest: Coverage configuration](https://vitest.dev/config/coverage.html)
