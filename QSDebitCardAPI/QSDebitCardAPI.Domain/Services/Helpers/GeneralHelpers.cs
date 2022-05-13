using System;

namespace QSDebitCardAPI.Domain.Services.Helpers
{
    public class GeneralHelpers
    {
        public static string GenerateRequestTranID()
        {
            int _min = 100000001;
            int _max = 999999999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max).ToString();
        }

        /// <summary>
        /// Add +234(0) to phone number
        /// </summary>
        /// <param name="phoneNumber">Phone number</param>
        /// <returns>+234(0)PhoneNumber</returns>
        public static string FormatPhoneNumberWithLocalCode(string phoneNumber)
        {
            return "+234(0)" + phoneNumber.Substring(1);
        }

        public string SanitizePhoneNumber(string phoneNumber)
        {
            string tempMobileNo = phoneNumber;
            var phoneNumberPrefix = "234";
            tempMobileNo = tempMobileNo.Replace(" ", "")
                                        .Replace("+", "")
                                        .Replace("(0", "")
                                        .Replace("(", "")
                                        .Replace(")", "");

            if (tempMobileNo.Length > 11 && tempMobileNo.StartsWith(phoneNumberPrefix))
            {
                return tempMobileNo;
            }
            else if (tempMobileNo.Length >= 10 && !tempMobileNo.StartsWith(phoneNumberPrefix))
            {
                return phoneNumberPrefix + tempMobileNo.Substring(Math.Max(0, tempMobileNo.Length - 10));
            }
            else
            {
                return phoneNumberPrefix + phoneNumber;
            }
        }
    }
}