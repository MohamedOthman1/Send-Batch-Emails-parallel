using System;
using System.Collections.Generic;
using System.Text;

namespace BatchEmailsEngine.Utils
{
    [Serializable]
    public class FailedToSendEmailException : Exception
    {
        private readonly ILogHelper _logHelper;

        public FailedToSendEmailException(ILogHelper logHelper)
        {
            _logHelper = logHelper;
        }
        public FailedToSendEmailException(string message)
            : base(message)
        {
            Console.WriteLine(message);
        }
    }
}
