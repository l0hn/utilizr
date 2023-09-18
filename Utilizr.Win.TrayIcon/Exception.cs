using System;
using System.Collections.Generic;
using System.Text;

namespace Utilizr.Win.TrayIcon
{
    public class TrayNotFoundException : Exception
    {
        public TrayNotFoundException(string message) : base(message) { }
    }
}
