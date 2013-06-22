using System;
using System.Collections.Generic;
using System.Text;

namespace Jdn45Common.Email
{
    /// <summary>
    /// Class that represents the content of an email.
    /// </summary>
    public class EmailContent
    {
        private string subject;
        private string body;
        private bool isHtml;

        public EmailContent()
        {
        }

        public EmailContent(string subject, string body, bool isHtml)
        {
            Subject = subject;
            Body = body;
            IsHtml = isHtml;
        }

        public string Subject
        {
            get { return subject; }
            set { subject = value; }
        }

        public string Body
        {
            get { return body; }
            set { body = value; }
        }

        public bool IsHtml
        {
            get { return isHtml; }
            set { isHtml = value; }
        }
    }
}
