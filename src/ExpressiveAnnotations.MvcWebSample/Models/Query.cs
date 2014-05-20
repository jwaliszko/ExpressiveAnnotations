using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        public IEnumerable<int?> Years
        {
            get { return new int?[] {null}.Concat(Enumerable.Range(18, 73).Select(x => (int?) x)); }
        }

        [Display(ResourceType = typeof (Resources), Name = "GoAbroad")]
        public bool GoAbroad { get; set; }

        [Required(ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof (Resources), Name = "Age")]
        public int? Age { get; set; }

        [RequiredIf(
            DependentProperty = "GoAbroad",
            TargetValue = true,
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FieldConditionallyRequired")]
        [Display(ResourceType = typeof (Resources), Name = "PassportNumber")]
        public string PassportNumber { get; set; }

        [Display(ResourceType = typeof (Resources), Name = "Country")]
        public string Country { get; set; }

        [Display(ResourceType = typeof (Resources), Name = "NextCountry")]
        public string NextCountry { get; set; }

        [RequiredIfExpression( /* interpretation => GoAbroad == true 
                                *                   && ( 
                                *                        (NextCountry != "Other" && NextCountry == value_from_country) 
                                *                        || Age ∈ (24, 55> 
                                *                      )
                                */
            Expression = "{0} && ( (!{1} && {2}) || ({3} && {4}) )",
            DependentProperties = new[] {"GoAbroad", "NextCountry", "NextCountry", "Age", "Age"},
            RelationalOperators = new[] {"==", "==", "==", ">", "<="},
            TargetValues = new object[] {true, "Other", "[Country]", 24, 55},
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "ReasonForTravelRequired")]
        [Display(ResourceType = typeof (Resources), Name = "ReasonForTravel")]
        public string ReasonForTravel { get; set; }

        public DateTime LatestSuggestedReturnDate { get; set; }

        [RequiredIf(
            DependentProperty = "GoAbroad",
            TargetValue = true,
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "FieldConditionallyRequired")]
        [Display(ResourceType = typeof(Resources), Name = "ReturnDate")]
        public DateTime? ReturnDate { get; set; }

        [RequiredIfExpression( /* interpretation => GoAbroad == true && ReturnDate > value_from_latest_suggested_return_date */
            Expression = "{0} && {1}",
            DependentProperties = new[] { "GoAbroad", "ReturnDate" },
            RelationalOperators = new[] { "==", ">" },
            TargetValues = new object[] { true, "[LatestSuggestedReturnDate]" },
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ReasonForLongTravelRequired")]
        [Display(ResourceType = typeof(Resources), Name = "ReasonForLongTravel")]
        public string ReasonForLongTravel { get; set; }

        [Display(ResourceType = typeof (Resources), Name = "PoliticalStability")]
        [RequiredIf(
            DependentProperty = "GoAbroad",
            RelationalOperator = "!=",
            TargetValue = false,
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FieldConditionallyRequired")]
        public Stability? PoliticalStability { get; set; }

        [RequiredIfExpression( /* interpretation => PoliticalStability != null && PoliticalStability != Stability.High */
            Expression = "!{0} && !{1}",
            DependentProperties = new[] {"PoliticalStability", "PoliticalStability"},
            TargetValues = new object[] {null, Stability.High},
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "AwareOfTheRisksRequired")]
        [Display(ResourceType = typeof (Resources), Name = "AwareOfTheRisks")]
        public bool AwareOfTheRisks { get; set; }

        [Display(ResourceType = typeof (Resources), Name = "SportType")]
        public string SportType { get; set; }

        [RequiredIfExpression( /* interpretation => SportType == "Extreme" || (SportType != "None" && GoAbroad == true) */
            Expression = "{0} || (!{1} && {2})",
            DependentProperties = new[] {"SportType", "SportType", "GoAbroad"},
            TargetValues = new object[] {"Extreme", "None", true},
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "BloodTypeRequired")]
        [Display(ResourceType = typeof (Resources), Name = "BloodType")]
        public string BloodType { get; set; }

        [RequiredIfExpression( /* interpretation => ContactDetails.Email == "*" || ContactDetails.Phone == "*" */
            Expression = "{0} || {1}",
            DependentProperties = new[] {"ContactDetails.Email", "ContactDetails.Phone"}, /* nested properties are supported */
            TargetValues = new object[] {"*", "*"}, /* any values */
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "AgreeForContactRequired")]
        [Display(ResourceType = typeof (Resources), Name = "AgreeForContact")]
        public bool AgreeForContact { get; set; }

        public Contact ContactDetails { get; set; }
    }
}