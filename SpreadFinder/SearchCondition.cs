using System.ComponentModel.DataAnnotations;
using PKHeX.Core;

namespace SpreadFinder;

using Enums;

public class SearchCondition
{
    [Display(Name = "Nature"), Required] public IEnumerable<Nature> Natures { get; set; } = new List<Nature>();
    [Display(Name = "SortBy"), Required] public SortBy SortBy { get; set; } = SortBy.Nature;
    [Display(Name = "HP"), Required] public IV HP { get; set; } = IV.Ignore;
    [Display(Name = "Attack"), Required] public IV Atk { get; set; } = IV.Ignore;
    [Display(Name = "Defense"), Required] public IV Def { get; set; } = IV.Ignore;
    [Display(Name = "SpAtk"), Required] public IV SpAtk { get; set; } = IV.Ignore;
    [Display(Name = "SpDef"), Required] public IV SpDef { get; set; } = IV.Ignore;
    [Display(Name = "Speed"), Required] public IV Spe { get; set; } = IV.Ignore;
}
