---
name: cover-with-tests
description: After any change to production code in this repo, extend the existing test suites so the new/changed logic is covered. Backend uses xUnit (in backend/InterviewApi.Tests). Frontend uses Jest + React Testing Library (co-located *.test.js files under frontend/src). Use this skill whenever new functionality, endpoints, services, components, or business rules are added or modified. Run both test suites and report which scenarios are covered.
---

# Cover with Tests

Use this skill **after** a feature is implemented in this repo to extend the test suites. The goal is to keep ≥80% coverage of branches and to lock in behavior so future changes can't silently regress.

## Test layout

### Backend (.NET 8, xUnit)
- Project: [backend/InterviewApi.Tests/InterviewApi.Tests.csproj](backend/InterviewApi.Tests/InterviewApi.Tests.csproj)
- Mirror the source tree:
  - `Services/<Service>Tests.cs` for service-layer logic.
  - `Controllers/<Controller>Tests.cs` for controller branches (200/201/400/404).
- Shared helpers live in `TestHelpers.cs` (`StubEnv`, `MakeTempDataDir`).

### Frontend (React, Jest + React Testing Library)
- Tests live next to source as `*.test.js`.
- Use `@testing-library/react`, `@testing-library/user-event`, `@testing-library/jest-dom`.
- `src/setupTests.js` already imports `@testing-library/jest-dom`.

## Conventions

### Backend
1. For services that read JSON, build a temp data directory with `TestHelpers.MakeTempDataDir(...)` and pass a `StubEnv { ContentRootPath = tempDir }`.
2. Cover both happy and unhappy paths:
   - Found vs not-found.
   - Validation: missing/invalid input → 400.
   - Business rules: write a separate `[Theory]` per edge case.
3. For loyalty / date-arithmetic logic, include months with 5 occurrences of a weekday and partial-coverage scenarios.
4. Controllers: instantiate them directly with stubbed `IXxxService` implementations. Do not spin up a full `WebApplicationFactory` unless the test is specifically about middleware.

### Frontend
1. Wrap routed components in `MemoryRouter` (and `Routes`/`Route` when params matter).
2. Mock network with `jest.spyOn(global, 'fetch')` and return `{ ok: true, status: 200, json: async () => data }`.
3. Use `screen.findBy*` for async UI (after fetch).
4. Cover render, user interaction (click, type, select), and the error branch (fetch rejects).
5. Never assert on implementation details — query by role / label / text.

## Running the suites

```
dotnet test backend/InterviewApi.Tests/InterviewApi.Tests.csproj
cd frontend && npm test -- --watchAll=false
```

Both must pass before declaring the skill done.

## Workflow when invoked

1. Identify the files changed in this turn (use `git status` + `git diff`).
2. For each changed file in production code, locate the matching test file (or create one).
3. Add tests for every new branch, returning early when the existing tests already cover it.
4. Run the relevant suite first (backend or frontend), then the other.
5. If a test fails because of a real bug in the new code, flag it and ask before changing production code — this skill's job is to add tests, not to alter behavior.
6. Report:
   - Files added/edited.
   - Scenarios covered (one bullet per scenario).
   - Final test counts ("X passed, 0 failed").

## Out of scope

- Refactoring production code for testability (raise it separately).
- Integration / e2e tests (this skill is unit-level).
- Snapshot tests — prefer explicit assertions.
