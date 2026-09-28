// Ignore Spelling: DTO SRT

using SRT.Core.SRTExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SRT.Core.GeneralFunctions.Persons
{
    public class NationalCodeCheck
    {
        public static bool isNationalCodeCheck(string nationalCode)
        {
            if (nationalCode.Length != 10)
                return false;

            if (!nationalCode.All(char.IsDigit))
                return false;

            // همه ارقام یکسان (مثل 0000000000 یا 5555555555) نامعتبر است
            // حتی اگر از checksum رد شوند
            if (nationalCode.All(c => c == nationalCode[0]))
                return false;

            int[] coefficients = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int checkDigit = int.Parse(nationalCode[9].ToString());
            int sum = 0;

            for (int i = 0; i < 9; i++)
            {
                sum += (int.Parse(nationalCode[i].ToString()) * coefficients[i]);
            }

            int remainder = sum % 11;

            if (remainder < 2)
            {
                return checkDigit == remainder;
            }
            else
            {
                return checkDigit == (11 - remainder);
            }
        }

        public static bool isShebaCheck(string input)
        {
            if (input.SRT_String_IsNullOrEmpty())
                return false;

            input = input.Replace(" ", "").ToLower();
            //بررسی رشته وارد شده برای اینکه در فرمت شبا باشد
            var isSheba = Regex.IsMatch(input, "^[a-zA-Z]{2}\\d{2} ?\\d{4} ?\\d{4} ?\\d{4} ?\\d{4} ?[\\d]{0,2}",
                RegexOptions.Compiled);

            if (!isSheba)
                return false;
            //طول شماره شبا را چک میکند کمتر نباشد
            if (input.Length < 26)
                return false;
            input = input.ToLower();
            //بررسی اعتبار سنجی اصلی شبا
            ////ابتدا گرفتن چهار رقم اول شبا
            var get4FirstDigit = input.Substring(0, 4);
            ////جایگزین کردن عدد 18 و 27 به جای آی و آر
            var replacedGet4FirstDigit = get4FirstDigit.ToLower().Replace("i", "18").Replace("r", "27");
            ////حذف چهار رقم اول از رشته شبا
            var removedShebaFirst4Digit = input.Replace(get4FirstDigit, "");
            ////کانکت کردن شبای باقیمانده با جایگزین شده چهار رقم اول
            var newSheba = removedShebaFirst4Digit + replacedGet4FirstDigit;
            ////تبدیل کردن شبا به عدد  - دسیمال تا 28 رقم را نگه میدارد
            var finalLongData = Convert.ToDecimal(newSheba);
            ////تقسیم عدد نهایی به مقدار 97 - اگر باقیمانده برابر با عدد یک شود این رشته شبا صحیح خواهد بود
            var finalReminder = finalLongData % 97;
            return finalReminder == 1;
        }
    }
}
