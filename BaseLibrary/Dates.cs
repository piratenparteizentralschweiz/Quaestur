﻿using System;
using System.Globalization;

namespace BaseLibrary
{
    public static class Dates
    {
        public static string FormatIso(this DateTime value)
        {
            return value.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
        }

        public static DateTime ParseIsoDate(this string value)
        {
            return DateTime.ParseExact(value, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
        }

        public static bool TryParseIsoDate(string value, out DateTime date)
        {
            return DateTime.TryParseExact(value, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date);
        }

        public static TimeSpan ComputeOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            // make sure range 1 is shorter than range 2
            if (end1.Subtract(start1).TotalDays > end2.Subtract(start2).TotalDays)
            {
                return ComputeOverlap(start2, end2, start1, end1); 
            }

            // =>  start1 |         end1 |                   
            // =>           start2 |                   end2 |
            if (end1 >= start2 && end1 <= end2 && start1 <= start2)
            {
                return end1.Subtract(start2);
            }
            // =>             start1 |         end1 |        
            // =>           start2 |                   end2 |
            else if (start1 >= start2 && end1 <= end2)
            {
                return end1.Subtract(start1);
            }
            // =>                           start1 |         end1 |
            // =>           start2 |                   end2 |      
            else if (start1 >= start2 && start1 <= end2 && end1 >= end2)
            {
                return end2.Subtract(start1);
            }
            else
            {
                return new TimeSpan(0, 0, 0, 0); 
            }
        }
    }
}
