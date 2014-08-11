using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Win32.TaskScheduler;

namespace Bitdiff.WinTasks
{
    public class TaskHelper : ITaskHelper
    {
        public event EventHandler<TaskEventArgs> TaskInstalledEvent;
        public event EventHandler<TaskEventArgs> WaitingForTaskToCompleteEvent;
        public event EventHandler<TaskEventArgs> TaskDisabledEvent;
        public event EventHandler<TaskEventArgs> TaskDeletedEvent;

        public void Install(IEnumerable<RepetitiveTask> tasks)
        {
            tasks = tasks.ToList();

            using (var ts = new TaskService())
            {
                var path = Assembly.GetEntryAssembly().Location;

                foreach (var t in tasks)
                {
                    var definition = ts.NewTask();
                    definition.Settings.Enabled = true;
                    definition.RegistrationInfo.Description = t.Description;

                    AddTrigger(definition, t);

                    definition.Actions.Add(new ExecAction(t.Path ?? path, t.Parameters, Path.GetDirectoryName(path)));

                    ts.RootFolder.RegisterTaskDefinition(
                        t.Name,
                        definition,
                        TaskCreation.CreateOrUpdate,
                        "SYSTEM",
                        null,
                        TaskLogonType.ServiceAccount);

                    OnTaskInstalledEvent(new TaskEventArgs { Task = t });
                }
            }
        }

        public void Disable(IEnumerable<RepetitiveTask> tasks)
        {
            using (var ts = new TaskService())
            {
                foreach (var t in tasks)
                {
                    var task = ts.GetTask(t.Name);
                    if (task != null)
                    {
                        WaitForTaskToComplete(task, t);

                        task.Enabled = false;
                        task.RegisterChanges();

                        OnTaskDisabledEvent(new TaskEventArgs { Task = t });
                    }
                }
            }
        }

        public void Delete(IEnumerable<RepetitiveTask> tasks)
        {
            using (var ts = new TaskService())
            {
                foreach (var t in tasks)
                {
                    var task = ts.GetTask(t.Name);
                    if (task != null)
                    {
                        WaitForTaskToComplete(task, t);

                        ts.RootFolder.DeleteTask(t.Name);

                        OnTaskDeletedEvent(new TaskEventArgs { Task = t });
                    }
                }
            }
        }

        private void WaitForTaskToComplete(Task task, RepetitiveTask t)
        {
            while (task.State == TaskState.Running)
            {
                OnWaitingForTaskToCompleteEvent(new TaskEventArgs { Task = t });
                Thread.Sleep(500);
            }
        }

        private void AddTrigger(TaskDefinition definition, RepetitiveTask task)
        {
            switch (task.Type)
            {
                case RepetitiveTaskType.Daily:
                    {
                        var trigger = (DailyTrigger)definition.Triggers.Add(new DailyTrigger());
                        trigger.StartBoundary = task.StartAt.HasValue ? task.StartAt.Value : DateTime.Now;
                    }
                    break;
                case RepetitiveTaskType.Weekly:
                    {
                        var trigger = (WeeklyTrigger)definition.Triggers.Add(new WeeklyTrigger(task.DaysOfWeek));
                        trigger.StartBoundary = task.StartAt.HasValue ? task.StartAt.Value : DateTime.Now;
                    }
                    break;

                case RepetitiveTaskType.Monthly:
                    {
                        var trigger = (MonthlyTrigger)definition.Triggers.Add(new MonthlyTrigger(task.DayOfMonth));
                        trigger.StartBoundary = task.StartAt.HasValue ? task.StartAt.Value : DateTime.Now;
                    }
                    break;
                case RepetitiveTaskType.Interval:
                    {
                        var trigger = (TimeTrigger)definition.Triggers.Add(new TimeTrigger());
                        trigger.StartBoundary = task.StartAt.HasValue ? task.StartAt.Value : DateTime.Now;
                        trigger.Repetition.Interval = task.Interval;
                    }
                    break;
                default:
                    throw new NotSupportedException("The task type is not supported.");
            }

            definition.Settings.StopIfGoingOnBatteries = false;
            definition.Settings.DisallowStartIfOnBatteries = false;
        }

        private void OnTaskInstalledEvent(TaskEventArgs e)
        {
            var handler = TaskInstalledEvent;
            if (handler != null) handler(this, e);
        }

        private void OnTaskDisabledEvent(TaskEventArgs e)
        {
            var handler = TaskDisabledEvent;
            if (handler != null) handler(this, e);
        }

        private void OnTaskDeletedEvent(TaskEventArgs e)
        {
            var handler = TaskDeletedEvent;
            if (handler != null) handler(this, e);
        }

        private void OnWaitingForTaskToCompleteEvent(TaskEventArgs e)
        {
            var handler = WaitingForTaskToCompleteEvent;
            if (handler != null) handler(this, e);
        }
    }
}