using cilua.CodeAnalysis;

namespace cilua.Diagnostics;

public readonly struct Diagnostic(TextSpan span, string message, DiagnosticSeverity severity)
{
    public TextSpan Span { get; } = span;
    public string Message { get; } = message;
    public DiagnosticSeverity Severity { get; } = severity;

    public override string ToString() => $"{Severity}: {Message} {Span}";
}