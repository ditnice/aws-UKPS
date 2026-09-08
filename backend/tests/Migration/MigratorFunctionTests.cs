using Amazon.Lambda.TestUtilities;
using MigratorLambda;

namespace UKPS.Api.Tests.Migration;

/// <summary>
///
/// Manual test for the migrator lambda.
///
/// Prerequisites:
/// - Docker running a local Postgres instance
/// - AWS SSO configured
///
/// To run: remove the Skip attribute and enter env vars.
/// </summary>
public sealed class MigratorFunctionTests
{
    [Fact(Skip = "Intended for manual testing only")]
    public async Task FunctionHandler_RunsMigrationsAgainstRealDb()
    {
        Environment.SetEnvironmentVariable("AWS_REGION", "");
        Environment.SetEnvironmentVariable("DB_SECRET_ARN", "");
        Environment.SetEnvironmentVariable("Seeding__ReseedOnStartup", "");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "");

        var context = new TestLambdaContext { RemainingTime = TimeSpan.FromMinutes(5) };

        var exception = await Record.ExceptionAsync(() =>
            MigratorFunction.FunctionHandler(null!, context)
        );

        Assert.Null(exception);
    }
}
