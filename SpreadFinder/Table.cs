namespace SpreadFinder;

public static class Table
{
    private const int TableWidth = 101;

    public static void PrintLine()
    {
        Console.WriteLine(new string('-', TableWidth));
    }

    public static void PrintEmptyLine()
    {
        Console.WriteLine();
    }

    public static void PrintRow(params string[] columns)
    {
        int width = (TableWidth - columns.Length) / columns.Length;
        string row = "|";

        foreach (string column in columns)
        {
            row += AlignCentre(column, width) + "|";
        }

        Console.WriteLine(row);
    }

    private static string AlignCentre(string text, int width)
    {
        text = text.Length > width
            ? string.Concat(text.AsSpan(0, width - 3), "...")
            : text;

        return string.IsNullOrEmpty(text)
            ? new string(' ', width)
            : text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
    }
}