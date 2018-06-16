using LibraryData.Models;
using System;
using System.Collections.Generic;


namespace LibraryServices
{
    public class DataHelpers
    {

        public static IEnumerable<string> HumanizeBizHours(IEnumerable<BranchHours> branchHours)
        {
            var hours = new List<string>();

            foreach( var time in branchHours)
            {
                var day = HumanizeDay(time.DayOfWeek);
                var  openTime = HumanizeTime(time.OpenTime);
                var closeTime = HumanizeTime(time.CloseTime);
                var timeEntry = $"{day} { openTime} to {closeTime}";
            }
            return hours;
        }
        public  static string HumanizeDay(int number)
        {
            // our date correlate 1 --> sunday
            return Enum.GetName(typeof(DayOfWeek), number-1);
        }
        public static string HumanizeTime(int time)
        {
            return TimeSpan.FromHours(time).ToString("hh':'mm");
        }


    }
}
