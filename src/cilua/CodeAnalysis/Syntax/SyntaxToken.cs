using System.Collections.Immutable;

namespace cilua.CodeAnalysis.Syntax;

/// <summary>
/// A single lexical token. Value holds the parsed literal value for number/string
/// tokens (e.g. the double for a NumberToken, the unescaped string for a StringToken).
/// </summary>
public sealed record SyntaxToken(
    SyntaxKind Kind,
    TextSpan Span,
    string Text,
    object? Value,
    
    //for example consider
    //--comment
    //local x = 42   --comment
    //leading trivia for local is --comment\n
    //and the trailing is ' '
    ImmutableArray<SyntaxTrivia> LeadingTrivia,
    ImmutableArray<SyntaxTrivia> TrailingTrivia,
    bool IsMissing = false)
{
    public override string ToString() => $"{Kind}: '{Text}'";
}
