import { BackLink } from '@/components/BackLink/BackLink'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'

export default function ManageUserAccess() {
  return (
    <>
      <PageHeader
        backLink={<BackLink href="#">Back</BackLink>}
        heading="Manage user&#39;s access"
      />
      <p>julie.brooks@example.com is a standard user.</p>
      <p>Choose what you want to do:</p>
      <ul>
        <li>Change permissions - change what the user can do.</li>
        <li>
          Deactivate user - temporarily stop the user&#39;s access. You can reactivate them later.
        </li>
        <li>Remove user - permanently remove the user&#39;s access.</li>
      </ul>

      <ButtonGroup>
        <Button variant="cta">Change permissions</Button>
        <Button variant="secondary">Deactivate user</Button>
        <Button variant="secondary">Remove user</Button>
      </ButtonGroup>
    </>
  )
}
