using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public class Query
    {
        public bool IsBloodType(string group)
        {
            return Regex.IsMatch(group, "(A|B|AB|0)[+-]");
        }

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

        public IEnumerable<SelectListItem> Answers
        {
            get
            {
                return new[]
                {
                    new SelectListItem {Text = string.Empty, Value = null},
                    new SelectListItem {Text = Resources.Yes, Value = true.ToString()},
                    new SelectListItem {Text = Resources.No, Value = false.ToString()}
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

        [RequiredIf("GoAbroad == true",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FieldConditionallyRequired")]        
        [Display(ResourceType = typeof (Resources), Name = "PassportNumber")]
        public string PassportNumber { get; set; }

        [Display(ResourceType = typeof (Resources), Name = "Country")]
        public string Country { get; set; }

        [Display(ResourceType = typeof (Resources), Name = "NextCountry")]
        public string NextCountry { get; set; }

        [RequiredIf("GoAbroad == true " +
                    "&& (" +
                            "(NextCountry != 'Other' && NextCountry == Country) " +
                            "|| (Age > 24 && Age <= 55)" +
                        ")",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "ReasonForTravelRequired")]
        [Display(ResourceType = typeof (Resources), Name = "ReasonForTravel")]
        public string ReasonForTravel { get; set; }

        [UIHint("ISO8601Date")]
        public DateTime LatestSuggestedReturnDate { get; set; }

        [RequiredIf("GoAbroad == true",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FieldConditionallyRequired")]
        [AssertThat("ReturnDate >= Today()",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FutureDateRequired")]
        [Display(ResourceType = typeof (Resources), Name = "ReturnDate")]
        public DateTime? ReturnDate { get; set; }

        [RequiredIf("GoAbroad == true && ReturnDate > LatestSuggestedReturnDate",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "ReasonForLongTravelRequired")]
        [Display(ResourceType = typeof (Resources), Name = "ReasonForLongTravel")]
        public string ReasonForLongTravel { get; set; }

        [RequiredIf("GoAbroad == true",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FieldConditionallyRequired")]
        [Display(ResourceType = typeof (Resources), Name = "PoliticalStability")]
        public Stability? PoliticalStability { get; set; }

        [RequiredIf("PoliticalStability != null && PoliticalStability != 0",                
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "AwareOfTheRisksRequired")]
        [Display(ResourceType = typeof (Resources), Name = "AwareOfTheRisks")]
        //[KnownEnum(typeof(Stability))] ToDo: implement this to give parser information about other types used by expression, like enums e.g. Stability.High (instead of 0 like above).
        public bool AwareOfTheRisks { get; set; }

        [Display(ResourceType = typeof (Resources), Name = "SportType")]
        public string SportType { get; set; }

        [RequiredIf("SportType == 'Extreme' || (SportType != 'None' && GoAbroad == true)",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "BloodTypeRequired")]
        [AssertThat("IsBloodType(BloodType)")]
        [Display(ResourceType = typeof (Resources), Name = "BloodType")]
        public string BloodType { get; set; }

        [RequiredIf("ContactDetails.Email != null || ContactDetails.Phone != null",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "AgreeForContactRequired")]
        [Display(ResourceType = typeof (Resources), Name = "AgreeForContact")]
        public bool AgreeForContact { get; set; }

        [RequiredIf("AgreeForContact == true && CompareOrdinal(ContactDetails.Email, ContactDetails.Phone) >= 0",
            AllowEmptyOrFalse = true,
            ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "ImmediateContactRequired")]
        [Display(ResourceType = typeof(Resources), Name = "ImmediateContact")]
        public bool? ImmediateContact { get; set; }

        public Contact ContactDetails { get; set; }
    }
}
