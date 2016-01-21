using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using ExpressiveAnnotations.Attributes;
using ExpressiveAnnotations.MvcWebSample.Inheritance;

namespace ExpressiveAnnotations.MvcWebSample.Models
{
    public class Query
    {
        public const string SIMONS_CAT = @"Simon's cat named ""\\""
 (Double Backslash)";

        public Query()
        {
            ContactDetails = new Contact();
        }

        public IEnumerable<SelectListItem> Sports => new[]
        {
            new SelectListItem {Text = Resources.None, Value = "None"},
            new SelectListItem {Text = Resources.Normal, Value = "Normal"},
            new SelectListItem {Text = Resources.Extreme, Value = "Extreme"}
        };

        public IEnumerable<SelectListItem> Countries => new[]
        {
            new SelectListItem {Text = Resources.Poland, Value = "Poland"},
            new SelectListItem {Text = Resources.Germany, Value = "Germany"},
            new SelectListItem {Text = Resources.France, Value = "France"},
            new SelectListItem {Text = Resources.Other, Value = "Other"}
        };

        public IEnumerable<SelectListItem> Answers => new[]
        {
            new SelectListItem {Text = string.Empty, Value = null},
            new SelectListItem {Text = Resources.Yes, Value = true.ToString()},
            new SelectListItem {Text = Resources.No, Value = false.ToString()}
        };

        public IEnumerable<SelectListItem> Flights => new[]
        {
            new SelectListItem {Text = string.Empty, Value = Guid.Empty.ToString()},
            new SelectListItem {Text = "58776d02-7028-4299-81f5-db234c44b294", Value = "58776d02-7028-4299-81f5-db234c44b294"},
            new SelectListItem {Text = "8b7ee575-eeaa-441c-811f-db6eaacc7115", Value = "8b7ee575-eeaa-441c-811f-db6eaacc7115"},
            new SelectListItem {Text = "6b403167-5792-4871-bdf3-cdce8e9b90c0", Value = "6b403167-5792-4871-bdf3-cdce8e9b90c0"}
        };

        public IEnumerable<int?> Years => Enumerable.Range(15, 82).Cast<int?>();

        [UIHint("IntArray")]
        public int[] EarlyYears => new[] {15, 16, 17};

        public int[] AvailableDonations => new[] {1, 4, 9, 16, 25, 36, 49};

        public TimeSpan WeekPeriod => new TimeSpan(7, 0, 0, 0);

        [Display(ResourceType = typeof (Resources), Name = "GoAbroad")]
        public bool GoAbroad { get; set; }
        
        [Required(ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FieldRequired")]
        [Display(ResourceType = typeof (Resources), Name = "Age")]
        public int? Age { get; set; }

        [RequiredIf("GoAbroad == true",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FieldConditionallyRequired")]
        [AssertThat("IsDigitChain(PassportNumber)",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "DigitsOnlyAccepted")]
        [Display(ResourceType = typeof (Resources), Name = "PassportNumber")]
        public string PassportNumber { get; set; }

        [Display(ResourceType = typeof (Resources), Name = "Country")]
        public string Country { get; set; }

        [Display(ResourceType = typeof (Resources), Name = "NextCountry")]
        public string NextCountry { get; set; }

        [RequiredIf(@"GoAbroad == true
                      && (
                             (NextCountry != 'Other' && NextCountry == Country)
                             || (Age > 24 && Age <= 55)
                         )",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "ReasonForTravelRequired")]
        [RequiredIf("ArrayContains(Age, EarlyYears)",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "ReasonForTravelRequiredForYouth")]
        [AssertThat(@"ReasonForTravel != 'John\'s cat named ""\\\'""\n (Backslash Quote)' && ReasonForTravel != SIMONS_CAT",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "SecretAnswerDetected")]
        [Display(ResourceType = typeof (Resources), Name = "ReasonForTravel")]
        public string ReasonForTravel { get; set; }

        [UIHint("ISO8601Date")]
        [ValueParser("NonStandardDateParser")] // serialized DOM field value, when language is set to polish, is given in yyyy-mm-dd format -
                                               // - because built-in client-side logic cannot handle such a format, custom parser is indicated for use by this attribute
        public DateTime LatestSuggestedReturnDate { get; set; }

        [RequiredIf("GoAbroad == true",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FieldConditionallyRequired")]
        [AssertThat("ReturnDate >= Today()", Priority = 1, // to be invoked firstly
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FutureDateRequired")]
        [AssertThat("ReturnDate >= Today() + WeekPeriod", Priority = 2, // to be invoked secondly
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "MoreThanAWeekRequired")]
        [AssertThat("ReturnDate < AddYears(Today(), 1)",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "NoMoreThanAYear")]
        [Display(ResourceType = typeof (Resources), Name = "ReturnDate")]
        [ValueParser("NonStandardDateParser")]
        public DateTime? ReturnDate { get; set; }

        [RequiredIf("GoAbroad == true && ReturnDate > LatestSuggestedReturnDate", AllowEmptyStrings = true,
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "ReasonForLongTravelRequired")]
        [Display(ResourceType = typeof (Resources), Name = "ReasonForLongTravel")]
        public string ReasonForLongTravel { get; set; }

        [RequiredIf("GoAbroad == true",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FieldConditionallyRequired")]
        [Display(ResourceType = typeof (Resources), Name = "PoliticalStability")]
        public Stability? PoliticalStability { get; set; }

        [AssertThat(@"(
                          AwareOfTheRisks == true
                          && (PoliticalStability == Stability.Low || PoliticalStability == Stability.Uncertain)
                      ) 
                      || PoliticalStability == null || PoliticalStability == Stability.High",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "AwareOfTheRisksRequired")]
        [Display(ResourceType = typeof (Resources), Name = "AwareOfTheRisks")]
        public bool AwareOfTheRisks { get; set; }

        [Display(ResourceType = typeof (Resources), Name = "SportType")]
        public string SportType { get; set; }

        [RequiredIf("SportType == 'Extreme' || (SportType != 'None' && GoAbroad == true)",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "BloodTypeRequired")]
        [AssertThat("IsBloodType(Trim(BloodType))",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "BloodTypeInvalid")]
        [Display(ResourceType = typeof (Resources), Name = "BloodType")]
        public string BloodType { get; set; }

        [AssertThat("AgreeForContact == true",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "AgreeForContactRequired")]
        [Display(ResourceType = typeof (Resources), Name = "AgreeForContact")]
        public bool AgreeForContact { get; set; }

        [RequiredIf(@"AgreeForContact == true
                      && (ContactDetails.Email != null || ContactDetails.Phone != null)
                      && (ContactDetails.Addresses[0].Details != null || ContactDetails.Addresses[1].Details != null)",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "ImmediateContactRequired")]
        [Display(ResourceType = typeof (Resources), Name = "ImmediateContact")]
        public bool? ImmediateContact { get; set; }

        [AssertThat(@"FlightId != Guid('00000000-0000-0000-0000-000000000000') || GoAbroad == false",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FlightIdentifierInvalid")]        
        [Display(ResourceType = typeof (Resources), Name = "FlightId")]
        public Guid FlightId { get; set; }

        [RequiredIf("GoAbroad == true",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "FieldConditionallyRequired")]
        [AssertThat("ArrayLength(SelectedDonations) > 1",
            ErrorMessageResourceType = typeof (Resources), ErrorMessageResourceName = "NotEnoughDonations")]
        [ValueParser("ArrayParser")]
        [Display(ResourceType = typeof (Resources), Name = "Donation")]
        public int[] SelectedDonations { get; set; }

        [CustomizedRequiredIf("GoAbroad == true")]
        [CustomizedAssertThat("Length(コメント) > 1e1 - 1")]
        [Display(ResourceType = typeof (Resources), Name = "Comment")]
        public string コメント { get; set; }

        public Contact ContactDetails { get; set; }

        public bool IsBloodType(string group)
        {
            return Regex.IsMatch(group, @"^(A|B|AB|0)[\+-]$"); /* Reminder: verbatim string usage is adviced when working with regex patterns. Regex patterns are full of backslashes, 
                                                                * and backslash characters need to be escaped in regular string literals. Verbatim string literal, on the other hand, 
                                                                * is such a string which does not need to be escaped. It is treated literally by the compiler, since compiler does not 
                                                                * interpret backslash control characters anymore - they lose any special significance for it (one thing to remember is 
                                                                * usage of "" for quote escape sequence: http://msdn.microsoft.com/en-us/library/aa691090(v=vs.71).aspx).
                                                                */
        }

        public DateTime AddYears(DateTime from, int years)
        {
            return from.AddYears(years);
        }

        public bool ArrayContains(int? value, int[] array)
        {
            return value != null && array.Contains((int)value);
        }

        public int ArrayLength(int[] array)
        {
            return array.Length;
        }        
    }
}
