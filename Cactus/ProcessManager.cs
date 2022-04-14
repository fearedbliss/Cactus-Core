// Copyright © 2018-2022 Jonathan Vasquez <jon@xyinn.org>
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE AUTHOR AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
// OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
// OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
// SUCH DAMAGE.

using Cactus.Interfaces;
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

        public void Launch(string launcherPath, string launcherFlags, bool isAdmin)
        {
            try
            {
                _processCount++;

                var processInfo = new ProcessStartInfo
                {
                    FileName = launcherPath,
                    Arguments = launcherFlags
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
                CactusMessageBox.Show($"There was an error launching the application.\n\n{ex.Message}\n\nLaunch Path: {launcherPath}");
            }

            _processCount--;
        }
    }
}
