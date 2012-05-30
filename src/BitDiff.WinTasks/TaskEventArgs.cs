using System;

namespace Bitdiff.WinTasks
{
    public class TaskEventArgs : EventArgs
    {
        public RepetitiveTask Task { get; set; }
    }
}