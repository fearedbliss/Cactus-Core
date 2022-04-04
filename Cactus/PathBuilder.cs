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
