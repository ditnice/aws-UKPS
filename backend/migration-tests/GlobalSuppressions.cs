// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Style",
    "IDE0130:Namespace does not match folder structure",
    Justification = "We have an odd folder naming convention that means this is irrelevant",
    Scope = "namespace",
    Target = "~N:MigratorLambda.Tests"
)]
[assembly: SuppressMessage(
    "Naming",
    "CA1707:Identifiers should not contain underscores",
    Justification = "Convention used in test names.",
    Scope = "member",
    Target = "~M:MigratorLambda.Tests.MigratorFunctionTests.FunctionHandler_RunsMigrationsAgainstRealDb~System.Threading.Tasks.Task"
)]
