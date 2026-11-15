'use client'

import { useFormStatus } from 'react-dom'

import { Button } from '@/components/Button/Button'

const GenerateOtpButton = () => {
  const { pending } = useFormStatus()

  return (
    <Button buttonType="submit" type="cta" disabled={pending}>
      Generate OTP
    </Button>
  )
}

export default GenerateOtpButton
