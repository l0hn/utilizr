using System.ComponentModel;

namespace Utilizr.WPF
{
    public abstract class ParentViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _logCat = "";
        public string LogCat
        {
            get
            {
                if (string.IsNullOrEmpty(_logCat))
                {
                    _logCat = GetType().Name;
                }
                return _logCat;
            }
        }

        // Support for Fody, has to be public and accept single string argument
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Original, keep for legacy / plus manual invocations
        protected virtual void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (var propName in propertyNames)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}