using System.Collections.Generic;
using System.Web.Mvc;
using ExpressiveAnnotations.ConditionalAttributes;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public class Query
    {
        public IEnumerable<SelectListItem> Sports
        {
            get
            {
                return new[]
                    {
                        new SelectListItem {Text = "None", Value = "None"},
                        new SelectListItem {Text = "Normal", Value = "Normal"},
                        new SelectListItem {Text = "Extreme", Value = "Extreme"}
                    };
            }
        }

        public IEnumerable<SelectListItem> Countries
        {
            get
            {
                return new[]
                    {
                        new SelectListItem {Text = "Poland", Value = "Poland"},
                        new SelectListItem {Text = "Germany", Value = "Germany"},
                        new SelectListItem {Text = "France", Value = "France"},
                        new SelectListItem {Text = "Other", Value = "Other"}
                    };
            }
        }

        public bool GoAbroad { get; set; }

        [RequiredIf(DependentProperty = "GoAbroad", TargetValue = true)]
        public string PassportNumber { get; set; }

        public string Country { get; set; }
        public string NextCountry { get; set; }

        [RequiredIfExpression(  /* interpretation => GoAbroad == true && NextCountry != "Other" && NextCountry == [value from Country] */
            Expression = "{0} && !{1} && {2}",
            DependentProperties = new[] {"GoAbroad", "NextCountry", "NextCountry"},
            TargetValues = new object[] {true, "Other", "[Country]"},
            ErrorMessage = "If you plan to go abroad, why do you want to visit the same country twice?")]
        public string ReasonForTravel { get; set; }

        public string SportType { get; set; }

        [RequiredIfExpression(  /* interpretation => SportType == "Extreme" || (SportType != "None" && GoAbroad == true) */
            Expression = "{0} || (!{1} && {2})",
            DependentProperties = new[] {"SportType", "SportType", "GoAbroad"},
            TargetValues = new object[] {"Extreme", "None", true},
            ErrorMessage = "Blood type is required if you do extreme sports, or if you do any type of sport and plan to go abroad.")]
        public string BloodType { get; set; }

        [RequiredIfExpression(
            Expression = "{0} || {1}",
            DependentProperties = new[] {"ContactDetails.Email", "ContactDetails.Phone"},   /* nested properties are supported */
            TargetValues = new object[] {"*", "*"}, /* any values */
            ErrorMessage = "You have to authorize us to make contact.")]
        public bool AgreeToContact { get; set; }

        public Contact ContactDetails { get; set; }
    }
}