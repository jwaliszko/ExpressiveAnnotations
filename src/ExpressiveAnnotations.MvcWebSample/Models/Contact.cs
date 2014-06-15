using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public class Contact
    {
        [RequiredIf(
            DependentProperty = "Phone",
            TargetValue = null,
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "EmailOrPhoneRequired")]
        [Display(ResourceType = typeof(Resources), Name = "Email")]
        public string Email { get; set; }

        [RequiredIf(
            DependentProperty = "Email",
            TargetValue = null,
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "EmailOrPhoneRequired")]
        [Display(ResourceType = typeof(Resources), Name = "Phone")]
        public string Phone { get; set; }
    }
}