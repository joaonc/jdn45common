using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;

namespace Jdn45Common.Country
{
    /// <summary>
    /// Country specification for the pt_BR culture.
    /// Definitions:
    ///   * PersonalTaxId = CPF
    ///   * CompanyTaxId = CNPJ
    ///   * PersonalId = RG
    ///   * PostalCode = CEP
    /// </summary>
    public class pt_BR
    {
        #region States
        /// <summary>
        /// States of Brazil.
        /// Use Jdn2Common.Attributes.GetDescription(...) to get the full name.
        /// </summary>
        public enum State
        {
            [Description("Acre")]                   AC,
            [Description("Alagoas")]                AL,
            [Description("Amazonas")]               AM,
            [Description("Amapá")]                  AP,
            [Description("Bahia")]                  BA,
            [Description("Ceará")]                  CE,
            [Description("Distrito Frederal")]      DF,
            [Description("Espírito Santo")]         ES,
            [Description("Goiás")]                  GO,
            [Description("Maranhão")]               MA,
            [Description("Minas Gerais")]           MG,
            [Description("Mato Grosso do Sul")]     MS,
            [Description("Mato Grosso")]            MT,
            [Description("Pará")]                   PA,
            [Description("Paraíba")]                PB,
            [Description("Pernambuco")]             PE,
            [Description("Piauí")]                  PI,
            [Description("Paraná")]                 PR,
            [Description("Rio de Janeiro")]         RJ,
            [Description("Rio Grande do Norte")]    RN,
            [Description("Rondônia")]               RO,
            [Description("Rorâima")]                RR,
            [Description("Rio Grande do Sul")]      RS,
            [Description("Santa Catarina")]         SC,
            [Description("Sergipe")]                SE,
            [Description("São Paulo")]              SP,
            [Description("Tocantins")]              TO
        }
        #endregion

        /// <summary>
        /// Specific initialization for pt_BR.
        /// </summary>
        /// <param name="ci"></param>
        public static void Initialize(CultureInfo ci)
        {
            // Do nothing
        }

        /// <summary>
        /// Returns the states in a Dictionary.
        /// Key is the 2 letter description, Value is the full name.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetStates()
        {
            Dictionary<string, string> states = new Dictionary<string, string>();

            foreach (Enum state in Enum.GetValues(typeof(State)))
            {
                states.Add(state.ToString(), Jdn45Common.Attributes.AttributeUtil.GetDescription(state));
            }

            return states;
        }

        /// <summary>
        /// Returns true if the phone string conforms to the expected phone format.
        /// The expected format is:
        ///   digitsOnly = true: DDNNNNNNNN (DDD + full number)
        ///   digitsOnly = false: (DD) NNNN-NNNN
        /// </summary>
        /// <param name="phone">The phone number.</param>
        /// <param name="digitsOnly">Whether the number only contains digits (true) or also contains hyphen and parenthesis (false)</param>
        /// <returns></returns>
        public static bool ValidatePhone(string phone, bool digitsOnly)
        {
            string format = digitsOnly ? "##########" : "(##) ####-####";

            return StringUtil.VerifyFormat(phone, format);
        }

        public static bool ValidateCompanyTaxId(string companyTaxId)
        {
            return false;  // TODO
        }

        public static bool ValidatePersonalTaxId(string personalTaxId)
        {
            return false;  // TODO
        }

        public static bool ValidatePersonalId(string personalId)
        {
            return false;  // TODO
        }

        /// <summary>
        /// Formats the company's tax ID (CNPJ).
        /// To check if there are the correct number of digits (without adding zeroes), use VerifyCompanyTaxId(...).
        /// </summary>
        /// <param name="cnpj"></param>
        /// <returns></returns>
        public static string FormatCompanyTaxId(string companyTaxId)
        {
            string s = StringUtil.RemoveNonDigits(companyTaxId);
            return StringUtil.Format(s, "###.###.###/####-##");
        }

        /// <summary>
        /// Formats the personal tax ID (CPF).
        /// To check if there are the correct number of digits (without adding zeroes), use VerifyPersonalTaxId(...).
        /// </summary>
        /// <param name="cpf"></param>
        /// <returns></returns>
        public static string FormatPersonalTaxId(string personalTaxId)
        {
            string s = StringUtil.RemoveNonDigits(personalTaxId);
            return StringUtil.Format(s, "###.###.###-##");
        }

        /// <summary>
        /// Formats the phone number.
        /// </summary>
        /// <param name="telephone"></param>
        /// <returns></returns>
        public static string FormatTelephone(string telephone)
        {
            string s = StringUtil.RemoveNonDigits(telephone);
            return StringUtil.Format(s, "(##) ####-####");
        }

        /// <summary>
        /// Formats the personal ID (RG).
        /// To check if there are the correct number of digits (without adding zeroes), use VerifyPersonalId(...).
        /// </summary>
        /// <param name="rg"></param>
        /// <returns></returns>
        public static string FormatPersonalId(string personalId)
        {
            string s = StringUtil.RemoveChars(personalId, "./- ");
            return StringUtil.Format(s, "##.###.###-#");
        }

        /// <summary>
        /// Formats the Postal Code (CEP).
        /// To check if there are the correct number of digits (without adding zeroes), use VerifyPostalCode(...).
        /// </summary>
        /// <param name="rg"></param>
        /// <returns></returns>
        public static string FormatPostalCode(string postalCode)
        {
            string s = StringUtil.RemoveNonDigits(postalCode);
            return StringUtil.Format(s, "#####-###");
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
            bool formatOk;

            if (formatted)
            {
                formatOk = (postalCode.Length == 9 &&
                    postalCode.Equals(FormatPostalCode(postalCode)));
            }
            else
            {
                int i = 0;
                formatOk = postalCode.Length == 8 && int.TryParse(postalCode, out i);
                formatOk &= (i > 0);
            }

            return formatOk;
        }

        /// <summary>
        /// Verify that the state exists.
        /// </summary>
        /// <param name="state">The name (full name) or the 2 letter description of the state.</param>
        /// <param name="smallForm">Whether to validate the full name (true) or the 2 letter description (false).</param>
        /// <returns></returns>
        public static bool ValidateState(string state, bool fullName)
        {
            Dictionary<string, string> states = GetStates();

            bool isValid;
            if (fullName)
            {
                isValid = states.ContainsValue(state);
            }
            else
            {
                isValid = states.ContainsKey(state);
            }

            return isValid;
        }
    }
}
