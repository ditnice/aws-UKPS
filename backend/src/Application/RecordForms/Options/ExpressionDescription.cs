using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace UKPS.Api.Application.RecordForms.Options;

/// <summary>
/// Describes a provider's query or projection as stable text for the form fingerprint.
/// <see cref="Expression.ToString"/> is not used because its format is not a stable API, so a
/// .NET upgrade could change the fingerprint without any change to the form.
/// </summary>
/// <remarks>
/// Only the node types provider expressions need are supported. Anything else, including a
/// captured variable, throws, so the describer is extended deliberately rather than producing
/// text that does not capture the expression's meaning.
/// </remarks>
internal static class ExpressionDescription
{
    public static string Describe(Expression expression) =>
        expression switch
        {
            LambdaExpression lambda =>
                $"({string.Join(", ", lambda.Parameters.Select(Describe))}) => {Describe(lambda.Body)}",
            ParameterExpression parameter => parameter.Name ?? "_",
            MemberExpression { Expression: not null } member =>
                $"{Describe(member.Expression)}.{member.Member.Name}",
            MethodCallExpression call => DescribeCall(call),
            NewExpression create =>
                $"new {create.Type.Name}({string.Join(", ", create.Arguments.Select(Describe))})",
            UnaryExpression { NodeType: ExpressionType.Quote } quote => Describe(quote.Operand),
            UnaryExpression
            {
                NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked,
            } convert => $"({convert.Type.Name})({Describe(convert.Operand)})",
            BinaryExpression binary =>
                $"({Describe(binary.Left)} {binary.NodeType} {Describe(binary.Right)})",
            ConditionalExpression conditional =>
                $"({Describe(conditional.Test)} ? {Describe(conditional.IfTrue)} : {Describe(conditional.IfFalse)})",
            ConstantExpression constant => DescribeConstant(constant),
            _ => throw Unsupported(expression),
        };

    private static string DescribeCall(MethodCallExpression call)
    {
        MethodInfo method = call.Method;
        IEnumerable<Expression> arguments = call.Arguments;
        string target;
        if (call.Object is not null)
        {
            target = Describe(call.Object);
        }
        else if (method.IsDefined(typeof(ExtensionAttribute), false))
        {
            target = Describe(call.Arguments[0]);
            arguments = call.Arguments.Skip(1);
        }
        else
        {
            target = method.DeclaringType!.Name;
        }

        return $"{target}.{method.Name}({string.Join(", ", arguments.Select(Describe))})";
    }

    private static string DescribeConstant(ConstantExpression constant) =>
        constant.Value switch
        {
            null => "null",
            string text => JsonSerializer.Serialize(text),
            bool flag => flag ? "true" : "false",
            Enum member => $"{member.GetType().Name}.{member}",
            IFormattable number when constant.Type.IsPrimitive || number is decimal =>
                number.ToString(null, CultureInfo.InvariantCulture),
            _ => throw Unsupported(constant),
        };

    private static NotSupportedException Unsupported(Expression expression) =>
        new(
            $"Option provider expressions cannot use {expression.NodeType} nodes of type {expression.Type.Name}. Captured variables are not allowed; extend {nameof(ExpressionDescription)} for anything else."
        );
}
