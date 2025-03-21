using Commons.Constants;
using Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons.Extentions
{
    public static class UtilityExtention
    {
        public static string ConvertDecimalToIndianRs<T>(this T obj)
        {
            return Convert.ToDecimal(obj).ToString("C", CultureInfo.CreateSpecificCulture("en-IN"));
        }

    }
}

