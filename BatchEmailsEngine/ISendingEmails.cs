using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace BatchEmailsEngine
{
    public interface ISendingEmails
    {
        void Run();
        List<string> ReadDataFromFile();
        void StartBulk(IEnumerable<string> recipients);
        void ResendDeliverFailed();
        void SendMessage(SmtpClient smtp, MailMessage message);
        void RunBulk(string username, SmtpClient smtp, IEnumerable<string> recipients);
    }
}
