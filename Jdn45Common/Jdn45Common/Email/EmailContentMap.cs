using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Jdn45Common.Email
{
    public enum EmailContentType
    {
        /// <summary>
        /// Sample email with Html.
        /// </summary>
        EmailHtml = 1,
        /// <summary>
        /// Sample email with Text only.
        /// </summary>
        EmailText
    }

    /// <summary>
    /// This class helps retrieve the name of the XML file that has the email content.
    /// NOTE: This is old code to be refactored. These contents should not be hard coded.
    /// </summary>
    public class EmailContentMap
    {
        private static List<CultureInfo> availableCultures;
        private static Dictionary<EmailContentType, string> emailContentDict;

        static EmailContentMap()
        {
            // Available languages
            availableCultures = new List<CultureInfo>();
            availableCultures.Add(new CultureInfo("pt-BR", true));

            // Content
            emailContentDict = new Dictionary<EmailContentType, string>();
            emailContentDict.Add(EmailContentType.EmailHtml, "EmailHtml.xml");
            emailContentDict.Add(EmailContentType.EmailText, "EmailText.xml");
        }

        public static List<CultureInfo> AvailableCultures
        {
            get { return availableCultures; }
        }

        public static Dictionary<EmailContentType, string> AvailableContent
        {
            get { return emailContentDict; }
        }

        public static string GetFileName(CultureInfo cultureInfo, EmailContentType emailContentType)
        {
            // Check it's available in the given culture
            if (!availableCultures.Contains(cultureInfo))
            {
                throw new Exception("File not available for culture info " + cultureInfo.Name + ", " + cultureInfo.NativeName);
            }

            return @"Email\Content\" + cultureInfo.Name + @"\" + emailContentType.ToString() + ".xml";
        }

        public static EmailContent GetEmailContent(CultureInfo cultureInfo, EmailContentType emailContentType)
        {
            return GetEmailContent(GetFileName(cultureInfo, emailContentType));
        }

        public static EmailContent GetEmailContent(string fileName)
        {
            return (EmailContent)Jdn45Common.Util.DeserializeFromXmlFile(fileName, typeof(EmailContent));
        }

        public static void SaveEmailContent(CultureInfo cultureInfo, EmailContentType emailContentType, EmailContent emailContent)
        {
            Jdn45Common.Util.SerializeToXmlFile(emailContent, GetFileName(cultureInfo, emailContentType));
        }

        /// <summary>
        /// Gets the EmailContent object with the text tokens expanded with the given contents.
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <param name="emailContentType"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static EmailContent GetEmailContentExpanded(CultureInfo cultureInfo, EmailContentType emailContentType, Dictionary<string, string> content)
        {
            return GetEmailContentExpanded(GetFileName(cultureInfo, emailContentType), content);
        }

        /// <summary>
        /// Gets the EmailContent object with the text tokens expanded with the given contents.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static EmailContent GetEmailContentExpanded(string fileName, Dictionary<string, string> content)
        {
            EmailContent emailContent = GetEmailContent(fileName);

            emailContent.Subject = ExpandContent(emailContent.Subject, content);
            emailContent.Body = ExpandContent(emailContent.Body, content);

            return emailContent;
        }

        /// <summary>
        /// Replaces the tokens {{key}} in text with value.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string ExpandContent(string text, Dictionary<string, string> content)
        {
            foreach (string key in content.Keys)
            {
                text = text.Replace("{{" + key + "}}", content[key]);
            }

            return text;
        }
    }
}
