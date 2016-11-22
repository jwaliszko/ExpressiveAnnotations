using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public class Contact
    {
        public Contact()
        {
            Addresses = new List<Address> { new Address { Type = Resources.HomeAddress }, new Address { Type = Resources.OfficeAddress } };
        }

        [RequiredIf("Phone == null",
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.EmailOrPhoneRequired))]
        [AssertThat("IsEmail(Email)",
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.EmailFormatInvalid))]
        [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Email))]
        public string Email { get; set; }

        [RequiredIf("Email == null",
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.EmailOrPhoneRequired))]
        //[AssertThat("IsPhone(Phone)")]
        [AssertThat(@"IsRegexMatch(Phone, '^\\d+$')", // regex pattern escaped despite verbatim string - it's because our expressive language parser
                                                      // verbatim syntax should be perfectly valid with that one JavaScript accepts
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.DigitsOnlyAccepted), Priority = 1)]
        [AssertThat("Length(Phone) > 8 && Length(Phone) < 16",
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.RangeViolated), Priority = 2)]
        [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Phone))]
        public string Phone { get; set; }

        public List<Address> Addresses { get; set; }
    }
}
