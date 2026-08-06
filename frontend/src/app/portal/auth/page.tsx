'use client'

import { QRCodeSVG } from 'qrcode.react'
import { Dispatch, SetStateAction, useState } from 'react'

import {
  postAuthSetupUser,
  postAuthVerifyMfa,
  postAuthLogin,
  postAuthMfa,
  getUsers,
  postAuthRefresh,
} from '@/client/generated/sdk.gen'

type SetupUserProps = {
  setOtpLink: Dispatch<SetStateAction<string | undefined>>
  setAuthSession: Dispatch<SetStateAction<string | undefined>>
  setSetupToken: Dispatch<SetStateAction<string | undefined>>
}
const SetupUser = ({ setSetupToken, setOtpLink, setAuthSession }: SetupUserProps) => {
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
        credentials: 'include',
      })
      setOtpLink(response.data?.otpAuthUri)
      setAuthSession(response.data?.authenticationSession)
    } catch (error) {
      console.error('Error:', error)
    }
  }
  return (
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
  )
}

type SetupMfaProps = {
  authSession: string
  setupToken: string
  otpLink: string
}
const SetupMfa = ({ authSession, setupToken, otpLink }: SetupMfaProps) => {
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
          authenticationSession: authSession,
          setupToken,
        },
      })
      console.log(response)
    } catch (error) {
      console.error('Error:', error)
    }
  }

  return (
    <div>
      <QRCodeSVG value={otpLink} size={256} level="M" />
      <form onSubmit={handleVerifyMfaSubmit}>
        <label>
          Code:
          <input type="text" name="code" required />
        </label>
        <br />
        <button type="submit">Submit</button>
      </form>
    </div>
  )
}

const UserLogin = () => {
  const [challengeAuthSession, setChallengeAuthSession] = useState<string | undefined>(undefined)
  const handleLoginSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    const formData = new FormData(event.target as HTMLFormElement)
    const username = formData.get('username') as string
    const password = formData.get('password') as string

    try {
      const response = await postAuthLogin({ body: { username, password }, credentials: 'include' })
      if (response.error?.challengeType === 'MultiFactorAuthenticationRequired') {
        console.log('MFA required, challenge type:', response.error.challengeType)
        setChallengeAuthSession(response.error.authenticationSession ?? undefined)
      }
    } catch (error) {
      console.error('Error:', error)
    }
  }

  const handleMfaSubmit = async (event: React.FormEvent) => {
    event.preventDefault()
    const formData = new FormData(event.target as HTMLFormElement)
    const username = formData.get('username') as string
    const code = formData.get('mfaCode') as string

    if (!challengeAuthSession) throw Error()

    try {
      await postAuthMfa({
        body: { username, code, authenticationSession: challengeAuthSession },
        credentials: 'include',
      })
    } catch (error) {
      console.error('Error:', error)
    }
  }

  return (
    <div>
      <form onSubmit={handleLoginSubmit}>
        <label>
          Username:
          <input type="text" name="username" required />
        </label>
        <br />
        <label>
          Password:
          <input type="password" name="password" required />
        </label>
        <br />
        <button type="submit">Login</button>
      </form>
      {challengeAuthSession && (
        <form onSubmit={handleMfaSubmit}>
          <label>
            Username:
            <input type="text" name="username" required />
          </label>
          <br />
          <label>
            MFA Code:
            <input type="text" name="mfaCode" required />
          </label>
          <br />
          <button type="submit">Verify MFA</button>
        </form>
      )}
    </div>
  )
}

const TestFetch = () => {
  const fetchData = async () => {
    const response = await getUsers({ credentials: 'include' })
    console.log(response)
  }

  return (
    <div>
      <button onClick={fetchData}>Test Fetch</button>
    </div>
  )
}

const TestRefresh = () => {
  const handleRefreshSubmit = () => {
    postAuthRefresh({
      credentials: 'include',
      headers: { 'X-CSRF-Token': getCookie('csrf_token') },
    })
  }
  return <button onClick={handleRefreshSubmit}>Refresh Token</button>
}

function getCookie(name: string): string | null {
  const cookies = document.cookie.split(';')

  for (const cookie of cookies) {
    const [key, value] = cookie.trim().split('=')

    if (key === name) {
      return decodeURIComponent(value)
    }
  }

  return null
}

export default function ExampleAuthenticationPage() {
  const [otpLink, setOtpLink] = useState<string | undefined>(undefined)
  const [authSession, setAuthSession] = useState<string | undefined>(undefined)
  const [setupToken, setSetupToken] = useState<string | undefined>(undefined)

  return (
    <>
      <h1>Auth</h1>
      <SetupUser
        setOtpLink={setOtpLink}
        setAuthSession={setAuthSession}
        setSetupToken={setSetupToken}
      />
      {otpLink && authSession && setupToken && (
        <SetupMfa otpLink={otpLink} authSession={authSession} setupToken={setupToken} />
      )}
      <UserLogin />
      <TestFetch />
      <TestRefresh />

      {JSON.stringify({ otpLink, authSession })}
    </>
  )
}
