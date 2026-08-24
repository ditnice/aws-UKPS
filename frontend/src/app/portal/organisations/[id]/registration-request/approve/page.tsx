import { BackLink } from '@/components/BackLink/BackLink'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'

export default function ApproveUser() {
  return (
    <>
      <PageHeader backLink={<BackLink href="#">Back</BackLink>} heading="Approve user" />
      <p>You are about to approve julie.brooks@email.com&#39;s request for an account.</p>
      <p>Once aproved they will be able to access your organisations UKPS account.</p>

      <ButtonGroup>
        <Button variant="cta">Approve user</Button>
        <Button variant="secondary">Cancel</Button>
      </ButtonGroup>
    </>
  )
}
