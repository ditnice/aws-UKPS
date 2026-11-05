import { expect, type APIRequestContext } from '@playwright/test'

import { missingId } from './pages'
import { isDevAuthMode } from './test-environment'

import type {
  RegisterUserDetailsDto,
  UserInformationDto,
  UserListItemDto,
} from '../../../src/client/generated/types.gen'

export function accessSubjectEmail(variable: string, seededEmail: string): string {
  const email = isDevAuthMode() ? seededEmail : process.env[variable]?.trim()
  if (!email) {
    throw new Error(`Set ${variable} to a pre-provisioned, read-only access-control subject`)
  }
  return email
}

async function listedSubject(verifier: APIRequestContext, organisationId: string, email: string) {
  const query = new URLSearchParams({ organisationId, email })
  const response = await verifier.get(`/backend-api/users?${query}`)
  expect(response.ok(), 'The Super verifier can list the subject’s organisation').toBe(true)
  const { items } = (await response.json()) as { items: UserListItemDto[] }
  const matches = items.filter(({ emailAddress }) => emailAddress === email)
  expect(matches, 'A known subject must exist in the addressed organisation').toHaveLength(1)
  return matches[0]
}

export async function verifiedUser(
  verifier: APIRequestContext,
  organisationId: string,
  email: string,
): Promise<UserInformationDto> {
  const subject = await listedSubject(verifier, organisationId, email)
  if (!subject.userId) throw new Error('The protected subject must be an existing user')
  expect(subject.status).toBe('Active')
  expect(subject.role).toBe('Standard')

  const url = (id: number) => `/backend-api/users/${id}/organisations/${organisationId}`
  const response = await verifier.get(url(subject.userId))
  expect(response.ok(), 'The Super verifier can read the existing user').toBe(true)
  const user = (await response.json()) as UserInformationDto
  expect(user).toMatchObject({ workEmail: email, organisationId: Number(organisationId) })
  expect((await verifier.get(url(missingId))).status(), 'The comparison user is missing').toBe(404)
  return user
}

export async function verifiedRegistrationRequest(
  verifier: APIRequestContext,
  organisationId: string,
  email: string,
): Promise<RegisterUserDetailsDto> {
  const subject = await listedSubject(verifier, organisationId, email)
  if (!subject.registrationRequestId) {
    throw new Error('The protected subject must be an existing registration request')
  }
  expect(subject.status).toBe('RequestedAccess')

  const url = (id: number) =>
    `/backend-api/organisations/${organisationId}/membership-requests/${id}`
  const response = await verifier.get(url(subject.registrationRequestId))
  expect(response.ok(), 'The Super verifier can read the existing request').toBe(true)
  const request = (await response.json()) as RegisterUserDetailsDto
  expect(request.workEmail).toBe(email)
  expect((await verifier.get(url(missingId))).status(), 'The comparison request is missing').toBe(
    404,
  )
  return request
}
