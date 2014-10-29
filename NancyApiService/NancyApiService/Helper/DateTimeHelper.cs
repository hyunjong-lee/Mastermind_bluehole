using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NancyApiService.Helper
{
    public static class DateTimeHelper
    {
        public static DateTime ToDateTime(this int date)
        {
            return new DateTime(date / 10000, (date % 10000) / 100, date % 100);
        }

        public static int ToInt(this DateTime dateTime)
        {
            return dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day;
        }

        public static int AddDays(this int date, int days)
        {
            return date.ToDateTime().AddDays(days).ToInt();
        }

        public static List<int> DateRange(this int fromDate, int toDate)
        {
            var dateList = new List<int>();
            while (fromDate <= toDate)
            {
                dateList.Add(fromDate);
                fromDate = fromDate.AddDays(1);
            }

            return dateList;
        }

        public static string ToDateString(this int date)
        {
            return string.Format("{0}년 {1}월 {2}일", date / 10000, (date % 10000) / 100, date % 100);
        }
    }
}