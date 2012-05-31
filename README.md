Overview
========

wintasks is a simple wrapper for managing tasks in the Windows Task Scheduler. 

This is a thin wrapper around the excellent [Microsoft.Win32.TaskScheduler](http://taskscheduler.codeplex.com/) library optimized for the most common use-case of creating a task that repeats on an interval.

It includes a utility for managing the re-deployment of tasks by waiting for a task to complete before disabling - thereby allowing a deployment script to ensure that the binaries are not locked.

Usage
=====

The main entry point is the TaskHelper class which contains two main functions - Disable and Install.

Typically, in a deployment script, you would want to disable your tasks, copy your binaries, and re-enable the tasks.

An example of two task definitions is as follows:

```c#
var tasks
    = new List<RepetitiveTask>
          {
              new RepetitiveTask
                  {
                      Name = "My first task",
                      Description = "Does something on a regular interval.",
                      Parameters = @"--myparam=true",
                      RunsEveryHowManyMinutes = 15
                  },
              new RepetitiveTask
                  {
                      Name = "My other task",
                      Description = "Also does something on a regular interval.",
                      Path = @"c:\\bar\\baz.exe",
                      Parameters = "--myotherparam=false",
                      RunsEveryHowManyMinutes = 1
                  }
          };
```

To install these tasks in the Windows Task Scheduler, you would use the TaskHelper as follows:
```c#
var helper = new TaskHelper();
helper.Install(tasks);
```
To disable these tasks, you could do as follows:
```c#
var helper = new TaskHelper();
helper.Disable(tasks);
```

Typically, in an install script, you would want to first disable the tasks, then copy the binaries, and then run the install command. It is safe to run the disable even if the tasks have never been installed; it will not raise an exception if a disabled task does not exist.

RepetitiveTask Properties
=========================
- Name (required): The name of the task as it will appear in the Windows Task Scheduler. This is effectively the key so you should first delete or disable the old task in the Windows Task Scheduler before changing the name.
- Description (optional): The description as it appears in the Windows Task Scheduler interface.
- Path (optional): The path to the executable that should be run. If omitted, it assumes that the calling assembly of the task installation process itself is the desired executable.
- Parameters (optional): The command line parameters required by the scheduled executable.
- RunsEveryHowManyMinutes (required): The minimum interval between executions of the task. If the interval elapses and the task has not finished executing, the following interval will be skipped.


