using System.Collections.Generic;

namespace Bitdiff.WinTasks
{
    public interface ITaskHelper
    {
        void Disable(IEnumerable<RepetitiveTask> tasks);
        void Install(IEnumerable<RepetitiveTask> tasks);
    }
}