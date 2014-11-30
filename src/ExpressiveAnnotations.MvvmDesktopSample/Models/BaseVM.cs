using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using ExpressiveAnnotations.MvvmDesktopSample.Properties;

namespace ExpressiveAnnotations.MvvmDesktopSample.Models
{
    public abstract class BaseVM : INotifyPropertyChanged, IDataErrorInfo
    {
        static BaseVM()
        {
            ValidationErrors = new Dictionary<string, IEnumerable<string>>();
            AnnotatedProperties = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && t.IsSubclassOf(typeof (BaseVM)))
                .SelectMany(t => t.GetProperties())
                .Where(p => p.GetCustomAttributes().Any(a => a is ValidationAttribute))
                .ToList();
        }

        public static event EventHandler ValidationStateChanged;

        protected void OnValidationStateChanged()
        {
            if (ValidationStateChanged != null)
                ValidationStateChanged(this, new EventArgs());
        }

        public static IEnumerable<PropertyInfo> AnnotatedProperties { get; private set; }

        public static Dictionary<string, IEnumerable<string>> ValidationErrors { get; private set; }

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
                UpdateValidationErrors(propertyName, errors);
                return string.Join(Environment.NewLine, errors);
            }
        }

        public string Error
        {
            get { return null; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [NotifyPropertyChangedInvocator]
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
                throw new ArgumentNullException("propertyExpression");

            var body = propertyExpression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("Argument is incomplete or broken", "propertyExpression");

            var property = body.Member as PropertyInfo;
            if (property == null)
                throw new ArgumentException("Argument is not a property", "propertyExpression");

            return property.Name;
        }

        private void UpdateValidationErrors(string propertyName, IEnumerable<string> errors)
        {
            errors = errors.ToList();
            var fullName = string.Format("{0}.{1}", GetType().FullName, propertyName);
            var before = ValidationErrors.Count(x => x.Value.Any());
            ValidationErrors[fullName] = errors;
            var after = ValidationErrors.Count(x => x.Value.Any());

            if (before != after)
                OnValidationStateChanged();
        }
    }
}
