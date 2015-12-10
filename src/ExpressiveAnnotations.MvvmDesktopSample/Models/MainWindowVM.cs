using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressiveAnnotations.MvvmDesktopSample.Models
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        public MainWindowVM()
        {
            ExpressiveVM.ValidationStateChanged += (sender, args) => OnPropertyChanged(() => Progress);
            Query = new QueryVM();            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public QueryVM Query { get; set; }

        public double Progress
        {
            get
            {
                var fieldsCount = ExpressiveVM.AnnotatedPropertiesMap.SelectMany(x => x.Value).Count();
                var errorsCount = ExpressiveVM.ValidationErrorsMap.Count(x => x.Value.Any());
                var passedCount = fieldsCount - errorsCount;
                return (double) passedCount/fieldsCount;
            }
        }
        
        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (PropertyChanged != null)
            {
                var propertyName = GetPropertyName(propertyExpression);
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            var body = propertyExpression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException(
                    "Argument is incomplete or broken",
                    nameof(propertyExpression));

            var property = body.Member as PropertyInfo;
            if (property == null)
                throw new ArgumentException(
                    "Argument is not a property",
                    nameof(propertyExpression));

            return property.Name;
        }
    }
}
