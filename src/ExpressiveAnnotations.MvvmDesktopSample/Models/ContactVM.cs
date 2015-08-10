using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvvmDesktopSample.Models
{
    public class ContactVM : ExpressiveVM
    {
        private string _email;
        private string _phone;

        public ContactVM()
        {
            Addresses = new[] { new AddressVm { Type = "Home address" }, new AddressVm { Type = "Office address" } };
        }

        [RequiredIf("Phone == null")]
        [AssertThat("IsEmail(Email)")]
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        [RequiredIf("Email == null")]
        [AssertThat(@"IsRegexMatch(Phone, '^\\d+$')")]
        [AssertThat("Length(Phone) > 8 && Length(Phone) < 16")]
        public string Phone
        {
            get { return _phone; }
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }

        public AddressVm[] Addresses { get; private set; }
    }
}
