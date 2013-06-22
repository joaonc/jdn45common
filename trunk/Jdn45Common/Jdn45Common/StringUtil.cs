using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Jdn45Common
{
    /// <summary>
    /// Options for a few methods in the StringUtil class.
    /// The default is the absence of the option.
    /// These are bitwise options.
    /// </summary>
    public enum StringUtilOptions
    {
        /// <summary>
        /// Use default options.
        /// </summary>
        None               = 0,
        /// <summary>
        /// Display the column names.
        /// </summary>
        DisplayColumnNames = 1,
        /// <summary>
        /// Aligns the columns with spaces, instead of tabs which is the default.
        /// </summary>
        AlignWithSpaces    = 2,                 // 0010  -  00 (default alignment) + 10 AlignWithSpaces
        /// <summary>
        /// Aligns the columns to the left.
        /// This implies AlignWithSpaces.
        /// </summary>
        AlignLeft          = 6,                 // 0110  -  01 (left alignment)    + 10 AlignWithSpaces
        /// <summary>
        /// Aligns the columns to the right.
        /// This implies AlignWithSpaces.
        /// </summary>
        AlignRight         = 10,                // 1010  -  10 (right alignment)   + 10 AlignWithSpaces
        /// <summary>
        /// Aligns the columns in the center.
        /// This implies AlignWithSpaces.
        /// </summary>
        AlignCenter        = 14,                // 1110  -  11 (center alignment)  + 10 AlignWithSpaces
        /// <summary>
        /// Remove new lines.
        /// Substitutes them with spaces.
        /// </summary>
        RemoveNewLines     = 16
    }

    public static class StringUtil
    {
        public static string RemoveNonDigits(string text)
        {
            string parsedText = "";

            if (!string.IsNullOrEmpty(text))
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (char.IsDigit(text[i]))
                    {
                        parsedText += text[i];
                    }
                }
            }

            return parsedText;
        }

        public static string RemoveDigits(string text)
        {
            string parsedText = "";

            if (!string.IsNullOrEmpty(text))
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (!char.IsDigit(text[i]))
                    {
                        parsedText += text[i];
                    }
                }
            }

            return parsedText;
        }

        /// <summary>
        /// Removes the individual characters (chars) from the given text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static string RemoveChars(string text, string chars)
        {
            return RemoveChars(text, chars.ToCharArray());
        }

        public static string RemoveChars(string text, params char[] chars)
        {
            string parsedText = text;

            foreach(char c in chars)
            {
                parsedText = parsedText.Replace(c.ToString(), "");
            }

            return parsedText;
        }

        /// <summary>
        /// Works similar to string.Format("{0:####}", text)  (and other variants in the format),
        /// except that it works as follows:
        ///   Each '#' character is replaced by the text.
        ///   Every other character is left as is.
        ///   Replacements are done right to left, meaning extra charcters in the text or the formatting string are just added.
        /// 
        /// Ex.
        ///   Format("123", "#,#.#") = "1,2.3"
        ///   Format("MyString", "# ##") = "MyStri ng"
        ///   Format("a", "###") = "a"
        /// </summary>
        /// <param name="text"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Format(string text, string format)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            if (string.IsNullOrEmpty(format))
            {
                return text;
            }

            StringBuilder sb = new StringBuilder();

            int f = format.Length - 1;  // Initialized outside the for b/c it will be needed later
            for (int t = text.Length - 1; t >= 0; t--, f--)
            {
                char cFormat = f>=0 ? format[f] : '#';
                if (cFormat.Equals('#'))
                {
                    sb.Append(text[t]);
                }
                else
                {
                    sb.Append(cFormat);
                    t++;
                }
            }

            // Check the case where the text ended, but the format continues and it's not #
            // Ex. text = "1", format = "(#)"
            //     result is "(1)" and not "1)"
            while (f >= 0)
            {
                if (!format[f].Equals('#'))
                {
                    sb.Append(format[f]);
                }
                f--;
            }

            return Reverse(sb.ToString());
        }

        /// <summary>
        /// Verifies that the given text has the expected format.
        /// The keywords to use in the format are:
        /// '#' for number
        /// '~' for letter
        /// Every other character is compared with itself
        /// </summary>
        /// <param name="text"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool VerifyFormat(string text, string format)
        {
            if (text.Length != format.Length)
            {
                return false;
            }

            bool formatOk = true;
            for (int i = 0; i < text.Length; i++)
            {
                bool isControl = format[i].Equals('#') || format[i].Equals('~');
                if (isControl)
                {
                    if ((format[i].Equals('#') && !Char.IsDigit(text[i])) ||
                        (format[i].Equals('~') && !Char.IsLetter(text[i])))
                    {
                        formatOk = false;
                        break;
                    }
                }
                else if (!format[i].Equals(text[i]))
                {
                    formatOk = false;
                    break;
                }
            }

            return formatOk;
        }

        /// <summary>
        /// Reverses the string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Reverse(string str)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = str.Length - 1; i >= 0; i--)
            {
                sb.Append(str[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Removes diacritis from the string.
        /// Ex. "héllo" becomes "hello".
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        // Code taken from here: http://stackoverflow.com/questions/359827/ignoring-accented-letters-in-string-comparison
        public static string RemoveDiacritics(string str)
        {
            string strFormD = str.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in strFormD)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Checks that the date is in the given format.
        /// See TryParseDate(...) for more information and examples.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool ValidDate(string date, string format)
        {
            DateTime dateTime;
            return TryParseDate(date, format, out dateTime);
        }

        /// <summary>
        /// Returns true if the input string is a valid email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool ValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(strRegex);

            return re.IsMatch(email);
        }

        /// <summary>
        /// Converts the date is in the given format to a DateTime object.
        /// Not culture specific like DateTime.TryParse(...) or DateTime.TryParseExact(...)
        /// Works as follows
        ///   YYYY is checked to be a valid 4 digit year
        ///   YY is checked to be a valid 2 digit year
        ///   MM is checked to be a valid 2 digit month
        ///   DD is checked to be a valid 2 digit day for the given month/year
        ///   All other characters are checked for equality.
        ///   Comparison is case sensitive.
        /// 
        /// Ex.
        ///   ValidDate("2008-02-29", "YYYY-MM-DD") = true   // Leap year
        ///   ValidDate("2010-02-29", "YYYY-MM-DD") = false  // Not a leap year
        ///   ValidDate("2011/01/01", "YYYY-MM-DD") = false  // Separator characters don't match
        ///   ValidDate("03/2011", "MM/YYYY") = true
        ///   ValidDate("03/11", "MM/YY") = true
        ///   ValidDate("03/11", "MM/YYYY") = false          // Expected four digit year
        ///   ValidDate("03/2011", "MM/YY") = false          // Expected two digit year
        /// 
        /// Note 1: only the first characters in the format are used, meaning there can only be one MM and one DD, etc.
        /// Note 2: parts of the date that are not specified are given the value 1, except year, which is given 2010.
        ///         ex: TryParseDate("2011-05", "YYYY-MM", dateTime), dateTime is filled with 2011-05-01
        /// </summary>
        /// <param name="date">The string to convert.</param>
        /// <param name="format">The expected format the string should have.</param>
        /// <param name="dateTime">The output if the conversion is successful.</param>
        /// <returns>True if the conversion was successful, False otherwise.</returns>
        public static bool TryParseDate(string date, string format, out DateTime dateTime)
        {
            dateTime = new DateTime();
            int year=0, month=0, day=0;

            // Initial verification
            if (string.IsNullOrEmpty(date) ||
                string.IsNullOrEmpty(format) ||
                date.Length != format.Length)
            {
                return false;
            }

            // Check year
            int yearLength = 4;
            int yearIndex = format.IndexOf("YYYY");
            if (yearIndex == -1)
            {
                yearLength = 2;
                yearIndex = format.IndexOf("YY");
            }
            if (yearIndex != -1 && !int.TryParse(date.Substring(yearIndex, yearLength), out year))
            {
                return false;
            }

            // Check month
            int monthIndex = format.IndexOf("MM");
            if (monthIndex != -1 && !int.TryParse(date.Substring(monthIndex, 2), out month))
            {
                return false;
            }

            // Check day
            int dayIndex = format.IndexOf("DD");
            if (dayIndex != -1 && !int.TryParse(date.Substring(dayIndex, 2), out day))
            {
                return false;
            }

            // Check other characters
            // First index for removal doesn't need to be recalculated
            if (yearIndex != -1)
            {
                date = date.Remove(yearIndex, yearLength);
                format = format.Remove(yearIndex, yearLength);
            }
            // Remaining indexes for removal need to be recalculated
            if (monthIndex != -1)
            {
                monthIndex = format.IndexOf("MM");
                date = date.Remove(monthIndex, 2);
                format = format.Remove(monthIndex, 2);
            }
            if (dayIndex != -1)
            {
                dayIndex = format.IndexOf("DD");
                date = date.Remove(dayIndex, 2);
                format = format.Remove(dayIndex, 2);
            }

            if (!date.Equals(format))
            {
                return false;
            }

            // Check for valid date
            if (dayIndex == -1) day = 1;
            if (monthIndex == -1) month = 1;
            if (yearIndex == -1) yearIndex = 2010;

            try
            {
                dateTime = new DateTime(year, month, day);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Try to parse a number from a string.
        /// This is different from Decimal.TryParse(...) in:
        ///   * All letters are stripped first
        ///   * The first '.' or ',' in the right is considered the decimal separator
        ///     This allows to have both '.' and ',' as decimal separator, making it Culture insensitive
        ///     However, if the number only contains thousands separator (ex. "1,000"), it will be considered as decimal separator
        ///     In that case, you should use Decimal.TryParse(...)
        /// </summary>
        /// <param name="strNumber"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool TryParseDecimal(string strNumber, out decimal number)
        {
            if (string.IsNullOrEmpty(strNumber))
            {
                number = -1;
                return false;
            }

            // Remove everything that's not a number, '.' or ','
            string parsedText = "";
            for (int i = 0; i < strNumber.Length; i++)
            {
                if (char.IsDigit(strNumber[i]) || strNumber[i].Equals('.') || strNumber[i].Equals(',') || strNumber[i].Equals('-'))
                {
                    parsedText += strNumber[i];
                }
            }

            if (string.IsNullOrEmpty(parsedText))
            {
                number = -1;
                return false;
            }

            // The first in the right of '.' or ',' is considered the decimal separator
            // See Function notes on implications of this
            int decimalSeparatorIndex = parsedText.LastIndexOfAny(new char[] { '.', ',' });
            char decimalSeparator = decimalSeparatorIndex == -1 ? ' ' : parsedText[decimalSeparatorIndex];  // space means none (integer)

            if (!parsedText.IndexOf(decimalSeparator).Equals(decimalSeparatorIndex))
            {
                // There's more than one decimal separator, considering it thousands separator
                decimalSeparatorIndex = -1;
                decimalSeparator = ' ';
            }

            // Remove thousands separator
            parsedText = RemoveChars(parsedText, RemoveChars(".,", decimalSeparator));

            // Convert to a number
            // CultureInfo.InvariantCulture has a '.' as decimal separator, so making sure we have one
            // At this point, we either have ',' as separator or we already have a '.'
            parsedText = parsedText.Replace(',', '.');
            
            return decimal.TryParse(parsedText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out number);
        }

        /// <summary>
        /// See TryParseDecimal(...) for more information.
        /// Throws if conversion was not successful.
        /// </summary>
        /// <param name="strNumber"></param>
        /// <returns></returns>
        public static decimal ParseDecimal(string strNumber)
        {
            decimal number;
            bool ok = TryParseDecimal(strNumber, out number);

            if (!ok)
            {
                throw new Exception("String could not be converted to a decimal number: " + strNumber);
            }

            return number;
        }

        /// <summary>
        /// Returns a string with a line break '\r' for each entry in the enumerable.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static string ToString<T>(IEnumerable<T> enumerable)
        {
            StringBuilder sb = new StringBuilder();

            if (enumerable != null)
            {
                // Append each line
                foreach (T line in enumerable)
                {
                    sb.Append(line.ToString()).Append(Environment.NewLine);  // different than sb.AppendLine(...)
                }

                // Remove the last line break
                if (sb.Length > 0)
                {
                    sb = sb.Remove(sb.Length - Environment.NewLine.Length, Environment.NewLine.Length);
                }
            }

            return sb.ToString();
        }

        public static string ToString(IDictionary dictionary)
        {
            return ToString(dictionary, false, new KeyValuePair<string, string>(null, null));
        }

        public static string ToString(IDictionary dictionary, bool alignColumns)
        {
            return ToString(dictionary, alignColumns, new KeyValuePair<string, string>(null, null));
        }

        public static string ToString(IDictionary dictionary, bool alignColumns, KeyValuePair<string, string> keyValueNames)
        {
            List<List<string>> lines = new List<List<string>>();

            if (!string.IsNullOrEmpty(keyValueNames.Key) && !string.IsNullOrEmpty(keyValueNames.Value))
            {
                List<string> line = new List<string>();
                line.Add(keyValueNames.Key);
                line.Add(keyValueNames.Value);
            }

            foreach (object key in dictionary.Keys)
            {
                List<string> line = new List<string>();
                line.Add(key.ToString());
                line.Add(dictionary[key].ToString());

                lines.Add(line);
            }

            return ToString(lines, true, alignColumns);
        }

        /// <summary>
        /// Returns the string representation of a list (A) of list (B) of strings.
        /// List A are the rows. List B are the columns.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="displayColumnNames">Whether to display the column names. Assuming column names are the in the first line.</param>
        /// <param name="alignColumns"></param>
        /// <returns></returns>
        private static string ToString(List<List<string>> lines, bool displayColumnNames, bool alignColumns)
        {
            // Align columns
            if (alignColumns && lines.Count > 0)
            {
                List<int> columnLength = new List<int>();  // Position: column nr, Value: max len for that column
                // Initialize lenght list (assuming all lines have the same nr of columns, which is true in a DataReader or DataTable)
                for (int i = 0; i < lines[0].Count; i++)
                {
                    columnLength.Add(0);
                }

                // Set max lenghts
                for (int iLine = 0; iLine < lines.Count; iLine++)
                {
                    List<string> line = lines[iLine];
                    for (int iCol = 0; iCol < line.Count; iCol++)
                    {
                        if (line[iCol].Length > columnLength[iCol])
                        {
                            columnLength[iCol] = line[iCol].Length;
                        }
                    }
                }

                // Align existing text
                foreach (List<string> line in lines)
                {
                    for (int iCol = 0; iCol < line.Count; iCol++)
                    {
                        line[iCol] = line[iCol].PadRight(columnLength[iCol]);
                    }
                }
            }

            // Convert to string
            StringBuilder sb = new StringBuilder();
            string colSeparator = alignColumns ? " | " : "\t";
            for (int i = (displayColumnNames ? 0 : 1); i < lines.Count; i++)
            {
                foreach (string column in lines[i])
                {
                    sb.Append(column).Append(colSeparator);
                }
                sb.Remove(sb.Length - colSeparator.Length, colSeparator.Length);  // remove last separator in each line
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the string representation of the DataReader object.
        /// Note that DataReader is read only / forward only, so at then end, the reader object is closed and disposed.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="displayColumnNames"></param>
        /// <param name="alignColumns">Whether to align the columns with spaces (true) or separate them with tabs (false)</param>
        /// <returns></returns>
        public static string ToString(IDataReader dr, bool displayColumnNames, bool alignColumns)
        {
            return ToString(dr, displayColumnNames, alignColumns, Util.FilterType.Keep, null);
        }

        /// <summary>
        /// Returns the string representation of the DataTable object.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="displayColumnNames"></param>
        /// <param name="alignColumns">Whether to align the columns with spaces (true) or separate them with tabs (false)</param>
        /// <returns></returns>
        public static string ToString(DataTable dt, bool displayColumnNames, bool alignColumns)
        {
            return ToString(dt, displayColumnNames, alignColumns, Util.FilterType.Keep, null);
        }

        /// <summary>
        /// Returns the string representation of the DataReader object.
        /// Note that DataReader is read only / forward only, so at then end, the reader object is closed and disposed.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="displayColumnNames"></param>
        /// <param name="alignColumns">Whether to align the columns with spaces (true) or separate them with tabs (false)</param>
        /// <param name="columnFilter">Which columns to display.</param>
        /// <param name="filterType">Whether to keep the items in the filter (remove all others) or remove them (keep all others).</param>
        /// <returns></returns>
        public static string ToString(IDataReader dr, bool displayColumnNames, bool alignColumns, Util.FilterType filterType, List<string> columnFilter)
        {
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            dr.Dispose();

            return ToString(dt, displayColumnNames, alignColumns, filterType, columnFilter);
        }

        /// <summary>
        /// Returns the string representation of the DataReader object.
        /// Note that DataReader is read only / forward only, so at then end, the reader object is closed and disposed.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="displayColumnNames"></param>
        /// <param name="alignColumns">Whether to align the columns with spaces (true) or separate them with tabs (false)</param>
        /// <param name="columnFilter">Which columns to display.</param>
        /// <param name="filterType">Whether to keep the items in the filter (remove all others) or remove them (keep all others).</param>
        /// <returns></returns>
        public static string ToString(DataTable dt, bool displayColumnNames, bool alignColumns, Util.FilterType filterType, List<string> columnFilter)
        {
            // Create a list of lines (instead of using StringBuilder) to then align the columns.
            List<List<string>> lines = new List<List<string>>();
            List<string> line;

            // Filter columns to add
            List<string> filteredColumnNames = Db.DbUtil.GetColumnNames(dt);
            if (columnFilter != null)
            {
                filteredColumnNames = CollectionUtil<string>.Filter(filteredColumnNames, filterType, columnFilter);
            }

            // Columns
            // Always add, even if displayColumnNames == false, b/c filtering is done in ToString at end
            line = new List<string>();
            for (int i = 0; i < filteredColumnNames.Count; i++)
            {
                line.Add(filteredColumnNames[i]);
            }
            lines.Add(line);

            // Content
            foreach(DataRow row in dt.Rows)
            {
                line = new List<string>();
                for (int i = 0; i < filteredColumnNames.Count; i++)
                {
                    line.Add(row[filteredColumnNames[i]].ToString());
                }
                lines.Add(line);
            }

            return ToString(lines, displayColumnNames, alignColumns);
        }

        /// <summary>
        /// Returns the string representation of the DataGridView object.
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <param name="displayColumnNames"></param>
        /// <param name="alignColumns"></param>
        /// <param name="visibleColumnsOnly">Whether to show only the currently visible columns in the DataGridView (true) or all columns (false).</param>
        /// <returns></returns>
        public static string ToString(DataGridView dataGridView, bool displayColumnNames, bool alignColumns, bool visibleColumnsOnly)
        {
            // Create a list of lines (instead of using StringBuilder) o then align the columns.
            List<List<string>> lines = new List<List<string>>();
            List<string> line;

            // Columns
            // Always add, even if displayColumnNames == false, b/c filtering is done in ToString at end
            line = new List<string>();
            for (int i = 0; i < dataGridView.Columns.Count; i++)
            {
                if (dataGridView.Columns[i].Visible ||
                    (!dataGridView.Columns[i].Visible && !visibleColumnsOnly))
                {
                    line.Add(dataGridView.Columns[i].Name);
                }
            }
            lines.Add(line);

            // Content
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                line = new List<string>();
                for (int i = 0; i < dataGridView.Columns.Count; i++)
                {
                    if (dataGridView.Columns[i].Visible ||
                        (!dataGridView.Columns[i].Visible && !visibleColumnsOnly))
                    {
                        line.Add(row.Cells[i].Value.ToString());
                    }
                }
                lines.Add(line);
            }

            return ToString(lines, displayColumnNames, alignColumns);
        }

        private static string ToFormatFromDecimal(object obj, string format)
        {
            decimal amount;
            if (obj.GetType().Equals(typeof(decimal)))
            {
                amount = (decimal)obj;
            }
            else if (!TryParseDecimal(obj.ToString(), out amount))
            {
                throw new Exception("Not a valid number to convert: " + obj.ToString());
            }

            return string.Format(format, amount);
        }

        /// <summary>
        /// Returns a string with the number representation of the object formatted as currency for the current CultureInfo.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToCurrency(object obj)
        {
            return ToFormatFromDecimal(obj, "{0:c}");
        }

        /// <summary>
        /// Returns a string with the number representation of the object formatted as percentage for the current CultureInfo.
        /// Number should be between 0 and 1.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToPercentage(object obj)
        {
            return ToFormatFromDecimal(obj, "{0:#0.00%}");
        }
    }
}