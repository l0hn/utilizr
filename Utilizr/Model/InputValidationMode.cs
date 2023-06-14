using System;

namespace Utilizr.Model
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