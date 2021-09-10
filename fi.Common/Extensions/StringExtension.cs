using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace fi.Common
{
    public static class StringExtension
    {
        private static readonly Encoding Encoding = Encoding.GetEncoding("Cyrillic");

        public static bool IsNullOrEmpty(this string text) => string.IsNullOrEmpty(text);
        public static bool IsNotNullOrEmpty(this string text) => !text.IsNullOrEmpty();
        public static bool IsNullOrWhiteSpace(this string text) => string.IsNullOrWhiteSpace(text);
        public static bool IsNotNullOrWhiteSpace(this string text) => !text.IsNullOrWhiteSpace();
        public static bool IsNullOrWhiteSpace(this string source, char splitValue) => source.IsNullOrWhiteSpace() || source.Split(splitValue).Length <= 0;
        public static bool IsNotNullOrWhiteSpace(this string source, char splitValue) => !source.IsNullOrWhiteSpace(splitValue);
        public static string GetNumbers(this string text) => new(text.Where(char.IsDigit).ToArray());
        public static string ClearProtocols(this string source) => source.IsNullOrWhiteSpace() ? source : source.Replace("http://", "//").Replace("https://", "//");
        public static string ClearPhoneFaxMask(this string phone) => phone.IsNullOrWhiteSpace() ? phone : phone.Trim().Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");
        public static string UrlEncode(this string value) => value.IsNullOrWhiteSpace() ? value : System.Web.HttpUtility.UrlEncode(value);
        public static string UrlDecode(this string value) => value.IsNullOrWhiteSpace() ? value : System.Web.HttpUtility.UrlDecode(value);
        public static bool EndsWith(this string value, string[] array) => array.Any(item => value.EndsWith(item, StringComparison.InvariantCultureIgnoreCase));
        public static bool IsMatch(this string value, Regex regex) => regex.IsMatch(value);
        public static string ClearWhiteSpaces(this string value) => value.Replace(" ", "").Replace(Environment.NewLine, "");
        public static bool IsDateTime(this string data, string dateFormat) => DateTime.TryParseExact(data, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _);
        public static bool IsInteger(this string val) => Int32.TryParse(val, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out int _);

        public static string GetFirstname(this string text)
        {
            char[] delimiters = { '\r', '\n', ' ' };
            string[] parts = text.Trim().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            if (!parts.Any())
                return string.Empty;

            if (parts.Length <= 1)
                return string.Join(" ", parts);

            string firstName = string.Empty;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                firstName = string.Format("{0} {1}", firstName, parts[i]);
            }

            return firstName.Trim();
        }

        public static string GetSurname(this string text)
        {
            char[] delimiters = { '\r', '\n', ' ' };
            string lastWord = text.Trim()
                .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault().Trim();
            if (text.Trim().Equals(lastWord))
                return string.Empty;

            return lastWord;
        }

        public static string Truncate(this string text, int maxLength)
        {
            if (text.IsNullOrWhiteSpace() || maxLength <= 0)
                return string.Empty;

            if (text.Length > maxLength)
                return text.Substring(0, maxLength) + "...";

            return text;
        }

        public static bool IsValidCitizenshipNumber(this string text)
        {
            if (text.IsNullOrWhiteSpace())
                return false;

            var expression = text.GetNumbers();
            if (expression.Length > 18)
                return false;

            if (!expression.IsNumeric())
                return false;

            if (expression.Trim().Length != 11)
                return false;

            long number = long.Parse(expression);
            if (number.ToString(CultureInfo.InvariantCulture).Length != 11)
                return false;

            long citizenshipNumber = long.Parse(expression);
            long atcno = citizenshipNumber / 100;
            long btcno = citizenshipNumber / 100;
            long c1 = atcno % 10;
            atcno = atcno / 10;
            long c2 = atcno % 10;
            atcno = atcno / 10;
            long c3 = atcno % 10;
            atcno = atcno / 10;
            long c4 = atcno % 10;
            atcno = atcno / 10;
            long c5 = atcno % 10;
            atcno = atcno / 10;
            long c6 = atcno % 10;
            atcno = atcno / 10;
            long c7 = atcno % 10;
            atcno = atcno / 10;
            long c8 = atcno % 10;
            atcno = atcno / 10;
            long c9 = atcno % 10;

            long q1 = ((10 - ((((c1 + c3 + c5 + c7 + c9) * 3) + (c2 + c4 + c6 + c8)) % 10)) % 10);
            long q2 = ((10 - (((((c2 + c4 + c6 + c8) + q1) * 3) + (c1 + c3 + c5 + c7 + c9)) % 10)) % 10);

            bool returnvalue = ((btcno * 100) + (q1 * 10) + q2 == citizenshipNumber);
            return returnvalue;
        }

        public static bool IsValidMailAddress(this string text)
        {
            if (text.IsNullOrEmpty())
                return false;
            var isEmail = Regex.IsMatch(text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            return isEmail;
        }

        public static bool IsValidEmailAdress(this string text)
        {
            try
            {
                var m = new System.Net.Mail.MailAddress(text);
                return !string.IsNullOrWhiteSpace(m.Address);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsValidLicensePlateNumber(this string expression)
        {
            try
            {
                if (expression.IsNullOrWhiteSpace())
                    return false;

                expression = expression.RemoveWhiteSpace();
                var regex = new Regex(@"^(0[1-9]|[1-7][0-9]|8[01])(([a-zA-Z])(\d{4,5})|([a-zA-Z]{2})(\d{2,4})|([a-zA-Z]{3})(\d{2,3}))", RegexOptions.CultureInvariant);
                var match = regex.Match(expression);
                return match.Success;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsValidIban(this string expression)
        {
            if (expression.IsNullOrWhiteSpace())
                return false;

            expression = expression.Trim().ToUpper(); //IN ORDER TO COPE WITH THE REGEX BELOW
            if (Regex.IsMatch(expression, "^[A-Z0-9]"))
            {
                expression = expression.Replace(" ", string.Empty);
                string bank = expression.Substring(4, expression.Length - 4) + expression.Substring(0, 4);
                const int asciiShift = 55;

                var sb = new StringBuilder();
                foreach (var c in bank)
                {
                    int v;
                    if (Char.IsLetter(c))
                        v = c - asciiShift;
                    else
                        v = int.Parse(c.ToString(CultureInfo.InvariantCulture));
                    sb.Append(v);
                }

                string checkSumString = sb.ToString();
                int checksum = int.Parse(checkSumString.Substring(0, 1));
                for (int i = 1; i < checkSumString.Length; i++)
                {
                    int v = int.Parse(checkSumString.Substring(i, 1));
                    checksum *= 10;
                    checksum += v;
                    checksum %= 97;
                }

                return checksum == 1;
            }

            return false;
        }

        public static string RemoveWhiteSpace(this string text)
        {
            if (text.IsNullOrWhiteSpace())
                return string.Empty;

            char[] delimiters = { '\r', '\n', ' ' };
            var parts = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            if (!parts.Any())
                return string.Empty;

            return string.Join(" ", parts);
        }

        public static int[] ToIntArray(this string expression, char? sep)
        {
            var sa = !sep.HasValue ? expression.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray() : expression.Split(sep.Value);
            var ia = new int[sa.Length];

            for (var i = 0; i < ia.Length; ++i)
            {
                var s = sa[i];
                if (int.TryParse(s, out int j))
                {
                    ia[i] = j;
                }
            }

            return ia;
        }

        public static bool IsValidPhoneNumber(this string expression)
        {
            try
            {
                if (expression.IsNullOrWhiteSpace())
                    return false;

                expression = expression.Trim();
                expression = expression.RemoveWhiteSpace();

                if (expression.Length > 10)
                {
                    var phonePrefixs = new List<string>
                    {
                        "0090",
                        "+90",
                        "0"
                    };

                    const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;
                    foreach (var prefix in phonePrefixs)
                    {
                        if (expression.StartsWith(prefix, comparison))
                        {
                            expression = expression.Remove(0, prefix.Length);
                        }
                    }
                }

                if (!expression.IsNumeric())
                    return false;

                if (expression.Length != 10)
                    return false;

                var isHasSequence = expression.Substring(4).ToIntArray(null).HasSequence(5);
                if (isHasSequence)
                    return false;

                var regexTrLocalPhone = new Regex(@"^[2-4][1-9][0,2,4,6,8][0-9]{7}$", RegexOptions.IgnoreCase);
                var matchTrLocalPhone = regexTrLocalPhone.Match(expression);

                var regexTrMobilePhone = new Regex(@"^[5][0,3,4,5,6][0-9][0-9]{7}$", RegexOptions.IgnoreCase);
                var matchTrMobilePhone = regexTrMobilePhone.Match(expression);

                return matchTrLocalPhone.Success || matchTrMobilePhone.Success;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static string Right(this string text, int length)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Length <= length ? text : text.Substring(text.Length - length);
        }

        public static string Left(this string text, int length)
        {
            if (text.IsNullOrWhiteSpace())
                return text;

            length = Math.Abs(length);
            return (text.Length <= length ? text : text.Substring(0, length));
        }

        public static string ClearTurkishChars(this string value)
        {
            var sb = new StringBuilder(value);
            sb = sb.Replace("ı", "i")
                   .Replace("ğ", "g")
                   .Replace("ü", "u")
                   .Replace("ş", "s")
                   .Replace("ö", "o")
                   .Replace("ç", "c")
                   .Replace("İ", "I")
                   .Replace("Ğ", "G")
                   .Replace("Ü", "U")
                   .Replace("Ş", "S")
                   .Replace("Ö", "O")
                   .Replace("Ç", "C");

            return sb.ToString();
        }

        public static bool IsMatch(this string value, string pattern)
        {
            var regex = new Regex(pattern);
            return value.IsMatch(regex);
        }

        public static string RemoveAccent(this string value)
        {
            var bytes = StringExtension.Encoding.GetBytes(value);
            return Encoding.ASCII.GetString(bytes);
        }

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            var pos = text.IndexOf(search, StringComparison.Ordinal);
            if (pos < 0)
                return text;

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string ToDashCase(this string input)
        {
            const string pattern = "[A-Z]";
            const string dash = "-";
            return Regex.Replace(input, pattern, m => (m.Index == 0 ? string.Empty : dash) + m.Value.ToLowerInvariant());
        }

        public static DateTime ToDateTime(this string value, DateTime defaultValue = default)
        {
            if (value.IsNullOrWhiteSpace())
                return defaultValue;

            DateTime datetimeValue;
            if (DateTime.TryParse(value, out datetimeValue))
                return datetimeValue;

            if (DateTime.TryParseExact(value, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out datetimeValue))
                return datetimeValue;

            return defaultValue;
        }

        public static DateTime? ToNullableDateTime(this string value)
        {
            if (value.IsNullOrWhiteSpace())
                return null;

            if (DateTime.TryParse(value, out DateTime datetimeValue))
                return datetimeValue;

            if (DateTime.TryParseExact(value, "dd.MM.yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out datetimeValue))
                return datetimeValue;

            throw new InvalidCastException($"{value} tarih formatına çevrilemiyor.");
        }

        public static short ToInt16(this string value)
        {
            short result = 0;

            if (value.IsNotNullOrWhiteSpace())
                _ = short.TryParse(value, out result);

            return result;
        }

        public static int ToInt32(this string value)
        {
            var result = 0;

            if (value.IsNotNullOrWhiteSpace())
                int.TryParse(value, out result);

            return result;
        }

        public static long ToInt64(this string value)
        {
            long result = 0;

            if (value.IsNotNullOrWhiteSpace())
                _ = long.TryParse(value, out result);

            return result;
        }

        public static decimal ToDecimal(this string value, decimal @default = 0)
        {
            decimal result = @default;

            if (value.IsNotNullOrWhiteSpace())
                _ = decimal.TryParse(value.Replace(".", ","), out result);

            return result;
        }

        public static decimal? ToNullableDecimal(this string value)
        {
            decimal? result = null;
            try
            {
                if (value.IsNotNullOrWhiteSpace())
                    result = decimal.Parse(value.Replace(".", ","));
            }
            catch
            {
                //ignored 
            }
            return result;
        }

        public static Guid ToGuid(this string value) => Guid.TryParse(value, out Guid result) ? result : Guid.Empty;

        public static Guid? ToNullableGuid(this string value)
        {
            var result = value.ToGuid();
            return result.IsEmpty() ? null : result;
        }

        public static string GetCreditCardPrefix(this string cardNumber)
        {
            var cleanedCardNumber = cardNumber.Trim().Replace(" ", "");
            if (cleanedCardNumber.Length < 6)
                return cleanedCardNumber;
            return cleanedCardNumber.Substring(0, 6);
        }

        public static string Reverse(this string val)
        {
            var chars = new char[val.Length];
            for (int i = val.Length - 1, j = 0; i >= 0; --i, ++j)
            {
                chars[j] = val[i];
            }
            val = new String(chars);
            return val;
        }

        public static string ToKMBOnlyEx(this decimal num)
        {
            if (num >= 1000000000)
            {
                return "Milyar +";
            }
            else if (num >= 1000000)
            {
                return "Milyon +";
            }
            else if (num >= 1000)
            {
                return "Bin +";
            }
            return string.Empty;
        }

        public static string FormatNumberEx(this double num)
        {
            var i = Math.Pow(10, (int)Math.Max(0, Math.Log10(num) - 2));
            num = num / i * i;

            if (num >= 1000000000)
                return "Milyar +";
            if (num >= 1000000)
                return "Milyon +";
            if (num >= 1000)
                return "Bin +";

            return string.Empty;
        }

        public static uint ToTryuInt(this string s)
        {
            if (uint.TryParse(s, out uint result))
                return result;

            return uint.MinValue;
        }

        public static string RemoveSpecialCharacters(this string input, string allowedCharacters = null)
        {
            char[] buffer = new char[input.Length];
            int index = 0;

            var allowedSpecialCharacters = Array.Empty<char>();
            if (allowedCharacters != null)
                allowedSpecialCharacters = allowedCharacters.ToCharArray();

            foreach (char c in input.Where(c => char.IsLetterOrDigit(c) || allowedSpecialCharacters.Any(x => x == c)))
            {
                buffer[index] = c;
                index++;
            }

            return new string(buffer, 0, index);
        }

        public static bool ContainsAny(this string str, params string[] values)
        {
            if (!string.IsNullOrEmpty(str) || values.Length > 0)
            {
                foreach (string value in values)
                {
                    if (str.ToLower().Equals(value.ToLower()))
                        return true;
                }
            }

            return false;
        }

        public static bool IsLetterOrDigit(this string input)
        {
            foreach (char c in input.Where(c => !char.IsLetterOrDigit(c)))
            {
                return false;
            }
            return true;
        }
    }
}
