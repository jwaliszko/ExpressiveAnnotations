using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.ConditionalAttributes;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public class Contact
    {
        [RequiredIf("Email == null && Phone == null")]
        [Display(ResourceType = typeof(Resources), Name = "Email")]
        public string Email { get; set; }

        [RequiredIf("Email == null && Phone == null")]
        [Display(ResourceType = typeof(Resources), Name = "Phone")]
        public string Phone { get; set; }
    }
}