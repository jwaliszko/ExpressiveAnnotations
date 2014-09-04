using System.Linq;

namespace ExpressiveAnnotations.MvvmDesktopSample.Models
{
    public class MainWindowVM: BaseVM
    {
        public MainWindowVM()
        {
            Query = new QueryVM();
            Query.PropertyChanged += (sender, args) => OnPropertyChanged(() => Progress);
        }

        public QueryVM Query { get; set; }

        public double Progress
        {
            get
            {
                var attribCount = Query.AnnotatedProperties.Count() + Query.ContactDetails.AnnotatedProperties.Count();
                var errorsCount = Query.ValidationErrors.Count() + Query.ContactDetails.ValidationErrors.Count();
                var passedCount = attribCount - errorsCount;
                return (double) passedCount/attribCount;
            }
        }
    }
}
