using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public class Address
    {
        public string Type { get; set; }

        [AssertThat("StartsWith(Details, StreetPrefix)", Priority = 1,
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "AddressDetailsFormatInvalid")]
        [AssertThat("Length(Trim(Details)) > Length(StreetPrefix) + 1", Priority = 2,
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "AddressDetailsTooShort")]
        public string Details { get; set; }

        public string StreetPrefix => Resources.StreetPrefix;
    }
}