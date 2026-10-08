import type {
  BiomarkerType,
  DesignationStatus,
  EamsOpinionDecision,
  GenomicTestMandatoryStatus,
  GenomicTestNgtdRelationship,
  IndicationPaediatricStatus,
  MedicineHtaAssessor,
  MedicineTechnologyStatus,
  NetUkBudgetImpactBand,
  NhsServiceChangesRequired,
  PatientAccessSchemeRegion,
  ReferenceDataDto,
  RegulatoryDateDto,
  YesNoUnknown,
} from '@/client/generated'

export const notProvidedText = 'Not yet provided'

export const yesNoUnknownLabels: Record<YesNoUnknown, string> = {
  Yes: 'Yes',
  No: 'No',
  Unknown: 'Unknown',
}

export const indicationPaediatricStatusLabels: Record<IndicationPaediatricStatus, string> = {
  ExclusivelyChildren: 'Exclusively children',
  ExclusivelyAdults: 'Exclusively adults',
  BothChildrenAndAdults: 'Both children and adults',
  Unknown: 'Unknown',
}

export const medicineTechnologyStatusLabels: Record<MedicineTechnologyStatus, string> = {
  Biosimilar: 'Biosimilar',
  NewChemicalOrBiologicalEntity: 'New chemical or biological entity',
  NewDosingRegimen: 'New dosing regimen',
  NewFormulation: 'New formulation',
  NewIndication: 'New indication',
  NewPresentation: 'New presentation',
  SpcAmendmentWithoutIndicationChange: 'SPC amendment without indication change',
}

export const designationStatusLabels: Record<DesignationStatus, string> = {
  Granted: 'Granted',
  NotGranted: 'Not granted',
  DecisionToSubmitOngoing: 'Decision to submit ongoing',
  ApplicationSubmittedDecisionPending: 'Application submitted, decision pending',
  NoSubmissionIntended: 'No submission intended',
}

export const eamsOpinionDecisionLabels: Record<EamsOpinionDecision, string> = {
  Positive: 'Positive',
  Negative: 'Negative',
}

export const biomarkerTypeLabels: Record<BiomarkerType, string> = {
  GenomicBiomarker: 'Genomic biomarker',
  NonGenomicBiomarker: 'Non-genomic biomarker',
  Unknown: 'Unknown',
}

export const genomicTestNgtdRelationshipLabels: Record<GenomicTestNgtdRelationship, string> = {
  NewTest: 'New test',
  ExistingTestNewIndication: 'Existing test, new indication',
  ExistingTestSameIndication: 'Existing test, same indication',
  Unknown: 'Unknown',
}

export const genomicTestMandatoryStatusLabels: Record<GenomicTestMandatoryStatus, string> = {
  RecommendedNotRequired: 'Recommended but not required',
  MandatoryAlternativesMayExist: 'Mandatory, alternatives may exist',
  MandatoryNoAlternatives: 'Mandatory, no alternatives',
  Unknown: 'Unknown',
}

export const nhsServiceChangesRequiredLabels: Record<NhsServiceChangesRequired, string> = {
  NoChanges: 'No changes',
  SomeChange: 'Some change',
  CompleteTransformation: 'Complete transformation',
  Unknown: 'Unknown',
}

export const patientAccessSchemeRegionLabels: Record<PatientAccessSchemeRegion, string> = {
  England: 'England',
  Wales: 'Wales',
  Scotland: 'Scotland',
  NorthernIreland: 'Northern Ireland',
}

export const netUkBudgetImpactBandLabels: Record<NetUkBudgetImpactBand, string> = {
  LessThan5M: 'Less than £5 million',
  Between5MAnd40M: 'Between £5 million and £40 million',
  FortyMOrMore: '£40 million or more',
  Unknown: 'Unknown',
}

export const medicineHtaAssessorLabels: Record<MedicineHtaAssessor, string> = {
  Nice: 'NICE',
  Smc: 'SMC',
  Awmsg: 'AWMSG',
}

/**
 * Formats an optional value for display, falling back to {@link notProvidedText}.
 */
export const formatText = (value: string | null | undefined): string =>
  value?.trim() ? value : notProvidedText

/**
 * Formats an optional enum value using its label map.
 */
export const formatEnum = <T extends string>(
  value: T | null | undefined,
  labels: Record<T, string>,
): string => (value ? labels[value] : notProvidedText)

/**
 * Formats an optional list of enum values as a comma-separated list of labels.
 */
export const formatEnumList = <T extends string>(
  values: readonly T[] | null | undefined,
  labels: Record<T, string>,
): string => formatList(values?.map((value) => labels[value]))

/**
 * Formats an optional list of strings as a comma-separated list.
 */
export const formatList = (values: readonly string[] | null | undefined): string =>
  values && values.length > 0 ? values.join(', ') : notProvidedText

export const formatReferenceData = (value: ReferenceDataDto | null | undefined): string =>
  value?.label ?? notProvidedText

export const formatReferenceDataList = (values: readonly ReferenceDataDto[] | null | undefined) =>
  formatList(values?.map((value) => value.label))

const parseDateOnly = (value: string): Date => {
  const [year, month, day] = value.split('-').map(Number)
  return new Date(Date.UTC(year, month - 1, day))
}

const monthYearFormat = new Intl.DateTimeFormat('en-GB', {
  month: 'long',
  year: 'numeric',
  timeZone: 'UTC',
})

const fullDateFormat = new Intl.DateTimeFormat('en-GB', {
  day: 'numeric',
  month: 'long',
  year: 'numeric',
  timeZone: 'UTC',
})

/**
 * Formats a regulatory date at its recorded precision, e.g. "Q2 2027 (estimated)".
 */
export const formatRegulatoryDate = (value: RegulatoryDateDto | null | undefined): string => {
  if (!value) {
    return notProvidedText
  }

  const date = parseDateOnly(value.dateValue)
  let formatted: string
  switch (value.datePrecision) {
    case 'EstimatedQuarter':
      formatted = `Q${Math.floor(date.getUTCMonth() / 3) + 1} ${date.getUTCFullYear()} (estimated)`
      break
    case 'EstimatedMonth':
      formatted = `${monthYearFormat.format(date)} (estimated)`
      break
    case 'ActualDate':
      formatted = fullDateFormat.format(date)
      break
  }

  return value.isConfidential ? `${formatted} (confidential)` : formatted
}
