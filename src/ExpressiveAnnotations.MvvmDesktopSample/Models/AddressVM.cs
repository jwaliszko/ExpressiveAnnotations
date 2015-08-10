using ExpressiveAnnotations.Attributes;

namespace ExpressiveAnnotations.MvvmDesktopSample.Models
{
    public class AddressVm : ExpressiveVM
    {
        private string _details;

        public string Type { get; set; }
        
        [AssertThat("StartsWith(Details, 'Street')")]
        public string Details
        {
            get { return _details; }
            set
            {
                _details = value;
                OnPropertyChanged();
            }
        }
    }
}
