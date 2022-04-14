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

using Cactus.Models;
using Cactus.Interfaces;
using System.IO;

namespace Cactus
{
    public class PathBuilder : IPathBuilder
    {
        private readonly ISettingsManager _settingsManager;
        private readonly string _platformDirectoryName = "Platforms";
        private readonly string _savesDirectoryName = "Saves";

        public PathBuilder(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public string GetRootDirectory()
        {
            return _settingsManager.RootDirectory;
        }

        public bool IsRootDirectorySet()
        {
            return !string.IsNullOrWhiteSpace(GetRootDirectory());
        }

        public string GetPlatformDirectory(EntryModel entry)
        {
            return Path.Combine(GetPlatformsDirectory(), entry.Platform);
        }

        /// <summary>
        /// Gets the Launcher Path.
        ///
        /// Example: C:\Games\Diablo II\Game.exe
        /// </summary>
        public string GetLauncherPath(EntryModel entry)
        {
            return Path.Combine(GetRootDirectory(), entry.Launcher);
        }

        /// <summary>
        /// Gets the Save Root Directory for the particular entry.
        /// </summary>
        public string GetSaveDirectory(EntryModel entry, bool excludeLabel = false)
        {
            string savesDirectory = GetSavesDirectory();
            string saveDirectory = Path.Combine(savesDirectory, entry.Platform);

            if (!excludeLabel && !string.IsNullOrWhiteSpace(entry.Label))
            {
                saveDirectory = Path.Combine(saveDirectory, entry.Label);
            }

            return saveDirectory;
        }

        /// <summary>
        /// Gets the path to where all of our Platforms are stored.
        /// </summary>
        public string GetPlatformsDirectory()
        {
            return Path.Combine(GetRootDirectory(), _platformDirectoryName);
        }

        /// <summary>
        /// Gets the path to where all of our Saves are stored.
        /// </summary>
        public string GetSavesDirectory()
        {
            return Path.Combine(GetRootDirectory(), _savesDirectoryName);
        }
    }
}
