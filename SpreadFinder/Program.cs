using PKHeX.Core;
using Sharprompt;
using SpreadFinder;
using SpreadFinder.Enums;

Console.WriteLine("Hello, World!");

var fileType = Prompt.Select<FileType>("Select file type", defaultValue: FileType.PK9);
var path = Prompt.Input<string>("What's the path with PK files?");
if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
{
    Console.WriteLine($"Given directory [{path}] doesn't exist..");
    Console.ReadKey();
    return;
}

var result = Prompt.Bind<SearchCondition>();

var searchPattern = fileType switch
{
    FileType.PK8 => "*.pk8",
    FileType.EPK8 => "*.epk8",
    FileType.PK9 => "*.pk9",
    FileType.EPK9 => "*.epk9",
    _ => throw new ArgumentOutOfRangeException()
};

Console.WriteLine($"Searching through [{searchPattern}]");

var results = new List<(PKM Pk, int Count)>();
foreach (var file in Directory.GetFiles(path, searchPattern))
{
    var bytes = await File.ReadAllBytesAsync(file);

    PKM pk = fileType switch
    {
        FileType.PK8 or FileType.EPK8 => new PK8(bytes),
        FileType.PK9 or FileType.EPK9 => new PK9(bytes),
        _ => throw new ArgumentOutOfRangeException()
    };

    var nature = result.Natures.Any(n => n is Nature.Random || n == pk.Nature);

    var hp = Match(pk.IV_HP, result.HP);
    var atk = Match(pk.IV_ATK, result.Atk);
    var def = Match(pk.IV_DEF, result.Def);
    var spa = Match(pk.IV_SPA, result.SpAtk);
    var spd = Match(pk.IV_SPD, result.SpDef);
    var spe = Match(pk.IV_SPE, result.Spe);

    if (nature && hp && atk && def && spa && spd && spe)
        results.Add((pk, Count(pk, result.SortBy)));
}

Table.PrintLine();
Table.PrintRow("Species", "Nature", "HP", "Atk", "Def", "SpA", "SpD", "Spe", "Tot. IV", "Perf. IV");
Table.PrintLine();

var grouped = results
    .GroupBy(r => new { r.Pk.Species, r.Pk.Nature })
    .OrderBy(r => r.Key.Species)
    .ThenBy(r => r.Key.Nature);

foreach (var group in grouped)
{
    var pkms = group.OrderByDescending(g => g.Count);

    foreach (var (pk, count) in pkms)
    {
        var values = new[]
        {
            $"{(Species)pk.Species}",
            $"{pk.Nature}",
            $"{pk.IV_HP}",
            $"{pk.IV_ATK}",
            $"{pk.IV_DEF}",
            $"{pk.IV_SPA}",
            $"{pk.IV_SPD}",
            $"{pk.IV_SPE}",
            $"{count}",
            $"{Perfect(pk, result.SortBy)}"
        };

        Table.PrintRow(values);
    }

    Table.PrintLine();
}

Console.WriteLine("Finished...");
Console.ReadKey();

static bool Match(int stat, IV iv)
{
    return iv switch
    {
        IV.GreaterThanOrEqual30 => stat >= 30,
        IV.LowerThanOrEqual1 => stat <= 1,
        IV.LowerThanOrEqual10 => stat <= 10,
        IV.Ignore => true,
        _ => throw new ArgumentOutOfRangeException()
    };
}

static int Count(PKM pk, SortBy sortBy)
{
    return sortBy switch
    {
        SortBy.Nature => pk.Nature switch
        {
            Nature.Brave => pk.IV_HP + pk.IV_ATK + pk.IV_DEF + pk.IV_SPD - pk.IV_SPE,
            Nature.Adamant => pk.IV_HP + pk.IV_ATK + pk.IV_DEF + pk.IV_SPD + pk.IV_SPE,
            Nature.Bold => pk.IV_HP - pk.IV_ATK + pk.IV_DEF + pk.IV_SPA + pk.IV_SPD + pk.IV_SPE,
            Nature.Impish => pk.IV_HP + pk.IV_ATK + pk.IV_DEF + pk.IV_SPD + pk.IV_SPE,
            Nature.Timid => pk.IV_HP - pk.IV_ATK + pk.IV_DEF + pk.IV_SPA + pk.IV_SPD + pk.IV_SPE,
            Nature.Jolly => pk.IV_HP + pk.IV_ATK + pk.IV_DEF + pk.IV_SPD + pk.IV_SPE,
            Nature.Modest => pk.IV_HP - pk.IV_ATK + pk.IV_DEF + pk.IV_SPA + pk.IV_SPD + pk.IV_SPE,
            Nature.Calm => pk.IV_HP - pk.IV_ATK + pk.IV_DEF + pk.IV_SPA + pk.IV_SPD + pk.IV_SPE,
            Nature.Careful => pk.IV_HP + pk.IV_ATK + pk.IV_DEF + pk.IV_SPD + pk.IV_SPE,
            Nature.Naive => pk.IV_HP + pk.IV_ATK + pk.IV_DEF + pk.IV_SPA + pk.IV_SPD + pk.IV_SPE,
            Nature.Hasty => pk.IV_HP + pk.IV_ATK + pk.IV_DEF + pk.IV_SPA + pk.IV_SPD + pk.IV_SPE,
            _ => 0
        },
        SortBy.Physical => pk.IV_HP + pk.IV_ATK + pk.IV_DEF + pk.IV_SPD + pk.IV_SPE,
        SortBy.PhysicalTrickRoom => pk.IV_HP + pk.IV_ATK + pk.IV_DEF + pk.IV_SPD - pk.IV_SPE,
        SortBy.Special => pk.IV_HP - pk.IV_ATK + pk.IV_DEF + pk.IV_SPA + pk.IV_SPD + pk.IV_SPE,
        SortBy.SpecialTrickRoom => pk.IV_HP - pk.IV_ATK + pk.IV_DEF + pk.IV_SPA + pk.IV_SPD - pk.IV_SPE,
        _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
    };
}

static int Perfect(PKM pk, SortBy sortBy)
{
    return sortBy switch
    {
        SortBy.Nature => pk.Nature switch
        {
            Nature.Brave => Perfects(pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPD),
            Nature.Adamant => Perfects(pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPD, pk.IV_SPE),
            Nature.Bold => Perfects(pk.IV_HP, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD, pk.IV_SPE),
            Nature.Impish => Perfects(pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPD, pk.IV_SPE),
            Nature.Timid => Perfects(pk.IV_HP, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD, pk.IV_SPE),
            Nature.Jolly => Perfects(pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPD, pk.IV_SPE),
            Nature.Modest => Perfects(pk.IV_HP, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD, pk.IV_SPE),
            Nature.Calm => Perfects(pk.IV_HP, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD, pk.IV_SPE),
            Nature.Careful => Perfects(pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPD, pk.IV_SPE),
            Nature.Naive => Perfects(pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD, pk.IV_SPE),
            Nature.Hasty => Perfects(pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD, pk.IV_SPE),
            _ => 0
        },
        SortBy.Physical => Perfects(pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPD, pk.IV_SPE),
        SortBy.PhysicalTrickRoom => Perfects(pk.IV_HP, pk.IV_ATK, pk.IV_DEF, pk.IV_SPD),
        SortBy.Special => Perfects(pk.IV_HP, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD, pk.IV_SPE),
        SortBy.SpecialTrickRoom => Perfects(pk.IV_HP, pk.IV_DEF, pk.IV_SPA, pk.IV_SPD),
        _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
    };
}

static int Perfects(params int[] values)
{
    return values.Count(value => value == 31);
}
