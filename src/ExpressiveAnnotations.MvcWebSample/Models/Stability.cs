using System.ComponentModel.DataAnnotations;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public enum Stability
    {
        [Display(ResourceType = typeof(Resources), Name = "High")]
        High,
        [Display(ResourceType = typeof(Resources), Name = "Low")]
        Low,
        [Display(ResourceType = typeof(Resources), Name = "Uncertain")]
        Uncertain,
    }
}
