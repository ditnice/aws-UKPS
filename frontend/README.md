# Payload Blank Template

This template comes configured with the bare minimum to get started on anything you need.

## Quick start

This template can be deployed directly from our Cloud hosting and it will setup MongoDB and cloud S3 object storage for media.

## Quick Start - local setup

To spin up this template locally, follow these steps:

### Clone

After you click the `Deploy` button above, you'll want to have standalone copy of this repo on your machine. If you've already cloned this repo, skip to [Development](#development).

### Development

1. First [clone the repo](#clone) if you have not done so already
2. `cd my-project && cp .env.example .env` to copy the example environment variables.

3. `pnpm install && pnpm dev` to install dependencies and start the dev server
4. open `https://localhost:3000` to open the app in your browser

### Configuration

The following environment variables can be set:

| Variable                 | Required | Example                                                           | Description                                                                                                                                                                                         |
| ------------------------ | -------- | ----------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `BACKEND_API_BASE_URL`   | Yes      | `https://localhost:7180`                                          | Base URL of the backend API. All API requests are sent to this endpoint.                                                                                                                            |
| `BACKEND_API_TIMEOUT_MS` | No       | `60000`                                                           | Maximum time, in milliseconds, to wait for a backend API response before timing out.                                                                                                                |
| `COGNITO_ISSUER`         | Yes*     | `https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_89h3f298h` | AWS Cognito User Pool issuer URL used to validate authentication tokens.                                                                                                                            |
| `COGNITO_CLIENT_ID`      | Yes*     | `ioihsfd49fj09wj3f`                                               | AWS Cognito App Client ID used during authentication and authorization flows.                                                                                                                       |
| `AUTHENTICATION_MODE`    | No       | `DEV`                                                             | Controls how authentication is handled by the application. `DEV` means that you will be allowed to access all routes without authentication and is intended for local development and testing only. |

> \* Unless AUTHENTICATION_MODE=DEV

#### Example Configuration

```env
BACKEND_API_BASE_URL=https://localhost:7180
BACKEND_API_TIMEOUT_MS=60000
COGNITO_ISSUER=https://cognito-idp.eu-west-2.amazonaws.com/eu-west-2_89h3f298h
COGNITO_CLIENT_ID=ioihsfd49fj09wj3f
AUTHENTICATION_MODE=DEV

#### Docker (Optional)

If you prefer to use Docker for local development instead of a local MongoDB instance, the provided docker-compose.yml file can be used.

To do so, follow these steps:

- Modify the `MONGODB_URL` in your `.env` file to `mongodb://127.0.0.1/<dbname>`
- Modify the `docker-compose.yml` file's `MONGODB_URL` to match the above `<dbname>`
- Run `docker-compose up` to start the database, optionally pass `-d` to run in the background.

## How it works

The Payload config is tailored specifically to the needs of most websites. It is pre-configured in the following ways:

### Collections

See the [Collections](https://payloadcms.com/docs/configuration/collections) docs for details on how to extend this functionality.

- #### Users (Authentication)

  Users are auth-enabled collections that have access to the admin panel.

  For additional help, see the official [Auth Example](https://github.com/payloadcms/payload/tree/3.x/examples/auth) or the [Authentication](https://payloadcms.com/docs/authentication/overview#authentication-overview) docs.

- #### Media

  This is the uploads enabled collection. It features pre-configured sizes, focal point and manual resizing to help you manage your pictures.

### Docker

Alternatively, you can use [Docker](https://www.docker.com) to spin up this template locally. To do so, follow these steps:

1. Follow [steps 1 and 2 from above](#development), the docker-compose file will automatically use the `.env` file in your project root
1. Next run `docker-compose up`
1. Follow [steps 4 and 5 from above](#development) to login and create your first admin user

That's it! The Docker instance will help you get up and running quickly while also standardizing the development environment across your teams.

## Questions

If you have any issues or questions, reach out to us on [Discord](https://discord.com/invite/payload) or start a [GitHub discussion](https://github.com/payloadcms/payload/discussions).
```
