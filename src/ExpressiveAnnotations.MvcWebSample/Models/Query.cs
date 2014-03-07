using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
                        new SelectListItem {Text = Resources.None, Value = "None"},
                        new SelectListItem {Text = Resources.Normal, Value = "Normal"},
                        new SelectListItem {Text = Resources.Extreme, Value = "Extreme"}
                    };
            }
        }

        public IEnumerable<SelectListItem> Countries
        {
            get
            {
                return new[]
                    {
                        new SelectListItem {Text = Resources.Poland, Value = "Poland"},
                        new SelectListItem {Text = Resources.Germany, Value = "Germany"},
                        new SelectListItem {Text = Resources.France, Value = "France"},
                        new SelectListItem {Text = Resources.Other, Value = "Other"}
                    };
            }
        }

        [Display(ResourceType = typeof(Resources), Name = "GoAbroad")]
        public bool GoAbroad { get; set; }
        
        [RequiredIf(DependentProperty = "GoAbroad", TargetValue = true, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof(Resources), Name = "PassportNumber")]
        public string PassportNumber { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Country")]
        public string Country { get; set; }
        [Display(ResourceType = typeof(Resources), Name = "NextCountry")]
        public string NextCountry { get; set; }

        [RequiredIfExpression(  /* interpretation => GoAbroad == true && NextCountry != "Other" && NextCountry == [value from Country] */
            Expression = "{0} && !{1} && {2}",
            DependentProperties = new[] { "GoAbroad", "NextCountry", "NextCountry" },
            TargetValues = new object[] { true, "Other", "[Country]" },
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ReasonForTravelRequired")]
        [Display(ResourceType = typeof(Resources), Name = "ReasonForTravel")]        
        public string ReasonForTravel { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "SportType")]
        public string SportType { get; set; }

        [RequiredIfExpression(  /* interpretation => SportType == "Extreme" || (SportType != "None" && GoAbroad == true) */
            Expression = "{0} || (!{1} && {2})",
            DependentProperties = new[] {"SportType", "SportType", "GoAbroad"},
            TargetValues = new object[] {"Extreme", "None", true},
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "BloodTypeRequired")]
        [Display(ResourceType = typeof(Resources), Name = "BloodType")]
        public string BloodType { get; set; }

        [RequiredIfExpression(
            Expression = "{0} || {1}",
            DependentProperties = new[] { "ContactDetails.Email", "ContactDetails.Phone" },   /* nested properties are supported */
            TargetValues = new object[] { "*", "*" }, /* any values */
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "AgreeToContactRequired")]
        [Display(ResourceType = typeof(Resources), Name = "AgreeToContact")]        
        public bool AgreeToContact { get; set; }

        public Contact ContactDetails { get; set; }
    }
}