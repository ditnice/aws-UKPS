![Alt text](documentation/assets/ukps-logo.png)

## Structure




```
./
├── backend/                - .NET API Layer
│   ├── src/                - API application source code
│   ├── tests/              - Backend test project and test assets
│   ├── .editorconfig       - Backend editor and formatting rules
│   ├── Containerfile       - Backend container build definition
│   ├── README.md           - Backend-specific documentation
│   └── ...                 - Supporting backend files
├── documentation/          - Project documentation assets
│   ├── assets/             - Documentation images and media assets
│   └── ...                 - Additional documentation files
├── frontend/               - Next.js frontend application
│   ├── src/                - Next.js application source code
│   ├── tests/              - Unit tests and test fixtures/mocks
│   ├── .editorconfig       - Frontend editor and formatting rules
│   ├── Containerfile       - Frontend container build definition
│   ├── README.md           - Frontend-specific documentation
│   └── ...                 - Supporting frontend files
├── infrastructure/         - Terraform infrastructure as code
│   ├── environments/       - Environment-specific Terraform configurations
│   ├── modules/            - Reusable Terraform modules
│   ├── .editorconfig       - Infrastructure editor and formatting rules
│   ├── README.md           - Infrastructure-specific documentation
│   └── ...                 - Supporting Terraform files
├── docker-compose.yml      - Local multi-container orchestration file
├── README.md               - Repository overview and structure
└── ...                     - Repository metadata and supporting files
```

## Backend

The backend is an ASP.NET Core Web API built on .NET 10, contained in the `UKPS.Api` project. It includes OpenAPI support via `Microsoft.AspNetCore.OpenApi`, with automatic API schema generation and documentation. Tests are written using xUnit and live in the `UKPS.Api.Tests` project under `tests/`.

For more information, see the [backend README](backend/README.md).

## Frontend

The frontend is a Next.js 16 application with Payload CMS 3 embedded directly via the `/app` directory integration, backed by PostgreSQL. The app is divided into three route groups: `(payload)` for the Payload admin interface, `(public)` for the public-facing site with slug-based routing, and `portal` for the authenticated user portal. Tests use Vitest for integration tests and Playwright for end-to-end tests.

For more information, see the [frontend README](frontend/README.md).

## Infrastructure

Infrastructure is defined in Terraform and targets AWS across five environments: `dev`, `test`, `alpha`, `beta`, and `live`. Each environment directory contains its own backend, variable, and configuration files, keeping per-environment state isolated from shared module logic. Reusable modules live under `modules/` and are composed together within each environment.

For more information, see the [infrastructure README](infrastructure/README.md).
