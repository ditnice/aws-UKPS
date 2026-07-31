'use client'

import { QRCodeSVG } from 'qrcode.react'
import { useState } from 'react'

import { postAuthSetupUser, postAuthVerifyMfa } from '@/client/generated/sdk.gen'

export default function ExampleAuthenticationPage() {
  const [otpLink, setOtpLink] = useState<string | undefined>()
  const [authSession, setAuthSession] = useState<string | undefined>()
  const [setupToken, setSetupToken] = useState<string | undefined>()

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    const formData = new FormData(event.target as HTMLFormElement)
    const setupToken = formData.get('setupToken') as string
    const newPassword = formData.get('newPassword') as string

    setSetupToken(setupToken)

    try {
      const response = await postAuthSetupUser({
        body: {
          setupToken,
          newPassword,
        },
      })
      setOtpLink(response.data?.otpAuthUri)
      setAuthSession(response.data?.authenticationSessionId)
    } catch (error) {
      console.error('Error:', error)
    }
  }

  const handleVerifyMfaSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    const formData = new FormData(event.target as HTMLFormElement)
    const code = formData.get('code') as string

    if (!authSession) throw Error()
    if (!setupToken) throw Error()

    try {
      const response = await postAuthVerifyMfa({
        body: {
          code,
          authenticationSessionId: authSession,
          setupToken,
        },
      })
      console.log(response)
    } catch (error) {
      console.error('Error:', error)
    }
  }

  return (
    <>
      <h1>Auth</h1>
      {JSON.stringify({ otpLink, authSession })}
      <form onSubmit={handleSubmit}>
        <label>
          Setup Token:
          <input type="text" name="setupToken" required />
        </label>
        <br />
        <label>
          New Password:
          <input type="password" name="newPassword" required />
        </label>
        <br />
        <button type="submit">Submit</button>
      </form>

      <div>{otpLink && <QRCodeSVG value={otpLink} size={256} level="M" />}</div>

      <form onSubmit={handleVerifyMfaSubmit}>
        <label>
          Code:
          <input type="text" name="code" required />
        </label>
        <br />
        <button type="submit">Submit</button>
      </form>
    </>
  )
}
