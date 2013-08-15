using ExpressiveAnnotations.ConditionalAttributes;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public class Contact
    {
        [RequiredIfExpression(
            Expression = "{0} && {1}",
            DependentProperties = new[] {"Email", "Phone"},
            TargetValues = new object[] {null, null},
            ErrorMessage = "You do not enter any contact information. At least email or phone should be provided.")]
        public string Email { get; set; }

        [RequiredIfExpression(
            Expression = "{0} && {1}",
            DependentProperties = new[] {"Email", "Phone"},
            TargetValues = new object[] {null, null},
            ErrorMessage = "You do not enter any contact information. At least email or phone should be provided.")]
        public string Phone { get; set; }
    }
}