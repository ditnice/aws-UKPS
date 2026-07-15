'use client'

import { useState } from 'react'

import { Tag } from '@nice-digital/nds-tag'

import { Example } from '../../_components/Example'
import styles from '../../page.module.scss'

export function Examples() {
  const [showDiabetesFilter, setShowDiabetesFilter] = useState(true)

  return (
    <>
      <Example title="Guidance and service status colours">
        <div className={styles.inlineExamples}>
          <Tag>Default</Tag>
          <Tag alpha>Alpha</Tag>
          <Tag beta>Beta</Tag>
          <Tag isNew>New</Tag>
          <Tag updated>Updated</Tag>
          <Tag consultation>Consultation</Tag>
        </div>
      </Example>
      <Example title="Semantic feedback colours">
        <div className={styles.inlineExamples}>
          <Tag info>Information</Tag>
          <Tag success>Complete</Tag>
          <Tag caution>Review needed</Tag>
          <Tag error>Action required</Tag>
        </div>
      </Example>
      <Example title="Impact, flush, outline and removable">
        <div className={styles.inlineExamples}>
          <Tag impact>Priority</Tag>
          <Tag flush>Compact</Tag>
          <Tag outline>Filtered</Tag>
          {showDiabetesFilter ? (
            <Tag
              outline
              remove={
                <button type="button" onClick={() => setShowDiabetesFilter(false)}>
                  Remove diabetes filter
                </button>
              }
            >
              Diabetes
            </Tag>
          ) : (
            <button type="button" onClick={() => setShowDiabetesFilter(true)}>
              Restore diabetes filter
            </button>
          )}
        </div>
      </Example>
      <Example title="Documented Live phase limitation">
        <p>
          The design guidance documents a Live phase tag, but the installed React package does not
          expose a corresponding prop or style. It is not emulated here because that would
          misrepresent the component API.
        </p>
      </Example>
    </>
  )
}
