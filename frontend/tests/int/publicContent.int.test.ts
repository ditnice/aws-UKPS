import { getPayload } from 'payload'
import { afterAll, beforeAll, describe, expect, it, vi } from 'vitest'

import { getHeaderNav, getPageByPath } from '@/payload/PayloadContent'
import type { Page, User } from '@/payload-types'

import config, { testSchema } from './helpers/payloadTestConfig'

import type { Payload } from 'payload'

// Replace only the runtime config and server-only marker, not Payload or its access checks.
vi.mock('@payload-config', () => import('./helpers/payloadTestConfig'))
vi.mock('server-only', () => ({}))

let payload: Payload | undefined
let user: User
let published: Page
let draft: Page

beforeAll(async () => {
  payload = await getPayload({ config })
  user = await payload.create({
    collection: 'users',
    overrideAccess: true,
    data: { email: 'editor@example.test', password: 'integration-test-password' },
  })

  const createPage = (slug: string, status: 'draft' | 'published', parent?: number) =>
    payload!.create({
      collection: 'pages',
      overrideAccess: true,
      data: {
        title: slug,
        slug,
        parent,
        _status: status,
        layout: [{ blockType: 'textSection', heading: slug, body: `Content for ${slug}` }],
      },
    })

  published = await createPage('public-page', 'published')
  draft = await createPage('private-page', 'draft')
  await createPage('private-child', 'draft', published.id)
  await createPage('public-child', 'published', draft.id)
  await payload.updateGlobal({
    slug: 'header',
    overrideAccess: true,
    user: { ...user, collection: 'users' },
    data: {
      headerLinks: [
        { label: 'Public', destination: published.id },
        { label: 'Private', destination: draft.id },
      ],
    },
  })
}, 60_000)

afterAll(async () => {
  if (!payload) return
  try {
    // testSchema is generated internally from a UUID, not untrusted input.
    await payload.db.pool.query(`DROP SCHEMA "${testSchema}" CASCADE`)
  } finally {
    await payload.destroy()
  }
})

describe('public content access with real Payload', () => {
  it('serves published pages to anonymous readers', async () => {
    expect(await getPageByPath('/public-page')).toMatchObject({ id: published.id })
  })

  it('does not expose draft-only pages to anonymous readers', async () => {
    expect(await getPageByPath('/private-page')).toBeNull()
  })

  it('allows direct anonymous reads of published pages but filters out drafts', async () => {
    const result = await payload!.find({
      collection: 'pages',
      overrideAccess: false,
      where: { id: { in: [published.id, draft.id] } },
    })

    expect(result.docs.map((page) => page.id)).toEqual([published.id])
  })

  it('denies anonymous page creation without creating a document', async () => {
    const slug = 'anonymous-created-page'

    await expect(
      payload!.create({
        collection: 'pages',
        overrideAccess: false,
        data: {
          title: 'Anonymous page',
          slug,
          _status: 'published',
          layout: [{ blockType: 'textSection', heading: 'Anonymous', body: 'Content' }],
        },
      }),
    ).rejects.toMatchObject({ status: 403 })

    const result = await payload!.find({
      collection: 'pages',
      overrideAccess: true,
      where: { slug: { equals: slug } },
    })
    expect(result.docs).toEqual([])
  })

  it('denies anonymous page updates without changing the document', async () => {
    const before = await payload!.findByID({
      collection: 'pages',
      id: published.id,
      overrideAccess: true,
    })

    await expect(
      payload!.update({
        collection: 'pages',
        id: published.id,
        overrideAccess: false,
        data: { title: 'Anonymous update' },
      }),
    ).rejects.toMatchObject({ status: 403 })

    const after = await payload!.findByID({
      collection: 'pages',
      id: published.id,
      overrideAccess: true,
    })
    expect(after).toEqual(before)
  })

  it('denies anonymous page deletion without removing the document', async () => {
    const before = await payload!.findByID({
      collection: 'pages',
      id: published.id,
      overrideAccess: true,
    })

    await expect(
      payload!.delete({
        collection: 'pages',
        id: published.id,
        overrideAccess: false,
      }),
    ).rejects.toMatchObject({ status: 403 })

    const after = await payload!.findByID({
      collection: 'pages',
      id: published.id,
      overrideAccess: true,
    })
    expect(after).toEqual(before)
  })

  it('enforces publication access on every segment of a nested path', async () => {
    expect(await getPageByPath('/public-page/private-child')).toBeNull()
    expect(await getPageByPath('/private-page/public-child')).toBeNull()
  })

  it('excludes protected destinations from public header navigation', async () => {
    expect(await getHeaderNav()).toEqual([{ label: 'Public', path: '/public-page' }])
  })

  it('allows authenticated readers to retrieve drafts without making public reads authenticated', async () => {
    const result = await payload!.find({
      collection: 'pages',
      overrideAccess: false,
      user: { ...user, collection: 'users' },
      where: { id: { equals: draft.id } },
    })

    expect(result.docs.map((page) => page.id)).toEqual([draft.id])
    expect(await getPageByPath('/private-page')).toBeNull()
  })
})
