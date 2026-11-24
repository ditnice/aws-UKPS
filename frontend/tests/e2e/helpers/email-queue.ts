import { ReceiveMessageCommand, SQSClient } from '@aws-sdk/client-sqs'
import { expect } from '@playwright/test'

/* On the Docker Compose stack the backend queues emails on Floci's SQS, and its listener is off
 * (Email__ListenerEnabled: "false"), so every email the app sends stays on this queue for tests
 * to read. See the backend and floci services in docker-compose.yml. */
const queueUrl =
  process.env.E2E_EMAIL_QUEUE_URL?.trim() || 'http://floci:4566/000000000000/ukps-e2e-email'

const sqs = new SQSClient({
  endpoint: new URL(queueUrl).origin,
  region: 'eu-west-2',
  // Floci accepts any credentials; these stop the SDK looking for real ones.
  credentials: { accessKeyId: 'e2e', secretAccessKey: 'e2e' },
})

/** A queued email, as the backend serialises its SendEmailCommand. */
export type QueuedEmail = {
  RecipientAddress: string
  Email: { $type: string } & Record<string, unknown>
}

async function peekQueuedEmails(): Promise<QueuedEmail[]> {
  const { Messages = [] } = await sqs.send(
    new ReceiveMessageCommand({
      QueueUrl: queueUrl,
      MaxNumberOfMessages: 10,
      VisibilityTimeout: 0,
    }),
  )
  return Messages.flatMap((message) =>
    message.Body ? [JSON.parse(message.Body) as QueuedEmail] : [],
  )
}

export async function findQueuedEmail(
  recipient: string,
  type: string,
): Promise<QueuedEmail | undefined> {
  return (await peekQueuedEmails()).find(
    (email) => email.RecipientAddress === recipient && email.Email.$type === type,
  )
}

export async function waitForQueuedEmail(recipient: string, type: string): Promise<QueuedEmail> {
  let found: QueuedEmail | undefined
  await expect
    .poll(async () => (found = await findQueuedEmail(recipient, type)), {
      message: `A ${type} email is queued for ${recipient}`,
    })
    .toBeDefined()
  return found!
}
