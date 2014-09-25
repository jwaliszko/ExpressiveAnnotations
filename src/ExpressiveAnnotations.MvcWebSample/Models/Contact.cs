using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public class Contact
    {
        [RequiredIf("Phone == null",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "EmailOrPhoneRequired")]
        [AssertThat("IsEmail(Email)",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "EmailFormatInvalid")]
        [Display(ResourceType = typeof (Resources), Name = "Email")]
        public string Email { get; set; }

        [RequiredIf("Email == null",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "EmailOrPhoneRequired")]
        [AssertThat(@"IsRegexMatch(Phone, '^\\d+$')", // regex pattern escaped despite verbatim string - it's because our expressive language parser
                                                      // verbatim syntax should be perfectly valid with that one JavaScript accepts 
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "DigitsOnlyAccepted")]
        [AssertThat("Length(Phone) > 8 && Length(Phone) < 16",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "RangeViolated")]
        [Display(ResourceType = typeof (Resources), Name = "Phone")]
        public string Phone { get; set; }
    }
}
