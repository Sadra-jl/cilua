using System.Collections.Immutable;
using System.Globalization;
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
        var leadingTrivia = ReadTrivia();

        var start = _position;

        if (_position >= source.Length)
        {
            return MakeToken(SyntaxKind.EndOfFileToken, start, "\0", null, [..leadingTrivia]);
        }

        // Long string literal: [[ ... ]] or [=[ ... ]=] etc. Must be disambiguated from `[`.
        if (Current == '[' && Lookahead is '[' or '=')
        {
            if (TryReadLongBracket(out var content, out var level, requireStringOpener: true))
            {
                var text = source.Text.Substring(start, _position - start);
                return MakeToken(SyntaxKind.StringToken, start, text, content, [..leadingTrivia]);
            }
        }

        if (char.IsDigit(Current) || (Current == '.' && char.IsDigit(Lookahead)))
        {
            return ReadNumber(start, leadingTrivia);
        }

        if (Current is '"' or '\'')
        {
            return ReadQuotedString(start, leadingTrivia);
        }

        if (char.IsLetter(Current) || Current == '_')
        {
            return ReadIdentifierOrKeyword(start, leadingTrivia);
        }

        return ReadOperatorOrPunctuation(start, leadingTrivia);
    }

    private SyntaxToken ReadOperatorOrPunctuation(int start, List<SyntaxTrivia> leadingTrivia)
    {
        throw new NotImplementedException();
    }

    private SyntaxToken ReadIdentifierOrKeyword(int start, List<SyntaxTrivia> leadingTrivia)
    {
        throw new NotImplementedException();
    }

    private SyntaxToken ReadQuotedString(int start, List<SyntaxTrivia> leadingTrivia)
    {
        throw new NotImplementedException();
    }

    private SyntaxToken ReadNumber(int start, List<SyntaxTrivia> leadingTrivia)
    {
        var isHex = Current == '0' && Lookahead is 'x' or 'X'; //like 0xFF or 0XFF
        if (isHex)
        {
            _position += 2;//skip to FF
            while (Uri.IsHexDigit(Current) || Current == '.') _position++; // lua supports floating point hex like 0x1A.2B
            if (Current is 'p' or 'P')// it also supports exponents like 0x1.8p+1
            {
                _position++;
                if (Current is '+' or '-') _position++;//optional exponent sign
                while (char.IsDigit(Current)) _position++;
            }
        }
        else
        {
            while (char.IsDigit(Current)) _position++;
            if (Current == '.')
            {
                _position++;
                while (char.IsDigit(Current)) _position++;
            }
            if (Current is 'e' or 'E')
            {
                _position++;
                if (Current is '+' or '-') _position++;
                while (char.IsDigit(Current)) _position++;
            }
        }

        var text = source.Text.Substring(start, _position - start);
        var value = ParseNumericLiteral(text, isHex);
        return MakeToken(SyntaxKind.NumberToken, start, text, value, [..leadingTrivia]);
    }

    private double ParseNumericLiteral(string text, bool isHex)
    {
        if (!isHex)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
                ? d
                : throw new NotImplementedException("proper diagnostics");
        }

        // Minimal hex-float support (0x1p4, 0x.1p-2, plain 0x1A). Full IEEE hex-float
        // parsing is easy to get subtly wrong — flagged as a TODO for the final lexer.
        var body = text[2..];
        var pIndex = body.IndexOfAny(['p', 'P']);
        var mantissa = pIndex >= 0 ? body[..pIndex] : body;
        var exponent = 0;
        if (pIndex >= 0)
        {
            int.TryParse(body[(pIndex + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out exponent);
        }

        var dot = mantissa.IndexOf('.');
        var intPart = dot >= 0 ? mantissa[..dot] : mantissa;
        var fracPart = dot >= 0 ? mantissa[(dot + 1)..] : string.Empty;

        double value = 0;
        foreach (var c in intPart)
        {
            value = value * 16 + Convert.ToInt32(c.ToString(), 16);
        }
        var fracScale = 1.0 / 16;
        foreach (var c in fracPart)
        {
            value += Convert.ToInt32(c.ToString(), 16) * fracScale;
            fracScale /= 16;
        }
        return value * Math.Pow(2, exponent);
    }

    private SyntaxToken MakeToken(SyntaxKind kind, int start, string text, object? value, ImmutableArray<SyntaxTrivia> leadingTrivia)
    {
        var trailingTrivia = new List<SyntaxTrivia>();
        // Trailing trivia = whitespace/comments up to (but not including) the next
        // line break, so a line comment stays attached to the token it follows.
        while (Current is ' ' or '\t' or '\r')
        {
            var wsStart = _position;
            while (Current is ' ' or '\t' or '\r') _position++;
            trailingTrivia.Add(new SyntaxTrivia(SyntaxKind.WhitespaceTrivia, TextSpan.FromBounds(wsStart, _position), source.Text[wsStart.._position]));
        }

        if (Current != '-' || Lookahead != '-')
            return new SyntaxToken(kind, TextSpan.FromBounds(start, start + text.Length), text, value, leadingTrivia,
                [.. trailingTrivia]);
        var commentStart = _position;
        _position += 2;
        if (Current == '[' && Lookahead is '[' or '=')
        {
            var bracketStart = _position;
            if (TryReadLongBracket(out _, out _, requireStringOpener: false))
            {
                trailingTrivia.Add(new SyntaxTrivia(SyntaxKind.BlockCommentTrivia, TextSpan.FromBounds(commentStart, _position), source.Text[commentStart.._position]));
                return new SyntaxToken(kind, TextSpan.FromBounds(start, start + text.Length), text, value, leadingTrivia, [..trailingTrivia]);
            }
            _position = bracketStart;
        }
        while (Current != '\n' && Current != '\0') _position++;
        trailingTrivia.Add(new SyntaxTrivia(SyntaxKind.LineCommentTrivia, TextSpan.FromBounds(commentStart, _position), source.Text[commentStart.._position]));

        return new SyntaxToken(kind, TextSpan.FromBounds(start, start + text.Length), text, value, leadingTrivia, [..trailingTrivia]);
    }

    private List<SyntaxTrivia> ReadTrivia()
    {
        var trivia = new List<SyntaxTrivia>();
        while (true)
        {
            var start = _position;
            switch (Current)
            {
                case ' ' or '\t' or '\r':
                {
                    while (Current is ' ' or '\t' or '\r') _position++;
                    trivia.Add(new SyntaxTrivia(SyntaxKind.WhitespaceTrivia, TextSpan.FromBounds(start, _position), source.Text[start.._position]));
                    break;
                }
                case '\n':
                    _position++;
                    trivia.Add(new SyntaxTrivia(SyntaxKind.LineBreakTrivia, TextSpan.FromBounds(start, _position), "\n"));
                    break;
                case '-' when Lookahead == '-':
                {
                    _position += 2;
                    if (Current == '[' && Lookahead is '[' or '=')
                    {
                        var commentStart = _position;
                        if (TryReadLongBracket(out _, out _, requireStringOpener: false))
                        {
                            trivia.Add(new SyntaxTrivia(SyntaxKind.BlockCommentTrivia, TextSpan.FromBounds(start, _position), source.Text[start.._position]));
                            continue;
                        }
                        _position = commentStart; // not actually a long bracket, fall through to line comment
                    }
                    while (Current != '\n' && Current != '\0') _position++;
                    trivia.Add(new SyntaxTrivia(SyntaxKind.LineCommentTrivia, TextSpan.FromBounds(start, _position), source.Text[start.._position]));
                    break;
                }
            }
            break;
        }
        return trivia;
    }

    /// <summary>
    /// Reads [[...]] / [=[...]=] / [==[...]==]-style long brackets, used by both long
    /// strings and long comments.
    /// </summary>
    private bool TryReadLongBracket(out string? content, out int level, bool requireStringOpener)
    {
        throw new NotImplementedException();
    }
}