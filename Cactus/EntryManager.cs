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
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;

namespace Cactus
{
    /// <summary>
    /// This class is responsible for managing the entries.
    /// 
    /// - Contains a collection of Entries
    /// - Adding, Editing, and Deletion of Entries
    /// - Saving to Json File
    /// </summary>
    public class EntryManager : ViewModelBase, IEntryManager
    {
        private List<EntryModel> _entries;
        private ILogger _logger;
        private IPathBuilder _pathBuilder;
        private IJsonManager _jsonManager;

        public EntryManager(ILogger logger, IPathBuilder pathBuilder, IJsonManager jsonManager)
        {
            _logger = logger;
            _pathBuilder = pathBuilder;
            _jsonManager = jsonManager;
            _entries = GetEntries();
        }

        public EntryModel GetLastRan()
        {
            foreach(var entry in _entries)
            {
                if (entry.WasLastRan) return entry;
            }
            return null;
        }

        /// <summary>
        /// Lets you know if the entry in question is equal to all other root directories.
        /// </summary>
        public bool IsRootDirectoryEqualToOthers(EntryModel entry)
        {
            foreach (var currentEntry in _entries)
            {
                var proposedEntryRoot = _pathBuilder.GetRootDirectory(entry);
                var currentEntryRoot = _pathBuilder.GetRootDirectory(currentEntry);
                if (!currentEntryRoot.EqualsIgnoreCase(proposedEntryRoot))
                {
                    return false;
                }
            }

            return true;
        }

        public void Add(EntryModel entry)
        {
            _entries.Add(entry);
        }

        public int Delete(EntryModel entry)
        {
            int removedIndex = -1;
            EntryModel elementToRemove = null;

            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];

                if (e.Platform.EqualsIgnoreCase(entry.Platform) && e.Path.EqualsIgnoreCase(entry.Path) &&
                    e.Flags.EqualsIgnoreCase(entry.Flags) && e.IsExpansion == entry.IsExpansion)
                {
                    elementToRemove = e;
                    removedIndex = i;
                    break;
                }
            }

            if (elementToRemove != null)
            {
                _entries.Remove(elementToRemove);
            }

            return removedIndex;
        }

        public EntryModel Copy(EntryModel entry)
        {
            var newEntry = new EntryModel
            {
                Label = entry.Label,
                Path = entry.Path,
                Flags = entry.Flags,
                IsExpansion = entry.IsExpansion
            };

            _entries.Add(newEntry);
            return newEntry;
        }

        public void MoveUp(EntryModel entry)
        {
            int selectedEntryIndex = _entries.FindIndex(_ => _ == entry);

            // If we are already at the top, then nothing needs to be done.
            if (selectedEntryIndex != 0)
            {
                // Find previous entry and swap
                int previousEntryIndex = selectedEntryIndex - 1;

                var previousEntry = _entries[previousEntryIndex];
                _entries[previousEntryIndex] = entry;
                _entries[selectedEntryIndex] = previousEntry;
            }
        }

        public void MoveDown(EntryModel entry)
        {
            int selectedEntryIndex = _entries.FindIndex(_ => _ == entry);
            int lastIndex = _entries.Count - 1;

            // If we are already at the bottom, then nothing needs to be done.
            if (selectedEntryIndex != lastIndex)
            {
                // Find next entry and swap
                int nextEntryIndex = selectedEntryIndex + 1;

                var nextEntry = _entries[nextEntryIndex];
                _entries[nextEntryIndex] = entry;
                _entries[selectedEntryIndex] = nextEntry;
            }
        }

        /// <summary>
        /// This marks this specific entry as the last ran. Useful in situations where
        /// the user never ran a version before through this application.
        /// </summary>
        public void MarkLastRan(EntryModel entry)
        {
            entry.WasLastRan = true;
        }

        /// <summary>
        /// Switches the two entries as the last ran. Useful when switching versions.
        /// </summary>
        public void SwapLastRan(EntryModel oldEntry, EntryModel newEntry)
        {
            oldEntry.WasLastRan = false;
            newEntry.WasLastRan = true;
        }

        public List<EntryModel> GetEntries()
        {
            try
            {
                if (_entries != null) return _entries;
                _entries = _jsonManager.GetEntries();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogWarning("Wiping existing json file since it's corrupted.");
                SaveEntries();
            }
            
            return _entries;
        }

        public void SaveEntries()
        {
            _jsonManager.SaveEntries(_entries);
        }

        /// <summary>
        /// Renames all platform references with a particular name, to a new name.
        /// </summary>
        public void RenamePlatform(string oldPlatformName, string newPlatformName)
        {
            foreach (var entry in _entries)
            {
                if (entry.Platform.EqualsIgnoreCase(oldPlatformName))
                {
                    entry.Platform = newPlatformName;
                }
            }
        }

        /// <summary>
        /// Renames all label references with a particular name, to a new name for a specific platform.
        /// </summary>
        public void RenameLabel(string platformName, string oldLabelName, string newLabelName)
        {
            foreach (var entry in _entries)
            {
                if (!entry.Platform.EqualsIgnoreCase(platformName)) continue;

                if (entry.Label.EqualsIgnoreCase(oldLabelName))
                {
                    entry.Label = newLabelName;
                }
            }
        }

        /// <summary>
        /// Checks to see if the platform name given matches the platform name of the last ran platform.
        /// </summary>
        public bool DoesPlatformNameMatchLastRan(string platformName)
        {
            var lastRanEntry = GetLastRan();

            if (lastRanEntry == null) return false;

            return lastRanEntry.Platform.EqualsIgnoreCase(platformName);
        }

        /// <summary>
        /// Validates the given information to see if it's correct.
        /// </summary>
        public bool IsInvalid(EntryModel entry)
        {
            return string.IsNullOrWhiteSpace(entry.Platform) ||
                   string.IsNullOrWhiteSpace(entry.Path) ||
                   !IsRootDirectoryEqualToOthers(entry) ||
                   _pathBuilder.ContainsInvalidCharacters(entry.Platform) ||
                   _pathBuilder.ContainsInvalidCharacters(entry.Label);
        }
    }
}
