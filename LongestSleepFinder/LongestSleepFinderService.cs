using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LongestSleepFinder
{
    public class LongestSleepFinderService
    {
        private const string DatePattern = "ddd dd/MM/yyyy";
        private const string TimePattern = "HH:mm";
        private const string IncorrectDayEndFormat = "24:00";

        private int DaysFromMonday = DayOfWeek.Monday - DateTime.UtcNow.DayOfWeek;

        private DateTime ToDayStart(DateTime dateTime)
        {
            const string dayStart = "00:00";
            
            return DateTime.ParseExact($"{dateTime.Date.ToString($"{DatePattern} {dayStart}")}", $"{DatePattern} {TimePattern}", CultureInfo.InvariantCulture);
        }
        
        private DateTime ToSpecificTime(DateTime dateTime, string time)
        {
            return DateTime.ParseExact($"{dateTime.Date.ToString($"{DatePattern} {time}")}", $"{DatePattern} {TimePattern}", CultureInfo.InvariantCulture);
        }

        public int Find(string schedule)
        {
            var weekStart = ToDayStart(DateTime.UtcNow.AddDays(DaysFromMonday));
            var weekEnd = ToDayStart(DateTime.UtcNow.AddDays(DaysFromMonday + 7));

            var meetingIntervals = GetOrderedMeetingIntervals(weekStart, schedule);

            var longestSleep = default(int);
            var startingPointToCheck = weekStart;

            foreach (var meeting in meetingIntervals)
            {
                var currentSleep = meeting.Start - startingPointToCheck;

                longestSleep = Math.Max(longestSleep, (int)currentSleep.TotalMinutes);

                startingPointToCheck = meeting.End;
            }

            var sleepFromLastMeetingUntilWeekEnd = weekEnd - startingPointToCheck;

            return Math.Max(longestSleep, (int)sleepFromLastMeetingUntilWeekEnd.TotalMinutes);
        }

        private IReadOnlyCollection<MeetingInterval> GetOrderedMeetingIntervals(DateTime weekStart, string schedule)
        {
            var meetingsInAWeek = schedule
                .Split(Environment.NewLine)
                .Select(m =>
                {
                    var meetingScheduleData = m.Split(new[] {' ', '-'}, StringSplitOptions.RemoveEmptyEntries);
                    var abbreviatedWeekDay = Enum.Parse(typeof(WeekDayAbbreviated), meetingScheduleData[0]);
                    var date = weekStart.AddDays((int) abbreviatedWeekDay);
                    var meetingStart = ToSpecificTime(date, time: meetingScheduleData[1]);
                        

                    var meetingEnd = string.Equals(meetingScheduleData[2], IncorrectDayEndFormat, StringComparison.OrdinalIgnoreCase)
                        ? ToDayStart(date.AddDays(1))
                        : ToSpecificTime(date, meetingScheduleData[2]);

                    return new MeetingInterval(meetingStart, meetingEnd);
                })
                .OrderBy(ms => ms.Start);

            return meetingsInAWeek.ToList();
        }

        private enum WeekDayAbbreviated
        {
            Mon,
            Tue,
            Wed,
            Thu,
            Fri,
            Sat,
            Sun
        }

        private struct MeetingInterval
        {
            public DateTime Start { get; }
            public DateTime End { get; }

            public MeetingInterval(DateTime start, DateTime end)
            {
                Start = start;
                End = end;
            }
        }
    }
}