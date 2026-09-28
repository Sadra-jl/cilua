using cilua.CodeAnalysis;

namespace cilua.Diagnostics;

/// <summary>
/// Accumulates diagnostics during lexing/parsing without throwing, so the parser can
/// recover and keep producing a (possibly partial) tree — same philosophy as Roslyn.
/// </summary>
public sealed class DiagnosticBag
{
    private readonly List<Diagnostic> _diagnostics = [];

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;
    public bool HasErrors => _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

    public void ReportError(TextSpan span, string message) =>
        _diagnostics.Add(new Diagnostic(span, message, DiagnosticSeverity.Error));

    public void ReportWarning(TextSpan span, string message) =>
        _diagnostics.Add(new Diagnostic(span, message, DiagnosticSeverity.Warning));

    public void AddRange(IEnumerable<Diagnostic> diagnostics) => _diagnostics.AddRange(diagnostics);
}