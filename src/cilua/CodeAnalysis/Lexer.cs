using cilua.CodeAnalysis.Syntax;

namespace cilua.CodeAnalysis;

/// <summary>
/// Converts source text into a flat token stream. Whitespace/comments are captured as
/// trivia and attached to the surrounding tokens (leading/trailing) rather than
/// discarded, so a formatter or "keep comments" refactor stays possible later —
/// same trade-off Roslyn makes.
/// </summary>
public sealed class Lexer(SourceText source)
{
    private int _position;

    private char Current => Peek(0);
    private char Lookahead => Peek(1);
    private char Peek(int offset)
    {
        var index = _position + offset;
        return index >= source.Length ? '\0' : source[index];
    }
    
    /// <summary>Lexes the entire source into a token array, terminated by EndOfFileToken.</summary>
    public List<SyntaxToken> Lex()
    {
        var tokens = new List<SyntaxToken>();
        SyntaxToken token;
        do
        {
            token = NextToken();
            tokens.Add(token);
        } while (token.Kind != SyntaxKind.EndOfFileToken);
        return tokens;
    }

    private SyntaxToken NextToken()
    {
        throw new NotImplementedException();
    }
}