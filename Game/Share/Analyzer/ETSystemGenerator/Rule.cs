using ET.Analyzer;
using Microsoft.CodeAnalysis;

namespace ET.ETSystemGenerator;

public static class ETSystemMethodIsInStaticPartialClassRule
{
    private const string Title = "ETSystem函数必须声明在静态分部类中";

    private const string MessageFormat = "ETSystem函数所在的类:{0} 不是静态类";

    private const string Description = "ETSystem函数必须声明在静态类中.";

    public static readonly DiagnosticDescriptor Rule =
            new DiagnosticDescriptor("ET1001",
                Title,
                MessageFormat,
                "Generator",
                DiagnosticSeverity.Error,
                true,
                Description);
}