namespace cilua.CodeAnalysis;

/// <summary>
/// Wraps the raw source string and lets diagnostics/tokens map an offset back to a
/// human-readable (line, column). Kept intentionally simple compared to Roslyn's SourceText.
/// </summary>
public sealed class SourceText
{
    private readonly int[] _lineStarts;

    public string FileName { get; }
    public string Text { get; }

    private SourceText(string fileName, string text)
    {
        FileName = fileName;
        Text = text;
        _lineStarts = ComputeLineStarts(text);
    }

    public static SourceText From(string text, string fileName = "<chunk>") => new(fileName, text);

    public int Length => Text.Length;

    public char this[int index] => index < Text.Length ? Text[index] : '\0';

    public (int Line, int Column) GetLineColumn(int position)
    {
        var line = Array.BinarySearch(_lineStarts, position);
        if (line < 0)
        {
            line = ~line - 1;
        }
        line = Math.Max(line, 0);
        var column = position - _lineStarts[line];
        return (line + 1, column + 1); // 1-based
    }

    private static int[] ComputeLineStarts(string text)
    {
        var starts = new List<int> { 0 };
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                starts.Add(i + 1);
            }
        }
        return [.. starts];
    }
}
