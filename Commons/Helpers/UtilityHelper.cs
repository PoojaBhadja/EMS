using Azure.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Helpers
{
    public class UtilityHelper
    {
        public static bool IsExcelFile(IFormFile file)
        {
            var extention = "." + file.FileName.Split(".")[file.FileName.Split(".").Length - 1];
            return (extention == ".xlsx" || extention == ".xls");
        }

        public static DateTime GetIndianTimeZoneDatetime(DateTime? dateTime = null)
        {
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            // Convert received UTC DateTime to IST
            DateTime istDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime ?? DateTime.UtcNow, istZone);

            // Get current IST time
            DateTime currentISTTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            // Set the received date with the current IST time
            DateTime finalISTDateTime = new DateTime(istDateTime.Year, istDateTime.Month, istDateTime.Day,
                                                     currentISTTime.Hour, currentISTTime.Minute, currentISTTime.Second);

            return finalISTDateTime;
        }
    }
}
