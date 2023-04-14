using PKHeX.Core;
using Sharprompt;
using SpreadFinder;
using SpreadFinder.Enums;

Console.WriteLine("Hello, World!");

var fileType = Prompt.Select<FileType>("Select file type", defaultValue: FileType.PK9);
var path = Prompt.Input<string>("What's the path with PK9 files?");
if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
{
    Console.WriteLine($"Given directory [{path}] doesn't exist..");
    Console.ReadKey();
    return;
}

var result = Prompt.Bind<SearchCondition>();

var searchPattern = fileType switch
{
    FileType.PK9 => "*.pk9",
    FileType.EPK9 => "*.epk9",
    _ => throw new ArgumentOutOfRangeException()
};

Console.WriteLine($"Searching through [{searchPattern}]");

var results = new List<(PK9 Pk9, int Count)>();
foreach (var file in Directory.GetFiles(path, searchPattern))
{
    var bytes = await File.ReadAllBytesAsync(file);
    var pk9 = new PK9(bytes);

    var nature = result.Natures.Any(n => n is Nature.Random || n == (Nature)pk9.Nature);

    var hp = Match(pk9.IV_HP, result.HP);
    var atk = Match(pk9.IV_ATK, result.Atk);
    var def = Match(pk9.IV_DEF, result.Def);
    var spa = Match(pk9.IV_SPA, result.SpAtk);
    var spd = Match(pk9.IV_SPD, result.SpDef);
    var spe = Match(pk9.IV_SPE, result.Spe);

    if (nature && hp && atk && def && spa && spd && spe)
        results.Add((pk9, Count(pk9)));
}

Table.PrintLine();
Table.PrintRow("Species", "Nature", "HP", "Atk", "Def", "SpA", "SpD", "Spe", "Tot.", "Perf.");
Table.PrintLine();

var grouped = results
    .GroupBy(r => new { r.Pk9.Species, r.Pk9.Nature })
    .OrderBy(r => r.Key.Species)
    .ThenBy(r => r.Key.Nature);

foreach (var group in grouped)
{
    var pkms = group.OrderByDescending(g => g.Count);

    foreach (var (pk9, count) in pkms)
    {
        var values = new[]
        {
            $"{(Species)pk9.Species}",
            $"{(Nature)pk9.Nature}",
            $"{pk9.IV_HP}",
            $"{pk9.IV_ATK}",
            $"{pk9.IV_DEF}",
            $"{pk9.IV_SPA}",
            $"{pk9.IV_SPD}",
            $"{pk9.IV_SPE}",
            $"{count}",
            $"{Perfect(pk9)}"
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

static int Count(PKM pkm)
{
    return (Nature)pkm.Nature switch
    {
        Nature.Brave => pkm.IV_HP + pkm.IV_ATK + pkm.IV_DEF + pkm.IV_SPD - pkm.IV_SPE,
        Nature.Adamant => pkm.IV_HP + pkm.IV_ATK + pkm.IV_DEF + pkm.IV_SPD + pkm.IV_SPE,
        Nature.Bold => pkm.IV_HP - pkm.IV_ATK + pkm.IV_DEF + pkm.IV_SPA + pkm.IV_SPD + pkm.IV_SPE,
        Nature.Impish => pkm.IV_HP + pkm.IV_ATK + pkm.IV_DEF + pkm.IV_SPD + pkm.IV_SPE,
        Nature.Timid => pkm.IV_HP - pkm.IV_ATK + pkm.IV_DEF + pkm.IV_SPA + pkm.IV_SPD + pkm.IV_SPE,
        Nature.Jolly => pkm.IV_HP + pkm.IV_ATK + pkm.IV_DEF + pkm.IV_SPD + pkm.IV_SPE,
        Nature.Modest => pkm.IV_HP - pkm.IV_ATK + pkm.IV_DEF + pkm.IV_SPA + pkm.IV_SPD + pkm.IV_SPE,
        Nature.Calm => pkm.IV_HP - pkm.IV_ATK + pkm.IV_DEF + pkm.IV_SPA + pkm.IV_SPD + pkm.IV_SPE,
        Nature.Careful => pkm.IV_HP + pkm.IV_ATK + pkm.IV_DEF + pkm.IV_SPD + pkm.IV_SPE,
        _ => 0,
    };
}

static int Perfect(PKM pkm)
{
    return (Nature)pkm.Nature switch
    {
        Nature.Brave => Perfects(pkm.IV_HP, pkm.IV_ATK, pkm.IV_DEF, pkm.IV_SPD),
        Nature.Adamant => Perfects(pkm.IV_HP, pkm.IV_ATK, pkm.IV_DEF, pkm.IV_SPD, pkm.IV_SPE),
        Nature.Bold => Perfects(pkm.IV_HP, pkm.IV_DEF, pkm.IV_SPA, pkm.IV_SPD, pkm.IV_SPE),
        Nature.Impish => Perfects(pkm.IV_HP, pkm.IV_ATK, pkm.IV_DEF, pkm.IV_SPD, pkm.IV_SPE),
        Nature.Timid => Perfects(pkm.IV_HP, pkm.IV_DEF, pkm.IV_SPA, pkm.IV_SPD, pkm.IV_SPE),
        Nature.Jolly => Perfects(pkm.IV_HP, pkm.IV_ATK, pkm.IV_DEF, pkm.IV_SPD, pkm.IV_SPE),
        Nature.Modest => Perfects(pkm.IV_HP, pkm.IV_DEF, pkm.IV_SPA, pkm.IV_SPD, pkm.IV_SPE),
        Nature.Calm => Perfects(pkm.IV_HP, pkm.IV_DEF, pkm.IV_SPA, pkm.IV_SPD, pkm.IV_SPE),
        Nature.Careful => Perfects(pkm.IV_HP, pkm.IV_ATK, pkm.IV_DEF, pkm.IV_SPD, pkm.IV_SPE),
        _ => 0,
    };
}

static int Perfects(params int[] values)
{
    return values.Count(value => value == 31);
}