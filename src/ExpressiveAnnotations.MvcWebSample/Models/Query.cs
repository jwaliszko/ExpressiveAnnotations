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
        
        public bool GoAbroad { get; set; }

        [RequiredIf(DependentProperty = "GoAbroad", TargetValue = true)]
        public string PassportNumber { get; set; }

        public string SportType { get; set; }

        [RequiredIfExpression(  /* interpretation => SportType == "Extreme" || (SportType != "None" && GoAbroad == true) */
            Expression = "{0} || (!{1} && {2})",
            DependentProperties = new[] { "SportType", "SportType", "GoAbroad" },
            TargetValues = new object[] { "Extreme", "None", true },
            ErrorMessage = "Blood type is required if you do extreme sports, or if you do any type of sport and plan to go abroad.")]
        public string BloodType { get; set; }
    }
}