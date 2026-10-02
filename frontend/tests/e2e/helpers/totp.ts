import * as OTPAuth from 'otpauth'

export function generateTotp(secret: string): string {
  const totp = new OTPAuth.TOTP({
    algorithm: 'SHA1',
    digits: 6,
    period: 30,
    secret: OTPAuth.Secret.fromBase32(secret.replace(/\s/g, '')),
  })

  return totp.generate()
}
