using System.Linq;

namespace ExpressiveAnnotations.MvvmDesktopSample.Models
{
    public class MainWindowVM : BaseVM
    {
        public MainWindowVM()
        {
            ValidationStateChanged += (sender, args) => OnPropertyChanged(() => Progress);
            Query = new QueryVM();            
        }

        public QueryVM Query { get; set; }

        public double Progress
        {
            get
            {
                var fieldsCount = AnnotatedProperties.Count();
                var errorsCount = ValidationErrors.Count(x => x.Value.Any());
                var passedCount = fieldsCount - errorsCount;
                return (double) passedCount/fieldsCount;
            }
        }
    }
}
