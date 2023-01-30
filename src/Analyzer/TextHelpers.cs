namespace Analyzer;

public static class TextHelpers
{
    public static ReadOnlySpan<char> GetLastToken(ref ReadOnlySpan<char> text, char delimiter)
    {
        int index = text.LastIndexOf(delimiter);
        ReadOnlySpan<char> result;
        if (index < 0)
        {
            result = text.Slice(0);
            text = ReadOnlySpan<char>.Empty;
        }
        else
        {
            result = text.Slice(index + 1);
            text = text.Slice(0, index);
        }

        return result;
    }
}