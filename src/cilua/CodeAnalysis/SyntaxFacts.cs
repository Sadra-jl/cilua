namespace cilua.CodeAnalysis;

/// <summary>
/// Static helpers for keyword classification and operator precedence — the Lua
/// grammar's precedence table (lowest to highest), transcribed from the Lua 5.4
/// reference manual 3.4.8:
///
///   or
///   and
///   &lt;  &gt;  &lt;=  &gt;=  ~=  ==
///   |
///   ~          (binary xor)
///   &amp;
///   &lt;&lt;  &gt;&gt;
///   ..                         (right associative)
///   +  -
///   *  /  //  %
///   unary operators: not  #  -  ~
///   ^                          (right associative, binds tighter than unary)
/// </summary>
public static class SyntaxFacts
{
    private static readonly Dictionary<string, SyntaxKind> Keywords = new()
    {
        ["and"] = SyntaxKind.AndKeyword,
        ["break"] = SyntaxKind.BreakKeyword,
        ["do"] = SyntaxKind.DoKeyword,
        ["else"] = SyntaxKind.ElseKeyword,
        ["elseif"] = SyntaxKind.ElseifKeyword,
        ["end"] = SyntaxKind.EndKeyword,
        ["false"] = SyntaxKind.FalseKeyword,
        ["for"] = SyntaxKind.ForKeyword,
        ["function"] = SyntaxKind.FunctionKeyword,
        ["goto"] = SyntaxKind.GotoKeyword,
        ["if"] = SyntaxKind.IfKeyword,
        ["in"] = SyntaxKind.InKeyword,
        ["local"] = SyntaxKind.LocalKeyword,
        ["nil"] = SyntaxKind.NilKeyword,
        ["not"] = SyntaxKind.NotKeyword,
        ["or"] = SyntaxKind.OrKeyword,
        ["repeat"] = SyntaxKind.RepeatKeyword,
        ["return"] = SyntaxKind.ReturnKeyword,
        ["then"] = SyntaxKind.ThenKeyword,
        ["true"] = SyntaxKind.TrueKeyword,
        ["until"] = SyntaxKind.UntilKeyword,
        ["while"] = SyntaxKind.WhileKeyword,
    };

    public static SyntaxKind GetKeywordKind(string text) =>
        Keywords.GetValueOrDefault(text, SyntaxKind.IdentifierToken);

    public static bool IsKeyword(SyntaxKind kind) => kind switch
    {
        SyntaxKind.AndKeyword
            or SyntaxKind.BreakKeyword
            or SyntaxKind.DoKeyword
            or SyntaxKind.ElseKeyword
            or SyntaxKind.ElseifKeyword
            or SyntaxKind.EndKeyword
            or SyntaxKind.FalseKeyword
            or SyntaxKind.ForKeyword
            or SyntaxKind.FunctionKeyword
            or SyntaxKind.GotoKeyword
            or SyntaxKind.IfKeyword
            or SyntaxKind.InKeyword
            or SyntaxKind.LocalKeyword
            or SyntaxKind.NilKeyword
            or SyntaxKind.NotKeyword
            or SyntaxKind.OrKeyword
            or SyntaxKind.RepeatKeyword
            or SyntaxKind.ReturnKeyword
            or SyntaxKind.ThenKeyword
            or SyntaxKind.TrueKeyword
            or SyntaxKind.UntilKeyword
            or SyntaxKind.WhileKeyword => true,

        _ => false
    };
    /// <summary>Precedence of a prefix ("unary") operator: not, #, -, ~(bnot). Higher binds tighter.</summary>
    public static int GetUnaryOperatorPrecedence(SyntaxKind kind) => kind switch
    {
        SyntaxKind.NotKeyword or SyntaxKind.HashToken or SyntaxKind.MinusToken or SyntaxKind.TildeToken => 12,
        _ => 0,
    };

    /// <summary>
    /// Precedence of a binary operator. Higher number binds tighter. `^` and `..` are
    /// right-associative, handled in the parser by passing (precedence - 1) as the
    /// max-precedence bound when recursing on the right-hand side.
    /// </summary>
    public static int GetBinaryOperatorPrecedence(SyntaxKind kind) => kind switch
    {
        SyntaxKind.OrKeyword => 1,
        SyntaxKind.AndKeyword => 2,
        SyntaxKind.LessToken or SyntaxKind.GreaterToken or SyntaxKind.LessEqualsToken
            or SyntaxKind.GreaterEqualsToken or SyntaxKind.TildeEqualsToken or SyntaxKind.EqualsEqualsToken => 3,
        SyntaxKind.PipeToken => 4,
        SyntaxKind.TildeToken => 5,                 // binary xor
        SyntaxKind.AmpersandToken => 6,
        SyntaxKind.LessLessToken or SyntaxKind.GreaterGreaterToken => 7,
        SyntaxKind.DotDotToken => 8,                // right-associative
        SyntaxKind.PlusToken or SyntaxKind.MinusToken => 9,
        SyntaxKind.StarToken or SyntaxKind.SlashToken or SyntaxKind.DoubleSlashToken or SyntaxKind.PercentToken => 10,
        // 12 is unary; ^ is 13, binds tighter than unary minus on its left operand only. for example -2^2 or 2^-2 
        SyntaxKind.CaretToken => 14,
        _ => 0,
    };

    public static bool IsRightAssociative(SyntaxKind kind) =>
        kind is SyntaxKind.DotDotToken or SyntaxKind.CaretToken;

    public static string GetText(SyntaxKind kind) => kind switch
    {
        SyntaxKind.PlusToken => "+",
        SyntaxKind.MinusToken => "-",
        SyntaxKind.StarToken => "*",
        SyntaxKind.SlashToken => "/",
        SyntaxKind.DoubleSlashToken => "//",
        SyntaxKind.PercentToken => "%",
        SyntaxKind.CaretToken => "^",
        SyntaxKind.HashToken => "#",
        SyntaxKind.AmpersandToken => "&",
        SyntaxKind.TildeToken => "~",
        SyntaxKind.PipeToken => "|",
        SyntaxKind.LessLessToken => "<<",
        SyntaxKind.GreaterGreaterToken => ">>",
        SyntaxKind.EqualsEqualsToken => "==",
        SyntaxKind.TildeEqualsToken => "~=",
        SyntaxKind.LessEqualsToken => "<=",
        SyntaxKind.GreaterEqualsToken => ">=",
        SyntaxKind.LessToken => "<",
        SyntaxKind.GreaterToken => ">",
        SyntaxKind.EqualsToken => "=",
        SyntaxKind.OpenParenToken => "(",
        SyntaxKind.CloseParenToken => ")",
        SyntaxKind.OpenBraceToken => "{",
        SyntaxKind.CloseBraceToken => "}",
        SyntaxKind.OpenBracketToken => "[",
        SyntaxKind.CloseBracketToken => "]",
        SyntaxKind.DoubleColonToken => "::",
        SyntaxKind.SemicolonToken => ";",
        SyntaxKind.ColonToken => ":",
        SyntaxKind.CommaToken => ",",
        SyntaxKind.DotToken => ".",
        SyntaxKind.DotDotToken => "..",
        SyntaxKind.DotDotDotToken => "...",
        SyntaxKind.AndKeyword => "and",
        SyntaxKind.BreakKeyword => "break",
        SyntaxKind.DoKeyword => "do",
        SyntaxKind.ElseKeyword => "else",
        SyntaxKind.ElseifKeyword => "elseif",
        SyntaxKind.EndKeyword => "end",
        SyntaxKind.FalseKeyword => "false",
        SyntaxKind.ForKeyword => "for",
        SyntaxKind.FunctionKeyword => "function",
        SyntaxKind.GotoKeyword => "goto",
        SyntaxKind.IfKeyword => "if",
        SyntaxKind.InKeyword => "in",
        SyntaxKind.LocalKeyword => "local",
        SyntaxKind.NilKeyword => "nil",
        SyntaxKind.NotKeyword => "not",
        SyntaxKind.OrKeyword => "or",
        SyntaxKind.RepeatKeyword => "repeat",
        SyntaxKind.ReturnKeyword => "return",
        SyntaxKind.ThenKeyword => "then",
        SyntaxKind.TrueKeyword => "true",
        SyntaxKind.UntilKeyword => "until",
        SyntaxKind.WhileKeyword => "while",
        _ => string.Empty,
    };
}
