namespace cilua.CodeAnalysis.Syntax;

/// <summary>
/// Whitespace/comments attached to a token. Kept (not discarded) so a future
/// pretty-printer / formatter can round-trip source exactly, as in Roslyn.
/// </summary>
public sealed record SyntaxTrivia(SyntaxKind Kind, TextSpan Span, string Text)
{
    public override string ToString() => Text;
}