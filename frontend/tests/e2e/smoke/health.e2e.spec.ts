import { expect, test } from '../fixtures/test'

test('reports that the frontend is healthy', async ({ request }) => {
  const response = await request.get('/health')

  expect(response.status()).toBe(200)
  expect(await response.json()).toEqual({ status: 'ok' })
  expect(response.headers()['cache-control']).toBe('no-store')
})
