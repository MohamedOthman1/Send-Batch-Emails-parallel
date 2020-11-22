using log4net;
using Microsoft.Extensions.Logging;
using System;


namespace BatchEmailsEngine.Utils
{
    public class LogHelper : ILogHelper

    {

        private readonly ILogger _logger;

        public LogHelper(ILogger<LogHelper> logger)
        {
            _logger = logger;

        }

        public void ErrorLogger(string message, Exception ex)
        {

            string methodName = ex?.TargetSite.Name;
            string className = ex?.Source;
            ThreadContext.Properties["ClassName"] = className;
            ThreadContext.Properties["MethodName"] = methodName;
            string msg;
            if (ex?.InnerException == null)
            {
                msg = string.Format("Message: {0} ; InnerExcept: No InnerException ; " +
                    "StackTrace: {3}", message, className, methodName, ex?.StackTrace);

            }
            else
            {
                msg = string.Format("Message: {0}  ; InnerExcept: {3} ;" +
                    "StackTrace: {4}", message, className, methodName, ex?.InnerException,
                    ex?.StackTrace);

            }
            _logger.LogError(msg);
        }



        public void InfoLogger(string message)
        {
            string msg = string.Format("Message: {0} ", message);
            _logger.LogInformation(msg);
        }
    }
}
