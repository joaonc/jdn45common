using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;

namespace Jdn45Common
{
    /// <summary>
    /// Miscellaneous utility methods.
    /// </summary>
    public static class Util
    {
        private static readonly string encryptionKey = "jd3wqq3K3EU24nzm8ap5DENZ";  // DO NOT CHANGE!! 192 bit key (24 characters)
        private static readonly string instanceSpecificName = Path.GetRandomFileName();  // Variable that gets initialized each time the Util class is instanciated.

        /// <summary>
        /// Represents a DateTime value that has not been set.
        /// Useful when creating new DateTime objects or passing as paramenters, since DateTime is a type and cannot be null.
        /// </summary>
        public static readonly DateTime DateTimeNotSet = new DateTime(1890, 10, 10);  // A date unlikely to be used and acceptable by MS SQL

        /// <summary>
        /// Random object to use in this class by using the Next(...) method.
        /// This avoids getting the same number generated when creating several new Random() too fast.
        /// </summary>
        private static Random random = new Random();

        /// <summary>
        /// Type of filter to apply in miscellaneous operations.
        /// </summary>
        public enum FilterType
        {
            /// <summary>
            /// Keep the items in the filter (all others are removed).
            /// </summary>
            Keep,
            /// <summary>
            /// Remove the items in the filter (leave all others).
            /// </summary>
            Remove
        }

        /// <summary>
        /// Which algorithm to use when generating a check digit with the CheckDigit(...) function.
        /// </summary>
        public enum CheckDigitAlgorithm
        {
            A,
            B
        }

        /// <summary>
        /// Encrypts the string using the key in this class.
        /// </summary>
        /// <param name="strUnencrypted"></param>
        /// <returns></returns>
        public static string Encrypt(string strUnencrypted)
        {
            return Encrypt(strUnencrypted, encryptionKey);
        }

        /// <summary>
        /// Encrypts the string using the given key.
        /// They will be needed to decrypt.
        /// </summary>
        /// <param name="strUnencrypted"></param>
        /// <returns></returns>
        public static string Encrypt(string strUnencrypted, string key)
        {
            // Code snippet from http://www.deltasblog.co.uk/code-snippets/basic-encryptiondecryption-c/
            byte[] inputArray = UTF8Encoding.UTF8.GetBytes(strUnencrypted);
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string strEncrypted)
        {
            return Decrypt(strEncrypted, encryptionKey);
        }

        public static string Decrypt(string strEncrypted, string key)
        {
            if (string.IsNullOrEmpty(strEncrypted))
            {
                return "";
            }

            // Code snippet from http://www.deltasblog.co.uk/code-snippets/basic-encryptiondecryption-c/
            byte[] inputArray = Convert.FromBase64String(strEncrypted);
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tripleDES.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();

            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public static void SerializeToXmlFile(Object obj, string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            TextWriter tw = new StreamWriter(fileName);

            try
            {
                serializer.Serialize(tw, obj);
            }
            finally
            {
                tw.Close();
            }
        }

        /// <summary>
        /// Deserializes an object from an XML file.
        /// The file must have that object only.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Object DeserializeFromXmlFile(string fileName, Type type)
        {
            return DeserializeFromStream(new FileStream(fileName, FileMode.Open), type);
        }

        public static Object DeserializeFromString(string str, Type type)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(str);
            return DeserializeFromStream(new MemoryStream(byteArray), type);
        }

        public static Object DeserializeFromStream(Stream stream, Type type)
        {
            XmlSerializer serializer = new XmlSerializer(type);
            Object obj;
            try
            {
                obj = serializer.Deserialize(stream);
            }
            finally
            {
                stream.Close();
            }

            return obj;
        }

        /// <summary>
        /// Method to convert a custom Object to XML string
        /// </summary>
        /// <param name="obj">Object that is to be serialized to XML</param>
        /// <returns>XML string</returns>
        public static string SerializeToString(Object obj)
        {
            MemoryStream memoryStream = new MemoryStream();
            XmlSerializer xs = new XmlSerializer(obj.GetType());
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);

            xs.Serialize(xmlTextWriter, obj);
            memoryStream = (MemoryStream)xmlTextWriter.BaseStream;

            return System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        /// <summary>
        /// Saves the strings to a file, each parameter (or entry in array) in a separate line.
        /// Overrites the existing file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="lines"></param>
        public static void SaveToFile(string fileName, params string[] lines)
        {
            SaveToFile(fileName, true, lines);
        }

        /// <summary>
        /// Saves the strings to a file, each parameter (or entry in array) in a separate line.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="overwrite"></param>
        /// <param name="lines"></param>
        public static void SaveToFile(string fileName, bool overwrite, params string[] lines)
        {
            StreamWriter fileWriter = null;

            try
            {
                if (overwrite)
                {
                    fileWriter = File.CreateText(fileName);
                }
                else
                {
                    fileWriter = File.AppendText(fileName);
                }

                foreach (string line in lines)
                {
                    fileWriter.Write(line);
                }
            }
            finally
            {
                if (fileWriter != null)
                {
                    fileWriter.Close();
                }
            }
        }

        /// <summary>
        /// Loads a file into a string.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string LoadFromFile(string fileName)
        {
            StreamReader streamReader = new StreamReader(fileName);
            string text = streamReader.ReadToEnd();
            streamReader.Close();

            return text;
        }

        /// <summary>
        /// Lookup a resource string through reflection.
        /// Useful for getting localized attributes at runtime.
        /// </summary>
        /// <param name="resourceManagerProvider"></param>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        public static string LookupResource(Type resourceManagerProvider, string resourceKey)
        {
            foreach (PropertyInfo staticProperty in resourceManagerProvider.GetProperties(BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (staticProperty.PropertyType == typeof(System.Resources.ResourceManager))
                {
                    System.Resources.ResourceManager resourceManager = (System.Resources.ResourceManager)staticProperty.GetValue(null, null);
                    return resourceManager.GetString(resourceKey);
                }
            }
            return resourceKey;  // Fallback with the key name
        }

        /// <summary>
        /// Gets a list with the name of the fields from a class type through Reflection.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> GetFieldNames(Type type)
        {
            string instance = "";
            List<string> items = new List<string>();
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                items.Add(fieldInfo.GetValue(instance).ToString());
            }
            items.Sort();

            return items;
        }

        /// <summary>
        /// Checks to see if the instance is or derives from a generic type.
        /// Example to check if an object is or derives from List:
        /// bool isList = IsInstanceOfGenericType(typeof(List<>), new List<string>()));  // True
        /// bool isList = IsInstanceOfGenericType(typeof(List<>), new string[0]));  // False
        /// </summary>
        /// <param name="genericType"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsInstanceOfGenericType(Type genericType, object obj)
        {
            Type type = obj.GetType();
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the object's type represents a number.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNumeric(object obj)
        {
            return obj != null && IsNumeric(obj.GetType());
        }

        /// <summary>
        /// Returns true if the type represents a number.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNumeric(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);

            return typeCode == TypeCode.Int16 || typeCode == TypeCode.Int32 || typeCode == TypeCode.Int64 ||
                typeCode == TypeCode.Decimal || typeCode == TypeCode.Single || typeCode == TypeCode.Double ||
                typeCode == TypeCode.UInt16 || typeCode == TypeCode.UInt32 || typeCode == TypeCode.UInt64 ||
                typeCode == TypeCode.Byte || typeCode == TypeCode.SByte;
        }

        /// <summary>
        /// Returns true if the object's type represents a string (including chars).
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsString(object obj)
        {
            return obj != null && IsString(obj.GetType());
        }

        /// <summary>
        /// Returns true if the type represents a string (including chars).
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsString(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);

            return typeCode == TypeCode.String || typeCode == TypeCode.Char;
        }

        /// <summary>
        /// Returns true if the object's type represents a date.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsDate(object obj)
        {
            return obj != null && IsDate(obj.GetType());
        }

        /// <summary>
        /// Returns true if the type represents a date.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsDate(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);

            return typeCode == TypeCode.DateTime;
        }

        /// <summary>
        /// Returns true if the object's type represents a boolean.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsBoolean(object obj)
        {
            return obj != null && IsBoolean(obj.GetType());
        }

        /// <summary>
        /// Returns true if the type represents a boolean.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBoolean(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);

            return typeCode == TypeCode.Boolean;
        }

        /// <summary>
        /// Returns true if the object's type represents a number with decimals.
        /// Note that it can be a type other than decimal (ex. float).
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsDecimal(object obj)
        {
            return obj != null && IsDecimal(obj.GetType());
        }

        /// <summary>
        /// Returns true if the type represents a number with decimals.
        /// Note that it can be a type other than decimal (ex. float).
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsDecimal(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);

            return typeCode == TypeCode.Decimal || typeCode == TypeCode.Single || typeCode == TypeCode.Double;
        }

        /// <summary>
        /// Returns true if the object's type represents an integral (no decimals).
        /// Note that it can be a type other than int (ex. long).
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsIntegral(object obj)
        {
            return obj != null && IsIntegral(obj.GetType());
        }

        /// <summary>
        /// Returns true if the type represents an integral (no decimals).
        /// Note that it can be a type other than int (ex. long).
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsIntegral(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);

            return typeCode == TypeCode.Int16 || typeCode == TypeCode.Int32 || typeCode == TypeCode.Int64 ||
                typeCode == TypeCode.UInt16 || typeCode == TypeCode.UInt32 || typeCode == TypeCode.UInt64 ||
                typeCode == TypeCode.Byte || typeCode == TypeCode.SByte;
        }

        /// <summary>
        /// Deep copy / clone an object.
        /// Class T MUST be marked as [Serializable], as well as any other classes inside it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object to deep clone.</param>
        /// <returns></returns>
        // Code snippet taken from link below. Check it out for more info.
        // http://stackoverflow.com/questions/129389/how-do-you-do-a-deep-copy-an-object-in-net-c-specifically
        public static T DeepCopy<T>(T obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter =
                    new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        /// <summary>
        /// Generate a random string.
        /// </summary>
        /// <param name="length">Length of the string.</param>
        /// <param name="includeLower">Include lowercase characters.</param>
        /// <param name="includeUpper">Include uppercase characters.</param>
        /// <param name="includeNumbers">Include numbers.</param>
        /// <param name="otherChars">Other characters to include, such as !#$ or spaces.</param>
        /// <returns></returns>
        public static string GenerateRandom(int length, bool includeLower, bool includeUpper, bool includeNumbers, string otherChars)
        {
            string charPool = "";

            if (includeLower)
            {
                charPool += "abcdefghijklmnopqrstuvwxyz";
            }
            if (includeUpper)
            {
                charPool += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            }
            if (includeNumbers)
            {
                charPool += "0123456789";
            }
            if (!string.IsNullOrEmpty(otherChars))
            {
                charPool += otherChars;  // Not checking for duplicate chars
            }

            return GenerateRandom(length, charPool);
        }

        /// <summary>
        /// Generate a random string with characters from the character pool.
        /// </summary>
        /// <param name="length">Length of the string.</param>
        /// <param name="charPool">Character pool to generate the random string.</param>
        /// <returns></returns>
        public static string GenerateRandom(int length, string charPool)
        {
            if (string.IsNullOrEmpty(charPool))
            {
                throw new Exception("Character pool cannot be empty.");
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(charPool[random.Next(charPool.Length)]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a temporary folder. Creates one if it doesn't exist. Does not contain '\' at the end.
        /// This is a temporary folder inside the system's temp folder that is specific to this application.
        /// For the given application, it always starts with the same name. If instance specific is selected, a random string is appended at the end.
        /// If you want to delete all instances of the temp folder, delete all the folders that start with the call to this function with instanceSpecific=false.
        /// If you need the system's temp folder, use Path.GetTempPath().
        /// </summary>
        /// <param name="instanceSpecific">Whether it's specific to the running instance (true) or just specific to the application (false).</param>
        /// <returns></returns>
        public static string GetTempFolder(bool instanceSpecific)
        {
            string tempFolder = string.Format(@"{0}\{1}", Path.GetTempPath().Trim('\\'), AppDomain.CurrentDomain.FriendlyName.Split('.')[0]);

            if (IsDebug())
            {
                tempFolder += "_DEBUG";
            }

            if (instanceSpecific)
            {
                tempFolder += "_" + instanceSpecificName;
            }

            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            return tempFolder;
        }

        /// <summary>
        /// Deletes the files that match the search criteria in the given folder.
        /// </summary>
        /// <param name="folder">Folder to run the search criteria.</param>
        /// <param name="search">The search criteria.</param>
        /// <param name="searchOption">Whther to include the top directory only or all subdirectories as well.</param>
        /// <returns>The number of files deleted.</returns>
        public static int DeleteFiles(string folder, string search, SearchOption searchOption)
        {
            DirectoryInfo di = new DirectoryInfo(folder);

            StringBuilder error = new StringBuilder();
            int i = 0;
            int nrErrors = 0;
            if (di.Exists)
            {
                FileInfo[] files = di.GetFiles(search, searchOption);
                foreach (FileInfo fi in files)
                {
                    try
                    {
                        fi.Delete();
                        i++;
                    }
                    catch (Exception ex)
                    {
                        nrErrors++;
                        error.AppendLine(ex.Message);
                    }
                }
            }

            if (nrErrors > 0)
            {
                throw new Exception(string.Format("Error deleting {0} file{1}:\n{2}",
                    nrErrors, nrErrors == 1 ? "" : "s", error.ToString()));
            }

            return i;
        }

        // DELETE BELOW IF SAME AS object.Equals(object objA, object objB)
        ///// <summary>
        ///// Returns true if the two objects are equal.
        ///// Takes null under consideration and uses the objects' Equal() method.
        ///// </summary>
        ///// <param name="obj1"></param>
        ///// <param name="obj2"></param>
        ///// <returns></returns>
        //public static bool Equal(object obj1, object obj2)
        //{
        //    if (obj1 == null && obj2 == null)
        //        return true;

        //    if (obj1 == null ^ obj2 == null)
        //        return false;

        //    if (obj1.GetType() != obj2.GetType())
        //        return false;

        //    return obj1.Equals(obj2);
        //}

        /// <summary>
        /// Creates a check digit for the string representing a number.
        /// Note that the string may have zeroes to the left and that will generate different results.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="size"></param>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        public static int CheckDigit(string number, CheckDigitAlgorithm algorithm)
        {
            double sum = 0;
            double x1 = algorithm == CheckDigitAlgorithm.A ? 1 : 2;
            double x2 = algorithm == CheckDigitAlgorithm.A ? 3 : 4;
            for (int i = 0; i < number.Length; i++)
            {
                double n = Convert.ToDouble(number[i].ToString());
                sum = sum + ((n % 2 == 0) ? (n * x1) : (n * x2));
            }

            return Convert.ToInt32(((Math.Ceiling(sum / 10) * 10) - sum));
        }

        /// <summary>
        /// Returns the details of the exception, data and inner exceptions.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetExceptionDetails(Exception ex)
        {
            StringBuilder error = new StringBuilder();

            int i = 0;
            while (ex != null)
            {
                error.AppendLine(string.Format("Erro {0}:", ++i));
                error.AppendLine(ex.Message);
                error.AppendLine();

                if (ex.Data != null && ex.Data.Keys.Count > 0)
                {
                    error.AppendLine("Data:");
                    error.AppendLine(StringUtil.ToString(ex.Data));
                    error.AppendLine();
                }

                ex = ex.InnerException;
            }

            return error.ToString();
        }

        /// <summary>
        /// Returns true if the application is running in Debug mode, false otherwise.
        /// </summary>
        /// <returns></returns>
        public static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}