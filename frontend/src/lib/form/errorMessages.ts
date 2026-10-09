export const errorMessages = {
  // Authentication
  incorrectMfaCode: 'The code is incorrect. Try again.',

  // Required
  organisationNameRequired: 'Enter the organisation name',
  addressRequired: 'Enter organisation address',
  organisationEmailRequired: 'Enter an email address',

  //// Setup
  personalFullNameRequired: 'Enter your full name',
  personalEmailRequired: 'Enter your email address',
  workEmailRequired: 'Enter your work email address',
  personalPhoneRequired: 'Enter your phone number',
  phoneRequired: 'Enter a phone number',
  passwordRequired: 'Enter your password',
  securityCodeRequired: 'Enter your security code',
  organisationRequired: 'Select an organisation',

  //// Onboarding
  userNameRequired: 'Enter their full name',
  userEmailRequired: 'Enter an email address',
  userPhoneNumberRequired: 'Enter their phone number',

  //// Records
  companyCodeRequired: 'Enter company code',
  genericNameRequired: 'Enter generic name',
  recordTitleRequired: 'Enter record title',

  // Formatting
  emailFormat: 'Enter an email address in the correct format, like name@example.com',
  phoneFormat: 'Enter a phone number, like 01632 960 001, 07700 900 982 or +44 808 157 0192',
  securityCodeFormat: 'Enter a 6-digit security code',
  passwordFormat: 'Password must be at least 8 characters long',

  passwordTooLong: 'Your password must be 256 characters or less',
  passwordWhitespace: 'Your password cannot contain spaces',

  genericNamesDistinct: 'Generic names must be distinct',
  recordTitleTooLong: 'Record title cannot be greater than 100 characters',

  // Data retrieval
  failedToRetrieveCurrentUser: 'Failed to retrieve the current user.',
  anErrorOccurredWhenTryingToRetrieveTheUserMembershipRequest:
    'An error occurred when trying to retrieve the user membership request',

  updatingUserDetailsError: 'An error occurred when updating user details.',
  creatingNewRecordError: 'An error occurred when creating a new medicine record.',
} as const
