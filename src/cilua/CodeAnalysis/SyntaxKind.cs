namespace cilua.CodeAnalysis;

public enum SyntaxKind
{
    // ---- Special ----
    BadToken,
    EndOfFileToken,

    // ---- Trivia ----
    WhitespaceTrivia,
    LineCommentTrivia,
    BlockCommentTrivia,
    LineBreakTrivia,

    // ---- Literals / names ----
    NumberToken,
    StringToken,
    IdentifierToken,

    // ---- Keywords ----
    AndKeyword,
    BreakKeyword,
    DoKeyword,
    ElseKeyword,
    ElseifKeyword,
    EndKeyword,
    FalseKeyword,
    ForKeyword,
    FunctionKeyword,
    GotoKeyword,
    IfKeyword,
    InKeyword,
    LocalKeyword,
    NilKeyword,
    NotKeyword,
    OrKeyword,
    RepeatKeyword,
    ReturnKeyword,
    ThenKeyword,
    TrueKeyword,
    UntilKeyword,
    WhileKeyword,

    // ---- Punctuation / operators ----
    PlusToken,              // +
    MinusToken,             // -
    StarToken,              // *
    SlashToken,             // /
    DoubleSlashToken,       // //
    PercentToken,           // %
    CaretToken,             // ^
    HashToken,              // #
    AmpersandToken,         // &
    TildeToken,             // ~   (unary bnot, binary bxor)
    PipeToken,              // |
    LessLessToken,          // <<
    GreaterGreaterToken,    // >>
    EqualsEqualsToken,      // ==
    TildeEqualsToken,       // ~=
    LessEqualsToken,        // <=
    GreaterEqualsToken,     // >=
    LessToken,              // <
    GreaterToken,           // >
    EqualsToken,            // =
    OpenParenToken,         // (
    CloseParenToken,        // )
    OpenBraceToken,         // {
    CloseBraceToken,        // }
    OpenBracketToken,       // [
    CloseBracketToken,      // ]
    DoubleColonToken,       // ::
    SemicolonToken,         // ;
    ColonToken,             // :
    CommaToken,             // ,
    DotToken,               // .
    DotDotToken,            // ..
    DotDotDotToken,         // ...

    // ---- Syntax nodes: top level ----
    Chunk,
    Block,

    // ---- Syntax nodes: statements ----
    LocalDeclarationStatement,
    AssignmentStatement,
    CallStatement,
    DoStatement,
    WhileStatement,
    RepeatStatement,
    IfStatement,
    ElseifClause,
    ElseClause,
    NumericForStatement,
    GenericForStatement,
    FunctionDeclarationStatement,
    LocalFunctionDeclarationStatement,
    ReturnStatement,
    BreakStatement,
    GotoStatement,
    LabelStatement,
    EmptyStatement,

    // ---- Syntax nodes: expressions ----
    NilLiteralExpression,
    TrueLiteralExpression,
    FalseLiteralExpression,
    NumberLiteralExpression,
    StringLiteralExpression,
    VarargExpression,
    NameExpression,
    ParenthesizedExpression,
    FunctionExpression,
    TableConstructorExpression,
    BinaryExpression,
    UnaryExpression,
    MemberAccessExpression,     // a.b
    IndexExpression,            // a[b]
    CallExpression,             // f(args)
    MethodCallExpression,       // a:m(args)

    // ---- Supporting nodes ----
    Parameter,
    ParameterList,
    Argument,
    ArgumentList,
    FunctionBody,
    NameList,
    TableField,
    FunctionNamePath // a.b.c or a.b:c
}
