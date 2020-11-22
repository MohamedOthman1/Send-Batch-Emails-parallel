using System;
using System.Collections.Generic;
using System.Text;

namespace BatchEmailsEngine.Utils
{
    public interface ILogHelper
    {
        void ErrorLogger(string message, Exception ex);
        void InfoLogger(string message);
    }
}
