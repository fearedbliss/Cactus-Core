// Copyright (C) 2018-2019 Jonathan Vasquez <jon@xyinn.org>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Cactus.Interfaces;
using Cactus.Models;
using System;
using System.Diagnostics;

namespace Cactus
{
    public class ProcessManager : IProcessManager
    {
        private int _processCount;

        public bool AreProcessesRunning
        {
            get
            {
                return _processCount > 0;
            }
        }

        /// <summary>
        /// Checks to see if Cactus is already running.
        /// </summary>
        /// <remarks>
        /// This is a very very simple implementation of making sure only one process
        /// of Cactus runs. If the user is running an application that is also called
        /// "Cactus", this would trigger a false positive.
        /// </remarks>
        public static bool IsMainApplicationRunning()
        {
            var currentProcess = Process.GetCurrentProcess();
            var currentProcesses = Process.GetProcessesByName(currentProcess.ProcessName);

            return currentProcesses.Length > 1;
        }

        public void Launch(EntryModel entry, bool isAdmin)
        {
            try
            {
                _processCount++;

                var processInfo = new ProcessStartInfo
                {
                    FileName = entry.Path,
                    Arguments = entry.Flags
                };

                if (isAdmin)
                {
                    processInfo.Verb = "runas";
                }

                var process = Process.Start(processInfo);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                CactusMessageBox.Show($"There was an error launching the application.\n\n{ex.Message}\n\nLaunch Path: {entry.Path}");
            }

            _processCount--;
        }
    }
}
