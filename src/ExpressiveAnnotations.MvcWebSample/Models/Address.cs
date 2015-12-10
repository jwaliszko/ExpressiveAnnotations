using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public class Address
    {
        public string Type { get; set; }

        [AssertThat("StartsWith(Details, StreetPrefix)",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "AddressDetailsFormatInvalid")]
        public string Details { get; set; }

        public string StreetPrefix => Resources.StreetPrefix;
    }
}