using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public class Contact
    {
        [RequiredIf("Email == null && Phone == null",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "EmailOrPhoneRequired")]
        [Display(ResourceType = typeof (Resources), Name = "Email")]
        public string Email { get; set; }

        [RequiredIf("Email == null && Phone == null",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "EmailOrPhoneRequired")]
        [Display(ResourceType = typeof (Resources), Name = "Phone")]
        public string Phone { get; set; }
    }
}
