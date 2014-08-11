using System;
using Microsoft.Win32.TaskScheduler;

namespace Bitdiff.WinTasks
{
    public class RepetitiveTask
    {
        public RepetitiveTaskType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string Parameters { get; set; }
        public TimeSpan Interval { get; set; }
        public DateTime? StartAt { get; set; }
        public DaysOfTheWeek DaysOfWeek { get; set; }
        public int DayOfMonth { get; set; }
    }
}