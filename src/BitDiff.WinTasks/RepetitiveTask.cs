﻿using System;

namespace Bitdiff.WinTasks
{
    public class RepetitiveTask
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public string Parameters { get; set; }
        public TimeSpan Interval { get; set; }
    }
}