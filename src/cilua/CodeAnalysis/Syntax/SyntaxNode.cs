using System.Reflection;

namespace cilua.CodeAnalysis.Syntax;

/// <summary>
/// Base of every syntax node. Roslyn keeps a separate immutable "green" tree and
/// a "red" tree with parent pointers; for now a single node tree
/// with a Kind + reflective GetChildren() is enough to get the same shape of API
/// (SyntaxTree walkers, span-based diagnostics) without the extra machinery.
/// </summary>
public abstract class SyntaxNode
{
    public abstract SyntaxKind Kind { get; }

    public virtual TextSpan Span
    {
        get
        {
            var children = GetChildren().ToList();
            if (children.Count == 0)
            {
                return default;
            }
            var first = children.First();
            var last = children.Last();
            return TextSpan.FromBounds(GetSpanStart(first), GetSpanEnd(last));
        }
    }

    private static int GetSpanStart(SyntaxElement node) => node switch
    {
        SyntaxToken t => t.Span.Start,
        SyntaxNode n => n.Span.Start,
        _ => 0
    };
    private static int GetSpanEnd(SyntaxElement node) => node switch
    {
        SyntaxToken t => t.Span.End,
        SyntaxNode n => n.Span.End,
        _ => 0
    };

    public union SyntaxElement(SyntaxToken, SyntaxNode);

    /// <summary>
    /// Reflects over public properties to yield tokens/nodes/node-lists in declaration
    /// order — enough for a generic tree printer or walker without hand-writing
    /// GetChildren() on every node type.
    /// </summary>
    public IEnumerable<SyntaxElement> GetChildren()
    {
        var properties = GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(this);

            switch (value)
            {
                case SyntaxNode node:
                    yield return node;
                    break;

                case SyntaxToken token:
                    yield return token;
                    break;

                case IEnumerable<SyntaxNode> nodes:
                    foreach (var node in nodes)
                        yield return node;
                    break;

                case IEnumerable<SyntaxToken> tokens:
                    foreach (var token in tokens)
                        yield return token;
                    break;
            }
        }
    }
}
