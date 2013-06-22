using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Reflection;

namespace Jdn45Common
{
    public static class CountryUtil
    {
        private static CultureInfo cultureInfo;

        public static void Initialize(CultureInfo ci)
        {
            cultureInfo = ci;

            // Initialize the country specific class as well
            ExecuteMethod("Initialize", ci);
        }

        /// <summary>
        /// Executes the method specific to the culture info set for this class.
        /// It's a form of overloading, but with static classes.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static object ExecuteMethod(string methodName, params object[] parameters)
        {
            if (cultureInfo == null)
            {
                throw new Exception("Culture info needs to be set.");
            }

            Type thisType = typeof(Jdn45Common.CountryUtil);
            string expectedType = string.Format("{0}.Country.{1}", thisType.Namespace, cultureInfo.Name.Replace('-', '_'));
            Type cultureType = Type.GetType(expectedType);
            if (cultureType == null)
            {
                throw new Exception("Type not found: " + expectedType);
            }

            MethodInfo methodInfo = cultureType.GetMethod(methodName);
            if (methodInfo == null)
            {
                throw new Exception("Method not found: " + expectedType + "." + methodName);
            }

            return methodInfo.Invoke(null, parameters);
        }

        /// <summary>
        /// Returns the states in a Dictionary.
        /// Key is the 2 letter description, Value is the full name.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetStates()
        {
            // Call the country specific function
            return (Dictionary<string, string>) ExecuteMethod(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// Returns true if the phone string conforms to the expected phone format.
        /// </summary>
        /// <param name="phone">The phone number.</param>
        /// <param name="digitsOnly">Whether the number only contains digits (true) or also contains hyphen and parenthesis (false)</param>
        /// <returns></returns>
        public static bool ValidatePhone(string phone, bool digitsOnly)
        {
            // Call the country specific function
            return (bool)ExecuteMethod(MethodInfo.GetCurrentMethod().Name, phone, digitsOnly);
        }

        public static bool ValidateCompanyTaxId(string companyTaxId)
        {
            // Call the country specific function
            return (bool)ExecuteMethod(MethodInfo.GetCurrentMethod().Name, companyTaxId);
        }

        public static bool ValidatePersonalTaxId(string personalTaxId)
        {
            // Call the country specific function
            return (bool)ExecuteMethod(MethodInfo.GetCurrentMethod().Name, personalTaxId);
        }

        public static bool ValidatePersonalId(string personalId)
        {
            // Call the country specific function
            return (bool)ExecuteMethod(MethodInfo.GetCurrentMethod().Name, personalId);
        }

        /// <summary>
        /// Formats the company's tax ID.
        /// To check if there are the correct number of digits (without adding zeroes), use VerifyCompanyTaxId(...).
        /// </summary>
        /// <param name="cnpj"></param>
        /// <returns></returns>
        public static string FormatCompanyTaxId(string companyTaxId)
        {
            // Call the country specific function
            return (string)ExecuteMethod(MethodInfo.GetCurrentMethod().Name, companyTaxId);
        }

        /// <summary>
        /// Formats the personal tax ID.
        /// To check if there are the correct number of digits (without adding zeroes), use VerifyPersonalTaxId(...).
        /// </summary>
        /// <param name="cpf"></param>
        /// <returns></returns>
        public static string FormatPersonalTaxId(string personalTaxId)
        {
            // Call the country specific function
            return (string)ExecuteMethod(MethodInfo.GetCurrentMethod().Name, personalTaxId);
        }

        /// <summary>
        /// Formats the phone number.
        /// </summary>
        /// <param name="telephone"></param>
        /// <returns></returns>
        public static string FormatTelephone(string telephone)
        {
            // Call the country specific function
            return (string)ExecuteMethod(MethodInfo.GetCurrentMethod().Name, telephone);
        }

        /// <summary>
        /// Formats the personal ID.
        /// To check if there are the correct number of digits (without adding zeroes), use VerifyPersonalId(...).
        /// </summary>
        /// <param name="rg"></param>
        /// <returns></returns>
        public static string FormatPersonalId(string personalId)
        {
            // Call the country specific function
            return (string)ExecuteMethod(MethodInfo.GetCurrentMethod().Name, personalId);
        }

        /// <summary>
        /// Formats the Postal Code.
        /// To check if there are the correct number of digits (without adding zeroes), use VerifyPostalCode(...).
        /// </summary>
        /// <param name="rg"></param>
        /// <returns></returns>
        public static string FormatPostalCode(string postalCode)
        {
            // Call the country specific function
            return (string)ExecuteMethod(MethodInfo.GetCurrentMethod().Name, postalCode);
        }

        /// <summary>
        /// Verify that the postal code is in the expected format.
        /// Does not validate whether it's a valid postal code, just that it's properly formatted.
        /// </summary>
        /// <param name="postalCode">The postal code to verify.</param>
        /// <param name="formatted">Whether it's formatted (true) or it's just the necessary information (false). Necessary information means no spaces, hyphens, etc.</param>
        /// <returns></returns>
        public static bool ValidatePostalCode(string postalCode, bool formatted)
        {
            // Call the country specific function
            return (bool)ExecuteMethod(MethodInfo.GetCurrentMethod().Name, postalCode, formatted);
        }

        /// <summary>
        /// Verify that the state exists.
        /// </summary>
        /// <param name="state">The name (full name) or description (usually 2 letters) of the state.</param>
        /// <param name="smallForm">Whether to validate the full name (true) or the 2 letter description (false).</param>
        /// <returns></returns>
        public static bool ValidateState(string state, bool fullName)
        {
            // Call the country specific function
            return (bool)ExecuteMethod(MethodInfo.GetCurrentMethod().Name, state, fullName);
        }
    }
}
