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
            get { return new int?[] { null }.Concat(Enumerable.Range(18, 73).Select(x => (int?)x)); }
        }

        [Display(ResourceType = typeof(Resources), Name = "GoAbroad")]
        public bool GoAbroad { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof(Resources), Name = "Age")]
        public int? Age { get; set; }

        [RequiredIf("GoAbroad == true")]
        [Display(ResourceType = typeof(Resources), Name = "PassportNumber")]
        public string PassportNumber { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "Country")]
        public string Country { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "NextCountry")]
        public string NextCountry { get; set; }

        [RequiredIf("GoAbroad == true " +
                    "&& (" +
                        "(NextCountry != 'Other' && NextCountry == Country) " +
                        "|| (Age > 24 && Age <= 55)" +
                    ")")]
        [Display(ResourceType = typeof(Resources), Name = "ReasonForTravel")]
        public string ReasonForTravel { get; set; }

        [UIHint("ISO8601Date")]
        public DateTime LatestSuggestedReturnDate { get; set; }
        [UIHint("ISO8601Date")]
        public DateTime Today { get; set; }

        [RequiredIf("GoAbroad == true")]
        [AssertThat("ReturnDate >= Today")]
        [Display(ResourceType = typeof(Resources), Name = "ReturnDate")]
        public DateTime? ReturnDate { get; set; }

        [RequiredIf("GoAbroad == true && ReturnDate > LatestSuggestedReturnDate")]
        [Display(ResourceType = typeof(Resources), Name = "ReasonForLongTravel")]
        public string ReasonForLongTravel { get; set; }

        [RequiredIf("GoAbroad == true")]
        [Display(ResourceType = typeof(Resources), Name = "PoliticalStability")]
        public Stability? PoliticalStability { get; set; }

        [RequiredIf("PoliticalStability != null && PoliticalStability != 0")]
        [Display(ResourceType = typeof(Resources), Name = "AwareOfTheRisks")]
        public bool AwareOfTheRisks { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "SportType")]
        public string SportType { get; set; }

        [RequiredIf("SportType == 'Extreme' || (SportType != 'None' && GoAbroad == true)")]
        [Display(ResourceType = typeof(Resources), Name = "BloodType")]
        public string BloodType { get; set; }

        [RequiredIf("ContactDetails.Email != null || ContactDetails.Phone != null")]
        [Display(ResourceType = typeof(Resources), Name = "AgreeForContact")]
        public bool AgreeForContact { get; set; }

        public Contact ContactDetails { get; set; }
    }
}