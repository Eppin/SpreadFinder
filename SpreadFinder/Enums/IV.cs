using System.ComponentModel.DataAnnotations;

namespace SpreadFinder;

public enum IV
{
    [Display(Name = ">= 30")] GreaterThanOrEqual30,
    [Display(Name = "<= 1")] LowerThanOrEqual1,
    [Display(Name = "<= 10")] LowerThanOrEqual10,
    [Display(Name = "Ignore")] Ignore,
}