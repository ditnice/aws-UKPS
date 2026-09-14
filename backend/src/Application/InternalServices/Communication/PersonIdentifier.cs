using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using UKPS.Api.Persistence.Entities.Identity;

namespace UKPS.Api.Application.InternalServices.Communication;

/// <summary>
/// Represents an identifier for a person within the UK PharmaScan system.
/// </summary>
/// <remarks>
/// A person can be identified either by their Cognito username or by the
/// identifier of their registration request.
/// </remarks>
[JsonConverter(typeof(PersonIdentifierJsonConverter))]
public record PersonIdentifier
{
    private const string RegistrationRequestPrefix = "membership-request-";

    /// <summary>
    /// Gets the identifier of the registration request associated with the person.
    /// </summary>
    /// <value>
    /// The registration request identifier, or <see langword="null"/> if the
    /// person is identified by their Cognito username.
    /// </value>
    public int? RegistrationRequestId { get; private init; }

    /// <summary>
    /// Gets the Cognito username identifying the person.
    /// </summary>
    /// <value>
    /// The Cognito username, or <see langword="null"/> if the person is
    /// identified by a registration request identifier.
    /// </value>
    public CognitoUsername? Username { get; private init; }

    private PersonIdentifier() { }

    /// <summary>
    /// Creates a person identifier from a registration request identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the registration request.
    /// </param>
    /// <returns>
    /// A person identifier representing the registration request.
    /// </returns>
    public static PersonIdentifier FromRegistrationId(int id)
    {
        return new PersonIdentifier() { RegistrationRequestId = id };
    }

    /// <summary>
    /// Converts a Cognito username to a person identifier.
    /// </summary>
    /// <param name="value">
    /// The Cognito username identifying the person.
    /// </param>
    /// <returns>
    /// A person identifier representing the Cognito user.
    /// </returns>
    public static implicit operator PersonIdentifier(CognitoUsername value)
    {
        return FromCognitoUsername(value);
    }

    /// <summary>
    /// Creates a person identifier from a Cognito username.
    /// </summary>
    /// <param name="value">
    /// The Cognito username identifying the person.
    /// </param>
    /// <returns>
    /// A person identifier representing the Cognito user.
    /// </returns>
    public static PersonIdentifier FromCognitoUsername(CognitoUsername value)
    {
        return new PersonIdentifier() { Username = value };
    }

    /// <summary>
    /// Returns a string representation of the person identifier.
    /// </summary>
    /// <returns>
    /// The Cognito username when the person is identified by a Cognito account;
    /// otherwise, a registration request identifier prefixed with
    /// <c>membership-request-</c>.
    /// </returns>
    public override string ToString()
    {
        if (Username.HasValue)
        {
            return Username.Value.Value;
        }

        return $"{RegistrationRequestPrefix}{RegistrationRequestId!.Value}";
    }

    internal sealed class PersonIdentifierJsonConverter : JsonConverter<PersonIdentifier>
    {
        public override PersonIdentifier? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected a string but found {reader.TokenType}.");
            }

            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new JsonException("Person identifier cannot be empty.");
            }

            if (value.StartsWith(RegistrationRequestPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var idValue = value[RegistrationRequestPrefix.Length..];

                if (!int.TryParse(idValue, CultureInfo.InvariantCulture, out var id))
                {
                    throw new JsonException($"Invalid registration request identifier '{value}'.");
                }

                return FromRegistrationId(id);
            }

            try
            {
                return FromCognitoUsername(CognitoUsername.Parse(value));
            }
            catch (Exception ex)
            {
                throw new JsonException($"Invalid person identifier '{value}'.", ex);
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            PersonIdentifier value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
