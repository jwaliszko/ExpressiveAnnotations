using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ExpressiveAnnotations.MvvmDesktopSample.Models
{
    public abstract class ExpressiveVM : INotifyPropertyChanged, IDataErrorInfo
    {
        static ExpressiveVM()
        {
            ValidationErrorsMap = new Dictionary<string, IEnumerable<string>>();
            AnnotatedPropertiesMap = new Dictionary<ExpressiveVM, IEnumerable<PropertyInfo>>();
        }

        protected ExpressiveVM()
        {
            AnnotatedPropertiesMap[this] = GetType().GetProperties()
                .Where(p => p.GetCustomAttributes().Any(a => a is ValidationAttribute))
                .ToList();
        }
        
        public static event EventHandler ValidationStateChanged;        
        public static Dictionary<string, IEnumerable<string>> ValidationErrorsMap { get; private set; }
        public static Dictionary<ExpressiveVM, IEnumerable<PropertyInfo>> AnnotatedPropertiesMap { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public string this[string propertyName]
        {
            get
            {
                var value = GetType().GetProperty(propertyName).GetValue(this);
                var context = new ValidationContext(this) {MemberName = propertyName};
                var results = new List<ValidationResult>();

                try
                {
                    Validator.TryValidateProperty(value, context, results);
                }
                catch (Exception e)
                {                    
                    MessageBox.Show(e.InnerException.Message, e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }                

                var errors = results.Select(x => x.ErrorMessage).ToList();
                UpdateValidationErrors(propertyName, this, errors);
                return string.Join(Environment.NewLine, errors);
            }
        }

        public string Error
        {
            get { return null; }
        }
        
        protected void OnValidationStateChanged()
        {
            if (ValidationStateChanged != null)
                ValidationStateChanged(this, new EventArgs());
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Revalidate();
        }

        private void Revalidate()
        {
            foreach (var kvp in AnnotatedPropertiesMap)
            {
                var context = kvp.Key;
                var properties = kvp.Value;
                foreach (var prop in properties)
                {
                    if (context.PropertyChanged != null)
                        PropertyChanged(context, new PropertyChangedEventArgs(prop.Name));
                }
            }
        }

        private void UpdateValidationErrors(string propertyName, object context, IEnumerable<string> errors)
        {
            errors = errors.ToList();
            var fullName = string.Format("{0}.{1}.{2}", GetType().FullName, propertyName, RuntimeHelpers.GetHashCode(context));
            var before = ValidationErrorsMap.Count(x => x.Value.Any());
            ValidationErrorsMap[fullName] = errors;
            var after = ValidationErrorsMap.Count(x => x.Value.Any());

            if (before != after)
                OnValidationStateChanged();
        }
    }
}
