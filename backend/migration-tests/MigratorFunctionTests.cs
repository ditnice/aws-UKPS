using Amazon.Lambda.TestUtilities;
using MigratorLambda;

namespace MigratorLambda.Tests;

public class MigratorFunctionTests
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
