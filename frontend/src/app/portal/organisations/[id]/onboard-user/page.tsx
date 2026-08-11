import { Button } from '@nice-digital/nds-button'

import { BackLink } from '@/components/BackLink/BackLink'
import { Input } from '@/components/Input/Input'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import styles from './page.module.scss'

interface Props {
  _params: Promise<{ id: string }>
}

export default async function OrganisationOnboardUserPage({ _params }: Props) {
  return (
    <>
      <PageHeader
        backLink={<BackLink href="#">Back</BackLink>}
        heading="Add a new user"
      ></PageHeader>

      <p>
        New users will be assigned the standard user role by default. You can change the permissions
        later using user management.
      </p>

      <Input label="Full name" name="full-name" width="one-third"></Input>

      <Input label="Work email address" name="email-address" width="one-third"></Input>

      <Input
        label="Phone number"
        name="phone-number"
        hint="For international numbers include the country code."
        width="one-third"
      ></Input>

      <div className={styles.actions}>
        <Button variant="cta">Send invite</Button>
        <Button variant="secondary">Cancel</Button>
      </div>
    </>
  )
}
