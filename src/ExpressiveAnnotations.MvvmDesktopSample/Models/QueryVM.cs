using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvvmDesktopSample.Models
{
    public enum Stability
    {
        [Display(Name = "High")]
        High,
        [Display(Name = "Low")]
        Low,
        [Display(Name = "Uncertain")]
        Uncertain,
    }

    public class QueryVM : ExpressiveVM
    {
        public const string SIMONS_CAT = @"Simon's cat named ""\\""
 (Double Backslash)";

        private int? _age;
        private bool _agreeForContact;
        private bool _awareOfTheRisks;
        private string _bloodType;
        private string _country;
        private Guid? _flightId;
        private bool _goAbroad;
        private bool? _immediateContact;
        private string _nextCountry;
        private string _passportNumber;
        private Stability? _politicalStability;
        private string _reasonForLongTravel;
        private string _reasonForTravel;
        private DateTime? _returnDate;
        private string _sportType;

        public QueryVM()
        {
            GoAbroad = true;
            Country = "Poland";
            NextCountry = "Other";
            SportType = "Extreme";
            AgreeForContact = false;
            LatestSuggestedReturnDate = DateTime.Today.AddMonths(1);
            ContactDetails = new ContactVM();
        }

        public IEnumerable<KeyValuePair<string, string>> Countries => new[]
        {
            new KeyValuePair<string, string>("Poland", "Poland"),
            new KeyValuePair<string, string>("Germany", "Germany"),
            new KeyValuePair<string, string>("France", "France"),
            new KeyValuePair<string, string>("Other", "Other")
        };

        public IEnumerable<KeyValuePair<string, bool?>> Answers => new[]
        {
            new KeyValuePair<string, bool?>(string.Empty, null),
            new KeyValuePair<string, bool?>("Yes", true),
            new KeyValuePair<string, bool?>("No", false)
        };

        public IEnumerable<KeyValuePair<string, int?>> Years
        {
            get
            {
                return new[] {new KeyValuePair<string, int?>(string.Empty, null)}
                    .Concat(Enumerable.Range(18, 73)
                        .Select(x => new KeyValuePair<string, int?>(x.ToString(CultureInfo.InvariantCulture), x)));
            }
        }

        public bool GoAbroad
        {
            get { return _goAbroad; }
            set
            {
                _goAbroad = value;
                OnPropertyChanged();
            }
        }

        [Required]
        public int? Age
        {
            get { return _age; }
            set
            {
                _age = value;
                OnPropertyChanged();
            }
        }

        [RequiredIf("GoAbroad == true")]
        [AssertThat("IsDigitChain(PassportNumber)")]
        public string PassportNumber
        {
            get { return _passportNumber; }
            set
            {
                _passportNumber = value;
                OnPropertyChanged();
            }
        }

        public string Country
        {
            get { return _country; }
            set
            {
                _country = value;
                OnPropertyChanged();
            }
        }

        public string NextCountry
        {
            get { return _nextCountry; }
            set
            {
                _nextCountry = value;
                OnPropertyChanged();
            }
        }

        [RequiredIf(@"
GoAbroad == true
&& (
        (NextCountry != 'Other' && NextCountry == Country)
        || (Age > 24 && Age <= 55)
    )")]
        [AssertThat(@"ReasonForTravel != 'John\'s cat named ""\\\'""\n (Backslash Quote)' && ReasonForTravel != SIMONS_CAT")]
        public string ReasonForTravel
        {
            get { return _reasonForTravel; }
            set
            {
                _reasonForTravel = value;
                OnPropertyChanged();
            }
        }

        public DateTime LatestSuggestedReturnDate { get; set; }

        [RequiredIf("GoAbroad == true")]
        [AssertThat("ReturnDate >= Today()")]
        [AssertThat("ReturnDate >= Today() + WeekPeriod")]
        [AssertThat("ReturnDate < AddYears(Today(), 1)")]
        public DateTime? ReturnDate
        {
            get { return _returnDate; }
            set
            {
                _returnDate = value;
                OnPropertyChanged();
            }
        }

        [RequiredIf("GoAbroad == true && ReturnDate > LatestSuggestedReturnDate")]
        public string ReasonForLongTravel
        {
            get { return _reasonForLongTravel; }
            set
            {
                _reasonForLongTravel = value;
                OnPropertyChanged();
            }
        }

        [RequiredIf("GoAbroad == true")]
        public Stability? PoliticalStability
        {
            get { return _politicalStability; }
            set
            {
                _politicalStability = value;
                OnPropertyChanged();
            }
        }

        [AssertThat(@"
(
    AwareOfTheRisks == true
    && (PoliticalStability == Stability.Low || PoliticalStability == Stability.Uncertain)
) 
|| PoliticalStability == null || PoliticalStability == Stability.High")]
        public bool AwareOfTheRisks
        {
            get { return _awareOfTheRisks; }
            set
            {
                _awareOfTheRisks = value;
                OnPropertyChanged();
            }
        }

        public string SportType
        {
            get { return _sportType; }
            set
            {
                _sportType = value;
                OnPropertyChanged();
            }
        }

        [RequiredIf("SportType == 'Extreme' || (SportType != 'None' && GoAbroad == true)")]
        [AssertThat("IsBloodType(Trim(BloodType))")]
        public string BloodType
        {
            get { return _bloodType; }
            set
            {
                _bloodType = value;
                OnPropertyChanged();
            }
        }

        [AssertThat("AgreeForContact == true")]
        public bool AgreeForContact
        {
            get { return _agreeForContact; }
            set
            {
                _agreeForContact = value;
                OnPropertyChanged();
            }
        }

        [RequiredIf(@"AgreeForContact == true
&& (ContactDetails.Email != null || ContactDetails.Phone != null)
&& (ContactDetails.Addresses[0].Details != null || ContactDetails.Addresses[1].Details != null)")]
        public bool? ImmediateContact
        {
            get { return _immediateContact; }
            set
            {
                _immediateContact = value;
                OnPropertyChanged();
            }
        }

        [AssertThat(@"
FlightId != Guid('00000000-0000-0000-0000-000000000000')
&& FlightId != Guid('11111111-1111-1111-1111-111111111111')")]
        public Guid? FlightId
        {
            get { return _flightId; }
            set
            {
                _flightId = value;
                OnPropertyChanged();
            }
        }

        public ContactVM ContactDetails { get; private set; }

        public bool IsBloodType(string group)
        {
            return Regex.IsMatch(group, @"^(A|B|AB|0)[\+-]$");
        }

        public DateTime AddYears(DateTime from, int years)
        {
            return from.AddYears(years);
        }

        public TimeSpan WeekPeriod
        {
            get { return new TimeSpan(7, 0, 0, 0); }
        }
    }
}
