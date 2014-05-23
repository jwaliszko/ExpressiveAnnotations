using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.ConditionalAttributes;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public class Contact
    {
        [RequiredIfExpression(
            Expression = "{0} && {1}",
            DependentProperties = new[] {"Email", "Phone"},
            TargetValues = new object[] {null,    null},
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "EmailOrPhoneRequired")]
        [Display(ResourceType = typeof(Resources), Name = "Email")]
        public string Email { get; set; }

        [RequiredIfExpression(
            Expression = "{0} && {1}",
            DependentProperties = new[] {"Email", "Phone"},
            TargetValues = new object[] {null,    null},
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "EmailOrPhoneRequired")]
        [Display(ResourceType = typeof(Resources), Name = "Phone")]
        public string Phone { get; set; }
    }
}