using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilizr.WPF.Validation
{
    public enum InputValidationMode
    {
        /// <summary>
        /// Validation will be invoked when the control losses focus and the property changes.
        /// </summary>
        Default,

        /// <summary>
        /// Validation will never be invoked automatically, it will be your responsibility entirely.
        /// </summary>
        Explicit,
    }
}
