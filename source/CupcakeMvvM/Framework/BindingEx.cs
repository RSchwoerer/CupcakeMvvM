using System.Windows.Data;

namespace CupcakeMvvM.Framework
{

    public class BindingEx : Binding
    {
        public BindingEx()
            : base()
        {
            Configure();
        }

        public BindingEx(string path)
            : base(path)
        {
            Configure();
        }

        private void Configure()
        {
            // ValidatesOnDataErrors=True, UpdateSourceTrigger=PropertyChanged, NotifyOnValidationError=True
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            NotifyOnValidationError = true;
            ValidatesOnDataErrors = true;
            ValidatesOnExceptions = true;
        }
    }
}