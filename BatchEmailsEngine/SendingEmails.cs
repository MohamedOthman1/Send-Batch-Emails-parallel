using BatchEmailsEngine.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BatchEmailsEngine
{
    public class SendingEmails : ISendingEmails
    {
        private readonly IConfiguration _configuration;
        private static IConfigurationSection smtpSection;
        private readonly ILogHelper _logHelper;
        private List<string> failedEmails = new List<string>();
        private const int retryNbr = 3;

        public SendingEmails(ILogHelper logHelper, IConfiguration configuration)
        {
            _configuration = configuration;
            _logHelper = logHelper;
        }
        public void Run()
        {
            Stopwatch stopwatch = new Stopwatch();

            try
            {
                List<string> emailsList = ReadDataFromFile();
                emailsList = emailsList.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                int batchSize = int.Parse(_configuration.GetSection("smtpConfig").GetSection("batchSize").Value);
                var mailingChunks = emailsList.Select((x, i) => new { Index = i, Value = x })
                                    .GroupBy(x => x.Index / batchSize)
                                    .Select(x => x.Select(v => v.Value).ToList())
                                    .ToList();

                stopwatch.Start();
                IEnumerable<Task> sendMailingChunks = mailingChunks.Select(
                mailingChunk => Task.Run(() => StartBulk(mailingChunk)));
                Task sendBuilkEmails = Task.WhenAll(sendMailingChunks);
                while (!sendBuilkEmails.IsCompleted)
                {
                    Task.Delay(500).Wait();
                }

                ResendDeliverFailed();
            }
            catch (Exception ex)
            {
                _logHelper.ErrorLogger("Filed not found", ex);
                Console.WriteLine("Filed not found" + ex.Message);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine("Sending Batching emails completed ");
                _logHelper.InfoLogger("Sending Batching emails completed in " + stopwatch.Elapsed);
            }
        }

        public void ResendDeliverFailed()
        {
            string host = smtpSection.GetSection("host").Value;
            using (SmtpClient smtp = new SmtpClient(host))
            {
                smtp.UseDefaultCredentials = bool.Parse(smtpSection.GetSection("useDefaultCredentials").Value);
                smtp.Port = int.Parse(smtpSection.GetSection("port").Value);
                string username = smtpSection.GetSection("username").Value;
                string password = smtpSection.GetSection("password").Value;
                smtp.Credentials = new System.Net.NetworkCredential(username, password);
                smtp.EnableSsl = bool.Parse(smtpSection.GetSection("enableSsl").Value);
                foreach (string email in failedEmails)
                {
                    try
                    {
                        int retry = retryNbr;
                        int attempts = 0;
                        while (retry > 0)
                        {

                            try
                            {

                                MailMessage message = new MailMessage(username, email)
                                {
                                    Subject ="Test Email",
                                    Body = "Dear reader,\n" +
                                           "This is not a SPAM !\n" +
                                           "This only a test email"
                                };
                                smtp.Send(message);
                            }
                            catch (SmtpException ex)
                            {
                                attempts++;
                                _logHelper.ErrorLogger("Retry sending email failed after "+ attempts + 
                                    "attempt(s) to:" + email, ex);
                                retry--;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Retry sending email failed to: " + email);
                                break;
                            }

                        }
                        if (retry == 0)
                        {
                            throw new FailedToSendEmailException("Retry sending email failed after all three attemps");
                        }
                    }
                    catch (FailedToSendEmailException ex)
                    {
                        _logHelper.ErrorLogger(ex.Message, ex);
                    }


                }

            }

        }

        public List<string> ReadDataFromFile()
        {
            List<string> emailsList = new List<string>();
            string filelocation = _configuration.GetSection("fileLocation").Value;
            try
            {
                using (StreamReader reader = new StreamReader(filelocation))
                {
                    string[] emails = reader.ReadToEnd().Split(';');
                    foreach (string email in emails)
                    {
                        emailsList.Add(email);
                    }
                }

                _logHelper.InfoLogger("Read list of emails from a file ");
                return emailsList;
            }
            catch (FileNotFoundException ex)
            {
                _logHelper.ErrorLogger("Filed not found", ex);
                Console.WriteLine("Filed not found" + ex.Message);
                throw;
            }

        }

        public void StartBulk(IEnumerable<string> recipients)
        {
            smtpSection = _configuration.GetSection("smtpConfig");
            string host = smtpSection.GetSection("host").Value;
            using (SmtpClient smtp = new SmtpClient(host))
            {
                Console.WriteLine("Create smtpclient");
                smtp.UseDefaultCredentials = bool.Parse(smtpSection.GetSection("useDefaultCredentials").Value);
                smtp.Port = int.Parse(smtpSection.GetSection("port").Value);
                string username = smtpSection.GetSection("username").Value;
                string password = smtpSection.GetSection("password").Value;
                smtp.Credentials = new System.Net.NetworkCredential(username, password);
                smtp.EnableSsl = bool.Parse(smtpSection.GetSection("enableSsl").Value);
                RunBulk(username, smtp, recipients);

            }
        }

        public void RunBulk(string username, SmtpClient smtp, IEnumerable<string> recipients)
        {
            foreach (var recipient in recipients)
            {
                MailMessage message = new MailMessage(username, recipient)
                {
                    Subject = "Test Email",
                    Body = "Dear reader,\n" +
                                          "This is not a SPAM !\n" +
                                          "This only a test email"
                };
                SendMessage(smtp, message);
            }
        }

        public void SendMessage(SmtpClient smtp, MailMessage message)
        {
            try
            {
                smtp.Send(message);
                _logHelper.InfoLogger("Email sent successfully to: " + message.To);
                Console.WriteLine("Email sent successfully to: " + message.To);
            }
            catch (SmtpException ex)
            {
                _logHelper.ErrorLogger("Failed to send email from the first attempt to : " + message.To, ex);
                failedEmails.Add(message.To.ToString());

            }
            catch (Exception ex)
            {
                _logHelper.ErrorLogger("Failed to send email to : " + message.To, ex);
                Console.WriteLine("Failed to send email to: " + message.To);
            }
        }
    }
}
