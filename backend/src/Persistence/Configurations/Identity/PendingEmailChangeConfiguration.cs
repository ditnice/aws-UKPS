using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UKPS.Api.Persistence.Entities.Identity;

#pragma warning disable MA0048 // File name must match type name
internal partial class User
#pragma warning restore MA0048 // File name must match type name
{
    internal static void ConfigurePendingEmail(EntityTypeBuilder<User> builder)
    {
        // Column names match the flat columns these properties were originally mapped to.
        // PendingEmailChangeHandler keeps its state in private fields with no CLR properties, so they are
        // mapped as field-only properties. EF Core requires the property name to match the field name exactly.
        builder.ComplexProperty(
            u => u.PendingEmailHandler,
            pendingEmail =>
            {
                pendingEmail.Property<string?>("_workEmail").HasColumnName("pending_work_email");
                pendingEmail
                    .Property<string?>("_verificationToken")
                    .HasColumnName("pending_email_verification_token");
                pendingEmail
                    .Property<DateTime?>("_verifiedAt")
                    .HasColumnName("pending_email_verified_at");
                pendingEmail
                    .Property<DateTime?>("_requestedAt")
                    .HasColumnName("pending_email_requested_at");
            }
        );
    }
}
