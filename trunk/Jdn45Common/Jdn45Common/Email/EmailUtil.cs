using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;

namespace Jdn45Common.Email
{
    /// <summary>
    /// Helper class for email functionality.
    /// </summary>
    public static class EmailUtil
    {
        private static System.Text.RegularExpressions.Regex reValidEmail = new System.Text.RegularExpressions.Regex(
            @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
            @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
            @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");

        /// <summary>
        /// Returns whether the email has valid syntax or not.
        /// Doesn't verify if the email exists.
        /// </summary>
        /// <param name="email">The email to check.</param>
        /// <returns></returns>
        public static bool IsValidEmail(string email)
        {
            return reValidEmail.IsMatch(email);
        }

        /// <summary>
        /// Email to one recipient.
        /// </summary>
        /// <param name="emailParameters"></param>
        /// <param name="to"></param>
        /// <param name="emailContent"></param>
        /// <param name="attachmentFileList"></param>
        public static void Email(EmailParameters emailParameters, string to, EmailContent emailContent, List<string> attachmentFileList)
        {
            Email(emailParameters, to, emailContent.Subject, emailContent.Body, emailContent.IsHtml, attachmentFileList);
        }

        /// <summary>
        /// Email to one recipient.
        /// </summary>
        /// <param name="emailParameters"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isHtml"></param>
        /// <param name="attachmentFileList"></param>
        public static void Email(EmailParameters emailParameters, string to, string subject, string body, bool isHtml, IEnumerable<string> attachmentFileList)
        {
            Email(emailParameters, new string[] { to }, null, null, subject, body, isHtml, null);
        }

        public static void Email(
            EmailParameters emailParameters,
            IEnumerable<string> toList, IEnumerable<string> ccList, IEnumerable<string> bccList,
            string subject, string body, bool isHtml,
            IEnumerable<string> attachmentFileList)
        {
            // Verification
            IEnumerator<string> enumerator = toList.GetEnumerator();
            if (toList == null || !enumerator.MoveNext() || string.IsNullOrEmpty(enumerator.Current))
            {
                throw new Exception("There must be at least one email address in the To list.");
            }

            foreach (string email in toList)
                if (!IsValidEmail(email))
                    throw new Exception("Invalid email in the To list: " + email);

            if (ccList != null)
            {
                foreach (string email in ccList)
                    if (!IsValidEmail(email))
                        throw new Exception("Invalid email in the Cc list: " + email);
            }

            if (bccList != null)
            {
                foreach (string email in bccList)
                    if (!IsValidEmail(email))
                        throw new Exception("Invalid email in the Bcc list: " + email);
            }

            // Smtp server
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = emailParameters.Host;
            if (!emailParameters.UsingDefaultPort)
            {
                smtpClient.Port = emailParameters.Port;
            }
            smtpClient.EnableSsl = emailParameters.UseSSL;
            smtpClient.Credentials = new System.Net.NetworkCredential(emailParameters.User, Util.Decrypt(emailParameters.Password));

            // Emails From, To, Cc, Bcc and attachments
            MailMessage mailMessage = new MailMessage();

            mailMessage.From = new MailAddress(emailParameters.Email, emailParameters.DisplayName);
            foreach (string to in toList)
            {
                mailMessage.To.Add(new MailAddress(to));
            }
            if (ccList != null)
            {
                foreach (string cc in ccList)
                {
                    mailMessage.CC.Add(new MailAddress(cc));
                }
            }
            if (bccList != null)
            {
                foreach (string bcc in bccList)
                {
                    mailMessage.Bcc.Add(new MailAddress(bcc));
                }
            }
            if (attachmentFileList != null)
            {
                foreach (string fileName in attachmentFileList)
                {
                    mailMessage.Attachments.Add(new Attachment(fileName));
                }
            }

            // Email Subject and Body
            mailMessage.Subject = subject;
            mailMessage.IsBodyHtml = isHtml;
            mailMessage.Body = body;

            // Send email
            if (!(Util.IsDebug() && emailParameters.SkipEmailOnDebug))
            {
                smtpClient.Send(mailMessage);
            }
        }
    }
}
