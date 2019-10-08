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

        private const char DayFromTimeSeparator = ' ';
        private const char MeetingTimeSeparator = '-';

        private int DaysFromMonday = DayOfWeek.Monday - DateTime.UtcNow.DayOfWeek;
        
        public int FindUsingString(string schedule)
        {
            var weekStart = ToDayStart(DateTime.UtcNow.AddDays(DaysFromMonday));
            var weekEnd = ToDayStart(DateTime.UtcNow.AddDays(DaysFromMonday + 7));

            var meetingIntervals = GetOrderedMeetingIntervalsUsingString(weekStart, schedule);

            var longestSleep = default(int);
            var startingPointToCheck = weekStart;

            foreach (var meeting in meetingIntervals)
            {
                var currentSleep = meeting.Start - startingPointToCheck;

                longestSleep = Math.Max(longestSleep, (int) currentSleep.TotalMinutes);

                startingPointToCheck = meeting.End;
            }

            var sleepFromLastMeetingUntilWeekEnd = weekEnd - startingPointToCheck;

            return Math.Max(longestSleep, (int) sleepFromLastMeetingUntilWeekEnd.TotalMinutes);
        }
        
        public int FindUsingSpan(string schedule)
        {
            var weekStart = ToDayStart(DateTime.UtcNow.AddDays(DaysFromMonday));
            var weekEnd = ToDayStart(DateTime.UtcNow.AddDays(DaysFromMonday + 7));

            var meetingIntervals = GetOrderedMeetingIntervalsAllSpan(weekStart, schedule);

            var longestSleep = default(int);
            var startingPointToCheck = weekStart;

            foreach (var meeting in meetingIntervals)
            {
                var currentSleep = meeting.Start - startingPointToCheck;

                longestSleep = Math.Max(longestSleep, (int) currentSleep.TotalMinutes);

                startingPointToCheck = meeting.End;
            }

            var sleepFromLastMeetingUntilWeekEnd = weekEnd - startingPointToCheck;

            return Math.Max(longestSleep, (int) sleepFromLastMeetingUntilWeekEnd.TotalMinutes);
        }

        private IReadOnlyCollection<MeetingInterval> GetOrderedMeetingIntervalsUsingString(DateTime weekStart,
            string schedule)
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
        
        private IReadOnlyCollection<MeetingInterval> GetOrderedMeetingIntervalsAllSpan(DateTime weekStart, string schedule)
        {
            var meetingsInAWeek = new List<MeetingInterval>();
            var incorrectDayEndFormat = (ReadOnlySpan<char>) stackalloc char[] {'2', '4', ':', '0', '0'};

            var scheduleSpan = new ReadOnlySpan<char>(schedule.ToCharArray());

            var startIndex = 0;
            var endIndex = scheduleSpan.IndexOf(Environment.NewLine);

            while (startIndex < endIndex)
            {
                var meetingDataSpan = scheduleSpan.Slice(startIndex, endIndex - startIndex);

                var abbreviatedWeekDaySpan = meetingDataSpan.Slice(0, meetingDataSpan.IndexOf(DayFromTimeSeparator));
                var abbreviatedWeekDay = GetDayNumber(abbreviatedWeekDaySpan);
                var date = weekStart.AddDays(abbreviatedWeekDay);

                var timeStartIndex = meetingDataSpan.IndexOf(DayFromTimeSeparator);
                var meetingTimeSeparatorIndex = meetingDataSpan.IndexOf(MeetingTimeSeparator);
                var startTimeSpan =
                    meetingDataSpan.Slice(timeStartIndex + 1, meetingTimeSeparatorIndex - timeStartIndex - 1);
                var meetingStart = ToSpecificTime(date, time: startTimeSpan.ToString());

                var endTimeSpan = meetingDataSpan.Slice(meetingDataSpan.IndexOf(MeetingTimeSeparator) + 1);
                var meetingEnd = endTimeSpan.Equals(incorrectDayEndFormat, StringComparison.OrdinalIgnoreCase)
                    ? ToDayStart(date.AddDays(1))
                    : ToSpecificTimeUsingSpan(date, endTimeSpan);

                meetingsInAWeek.Add(new MeetingInterval(meetingStart, meetingEnd));

                startIndex = endIndex + Environment.NewLine.Length;
                if (startIndex > scheduleSpan.Length)
                    continue;

                var newLineIndex = scheduleSpan.Slice(startIndex).IndexOf(Environment.NewLine);
                endIndex = newLineIndex > 0
                    ? newLineIndex + startIndex
                    : scheduleSpan.Length;
            }

            return meetingsInAWeek
                .OrderBy(ms => ms.Start)
                .ToList();
        }

        private DateTime ToDayStart(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);
        }

        private DateTime ToSpecificTime(DateTime dateTime, string time)
        {
            return DateTime.ParseExact($"{dateTime.Date.ToString($"{DatePattern} {time}")}",
                $"{DatePattern} {TimePattern}", CultureInfo.InvariantCulture);
        }
        
        private DateTime ToSpecificTimeUsingSpan(DateTime dateTime, ReadOnlySpan<char> time)
        {
            var delimiterIndex = time.IndexOf(':');
            var hours = int.Parse(time.Slice(0, delimiterIndex));
            var minutes = int.Parse(time.Slice(delimiterIndex + 1));
            
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, hours, minutes, 0);
        }

        private int GetDayNumber(ReadOnlySpan<char> dayAbbreviated)
        {
            var mon = (ReadOnlySpan<char>) stackalloc char[] {'M', 'o', 'n'};
            var tue = (ReadOnlySpan<char>) stackalloc char[] {'T', 'u', 'e'};
            var wed = (ReadOnlySpan<char>) stackalloc char[] {'W', 'e', 'd'};
            var thu = (ReadOnlySpan<char>) stackalloc char[] {'T', 'h', 'u'};
            var fri = (ReadOnlySpan<char>) stackalloc char[] {'F', 'r', 'i'};
            var sat = (ReadOnlySpan<char>) stackalloc char[] {'S', 'a', 't'};
            var sun = (ReadOnlySpan<char>) stackalloc char[] {'S', 'u', 'n'};

            if (mon.Equals(dayAbbreviated, StringComparison.OrdinalIgnoreCase))
                return 0;
            
            if (tue.Equals(dayAbbreviated, StringComparison.OrdinalIgnoreCase))
                return 1;
            
            if (wed.Equals(dayAbbreviated, StringComparison.OrdinalIgnoreCase))
                return 2;
            
            if (thu.Equals(dayAbbreviated, StringComparison.OrdinalIgnoreCase))
                return 3;
            
            if (fri.Equals(dayAbbreviated, StringComparison.OrdinalIgnoreCase))
                return 4;
            
            if (sat.Equals(dayAbbreviated, StringComparison.OrdinalIgnoreCase))
                return 5;
            
            if (sun.Equals(dayAbbreviated, StringComparison.OrdinalIgnoreCase))
                return 6;

            throw new NotSupportedException($"{dayAbbreviated.ToString()}");
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