'use client'

import { useState } from 'react'

import { Alert } from '@/components/Alert/Alert'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

type CreateRecordPageHeaderProps = {
  // Where Back goes once the user has seen the exit warning
  backHref: string
}

export const CreateRecordPageHeader = ({ backHref }: CreateRecordPageHeaderProps) => {
  const [showExitWarning, setShowExitWarning] = useState(false)

  return (
    <>
      {showExitWarning && (
        <Alert type="caution">
          <h2>If you continue, this record will not be saved</h2>
          <p>
            You must enter the company code, generic name, and record title for this record to be
            saved as a draft. You can update these answers later.
          </p>
        </Alert>
      )}
      <PageHeader
        heading="Provide product record details"
        preheading="Names and identifiers"
        backLink={
          <BackLink
            href={backHref}
            onClick={(event) => {
              // The first click warns; the next follows the link.
              if (!showExitWarning) {
                event.preventDefault()
                setShowExitWarning(true)
              }
            }}
          />
        }
      />
    </>
  )
}
