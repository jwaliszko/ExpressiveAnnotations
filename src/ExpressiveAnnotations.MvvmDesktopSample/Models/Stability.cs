using System.ComponentModel.DataAnnotations;

namespace ExpressiveAnnotations.MvvmDesktopSample.Models
{
    public enum Stability
    {
        [Display(Name = "High")]
        High,
        [Display(Name = "Low")]
        Low,
        [Display(Name = "Uncertain")]
        Uncertain,
    }
}
