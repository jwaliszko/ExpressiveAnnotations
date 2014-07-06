using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using ExpressiveAnnotations.MvvmDesktopSample.Properties;

namespace ExpressiveAnnotations.MvvmDesktopSample.Models
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
    {
        protected readonly Dictionary<string, IEnumerable<ValidationAttribute>> _validators;
        public event PropertyChangedEventHandler PropertyChanged;

        protected ViewModelBase()
        {
            _validators = GetType().GetProperties()
                .ToDictionary(p => p.Name, p => p.GetCustomAttributes<ValidationAttribute>());
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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

        public string this[string propertyName]
        {
            get
            {
                var value = GetType().GetProperty(propertyName).GetValue(this);
                var context = new ValidationContext(this) {MemberName = propertyName};
                var results = new List<ValidationResult>();
                Validator.TryValidateProperty(value, context, results);
                var message = string.Join(Environment.NewLine, results.Select(x => x.ErrorMessage));
                return message;
            }
        }

        public string Error
        {
            get
            {
                var modelResults = new List<ValidationResult>();
                var properties = GetType().GetProperties();
                foreach (var property in properties)
                {
                    var value = property.GetValue(this);
                    var context = new ValidationContext(this) {MemberName = property.Name};
                    var results = new List<ValidationResult>();
                    Validator.TryValidateProperty(value, context, results);
                    modelResults.AddRange(results);
                }
                var message = string.Join(Environment.NewLine, modelResults.Select(x => x.ErrorMessage));
                return message;
            }
        }
    }
}
