using System.Collections.Generic;
using Xunit;

namespace LongestSleepFinder.Tests
{
    public class LongestSleepFinderTests
    {
        [Theory]
        [MemberData(nameof(GetLongestSleepInScheduleTestData))]
        public void ShouldFindCorrectLongestSleepInAWeek(string schedule, int expectedSleep)
        {
            var result = FindLongestSleepInSchedule(schedule);
            
            Assert.Equal(expectedSleep, result);
        }
        
        public static IEnumerable<object[]> GetLongestSleepInScheduleTestData()
        {
            yield return new object[]
            {
                @"Mon 01:00-23:00
Tue 01:00-23:00
Wed 01:00-23:00
Thu 01:00-23:00
Fri 01:00-23:00
Sat 01:00-23:00
Sun 01:00-21:00", 180
            };
            yield return new object[]
            {
                @"Sun 10:00-20:00
Fri 05:00-10:00
Fri 16:30-23:50
Sat 10:00-24:00
Sun 01:00-04:00
Sat 02:00-06:00
Tue 03:30-18:15
Tue 19:00-20:00
Wed 04:25-15:14
Wed 15:14-22:40
Thu 00:00-23:59
Mon 05:00-13:00
Mon 15:00-21:00", 505
            };
        }
    }
}