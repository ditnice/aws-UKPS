import type { InputWidth } from '@/components/Input/Input'

// Independent expectations for the shared input, textarea, password and select width contract.
export const inputWidthCases: { width: InputWidth; maxWidth: string; fluidWidth: string }[] = [
  { width: 2, maxWidth: '2.75em', fluidWidth: '' },
  { width: 3, maxWidth: '3.75em', fluidWidth: '' },
  { width: 4, maxWidth: '4.5em', fluidWidth: '' },
  { width: 5, maxWidth: '5.5em', fluidWidth: '' },
  { width: 10, maxWidth: '11.5em', fluidWidth: '' },
  { width: 20, maxWidth: '20.5em', fluidWidth: '' },
  { width: 30, maxWidth: '29.5em', fluidWidth: '' },
  { width: 'full', maxWidth: '', fluidWidth: '100%' },
  { width: 'three-quarters', maxWidth: '', fluidWidth: '75%' },
  { width: 'two-thirds', maxWidth: '', fluidWidth: '66.6667%' },
  { width: 'one-half', maxWidth: '', fluidWidth: '50%' },
  { width: 'one-third', maxWidth: '', fluidWidth: '33.3333%' },
  { width: 'one-quarter', maxWidth: '', fluidWidth: '25%' },
]
