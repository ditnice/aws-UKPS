'use client'

import Link from 'next/link'

import { Button } from '@nice-digital/nds-button'

import { Input } from '@/components/Input/Input'
import { Select, SelectOption } from '@/components/Select/Select'

import styles from './RegistrationRequestForm.module.scss'

export function RegistrationRequestForm() {
  return (
    <>
      <Select
        defaultValue="choose"
        label="Select the organisation you are requesting access for"
        name="select-organisation-hint"
        width="one-third"
      >
        <SelectOption value="choose">Choose organisation</SelectOption>
        <SelectOption value="org1">Organisation 1</SelectOption>
        <SelectOption value="org2">Organisation 2</SelectOption>
      </Select>
      <p>
        If your organisation is not registered, you must{' '}
        <a href="URL">register your organisation (opens in a new tab)</a> before you can set up your
        account.
      </p>

      <Input label="Full name" name="Full name" width="one-third" className={styles.marginBottom} />

      <Input
        label="Work email address"
        name="Work email address"
        hint="We'll use this email address to contact you about your request. You must use an email address from your organisation."
        width="one-third"
        className={styles.marginBottom}
      />

      <Input
        label="Phone number"
        name="Phone number"
        hint="For international numbers include the country code."
        width="one-third"
        className={styles.marginBottom}
      />

      <Link href="/portal/request/request-submitted">
        <Button variant="cta">Submit request</Button>
      </Link>
    </>
  )
}
