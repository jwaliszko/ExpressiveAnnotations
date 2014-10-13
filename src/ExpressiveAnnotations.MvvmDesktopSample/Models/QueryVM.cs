using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvvmDesktopSample.Models
{
    public class QueryVM : BaseVM
    {
        public const string SIMONS_CAT = @"Simon's cat named ""\\""
 (Double Backslash)";

        private int? _age;
        private bool _agreeForContact;
        private bool _awareOfTheRisks;
        private string _bloodType;
        private ContactVM _contactDetails;
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

        public IEnumerable<KeyValuePair<string, string>> Countries
        {
            get
            {
                return new[]
                {
                    new KeyValuePair<string, string>("Poland", "Poland"),
                    new KeyValuePair<string, string>("Germany", "Germany"),
                    new KeyValuePair<string, string>("France", "France"),
                    new KeyValuePair<string, string>("Other", "Other")
                };
            }
        }

        public IEnumerable<KeyValuePair<string, bool?>> Answers
        {
            get
            {
                return new[]
                {
                    new KeyValuePair<string, bool?>(string.Empty, null),
                    new KeyValuePair<string, bool?>("Yes", true),
                    new KeyValuePair<string, bool?>("No", false)
                };
            }
        }

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
                OnPropertiesChanged();
            }
        }

        [Required]
        public int? Age
        {
            get { return _age; }
            set
            {
                _age = value;
                OnPropertiesChanged();
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
                OnPropertiesChanged();
            }
        }

        public string Country
        {
            get { return _country; }
            set
            {
                _country = value;
                OnPropertiesChanged();
            }
        }

        public string NextCountry
        {
            get { return _nextCountry; }
            set
            {
                _nextCountry = value;
                OnPropertiesChanged();
            }
        }

        [RequiredIf(@"GoAbroad == true
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
                OnPropertiesChanged();
            }
        }

        public DateTime LatestSuggestedReturnDate { get; set; }

        [RequiredIf("GoAbroad == true")]
        [AssertThat("ReturnDate >= Today()")]
        [AssertThat("ReturnDate < AddYears(Today(), 1)")]
        public DateTime? ReturnDate
        {
            get { return _returnDate; }
            set
            {
                _returnDate = value;
                OnPropertiesChanged();
            }
        }

        [RequiredIf("GoAbroad == true && ReturnDate > LatestSuggestedReturnDate")]
        public string ReasonForLongTravel
        {
            get { return _reasonForLongTravel; }
            set
            {
                _reasonForLongTravel = value;
                OnPropertiesChanged();
            }
        }

        [RequiredIf("GoAbroad == true")]
        public Stability? PoliticalStability
        {
            get { return _politicalStability; }
            set
            {
                _politicalStability = value;
                OnPropertiesChanged();
            }
        }

        [AssertThat(@"(
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
                OnPropertiesChanged();
            }
        }

        public string SportType
        {
            get { return _sportType; }
            set
            {
                _sportType = value;
                OnPropertiesChanged();
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
                OnPropertiesChanged();
            }
        }

        [AssertThat("AgreeForContact == true")]
        public bool AgreeForContact
        {
            get { return _agreeForContact; }
            set
            {
                _agreeForContact = value;
                OnPropertiesChanged();
            }
        }

        [RequiredIf("AgreeForContact == true && (ContactDetails.Email != null || ContactDetails.Phone != null)")]
        public bool? ImmediateContact
        {
            get { return _immediateContact; }
            set
            {
                _immediateContact = value;
                OnPropertiesChanged();
            }
        }

        [AssertThat(@"FlightId != Guid('00000000-0000-0000-0000-000000000000')
                      && FlightId != Guid('11111111-1111-1111-1111-111111111111')")]
        public Guid? FlightId
        {
            get { return _flightId; }
            set
            {
                _flightId = value;
                OnPropertiesChanged();
            }
        }

        public ContactVM ContactDetails
        {
            get { return _contactDetails; }
            set
            {
                _contactDetails = value;
                OnPropertiesChanged();
            }
        }

        private void OnPropertiesChanged()
        {
            OnPropertyChanged(() => GoAbroad);
            OnPropertyChanged(() => Age);
            OnPropertyChanged(() => PassportNumber);
            OnPropertyChanged(() => Country);
            OnPropertyChanged(() => NextCountry);
            OnPropertyChanged(() => ReasonForTravel);
            OnPropertyChanged(() => ReturnDate);
            OnPropertyChanged(() => ReasonForLongTravel);
            OnPropertyChanged(() => PoliticalStability);
            OnPropertyChanged(() => AwareOfTheRisks);
            OnPropertyChanged(() => SportType);
            OnPropertyChanged(() => BloodType);
            OnPropertyChanged(() => AgreeForContact);
            OnPropertyChanged(() => ImmediateContact);
            OnPropertyChanged(() => FlightId);
            OnPropertyChanged(() => ContactDetails);
        }

        public bool IsBloodType(string group)
        {
            return Regex.IsMatch(group, @"^(A|B|AB|0)[\+-]$");
        }

        public DateTime AddYears(DateTime from, int years)
        {
            return from.AddYears(years);
        }
    }
}
