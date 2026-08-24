import { BackLink } from '@/components/BackLink/BackLink'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'

export default function RejectUser() {
  return (
    <>
      <PageHeader backLink={<BackLink href="#">Back</BackLink>} heading="Reject user" />
      <p>You are about to reject julie.brooks@email.com&#39;s request for an account.</p>

      <ButtonGroup>
        <Button variant="cta">Reject user</Button>
        <Button variant="secondary">Cancel</Button>
      </ButtonGroup>
    </>
  )
}
