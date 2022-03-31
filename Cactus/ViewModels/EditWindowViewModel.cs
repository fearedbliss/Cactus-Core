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
using GalaSoft.MvvmLight.Command;
using System.IO;

namespace Cactus.ViewModels
{
    public class EditWindowViewModel : ViewModelBase, IEditWindowViewModel
    {
        private readonly IEntryManager _entryManager;
        private readonly IRegistryService _registryService;
        private readonly IPathBuilder _pathBuilder;
        private readonly IProcessManager _processManager;
        private readonly ILogger _logger;

        public EntryModel CurrentEntry { get; set; }
        public EntryModel LastRanEntry { get; set; }

        // Keep the old entry since we need it to restore all of the UI if the user cancels.
        private EntryModel _oldEntry;

        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public EditWindowViewModel(IEntryManager entryManager, IRegistryService registryService,
                                   IPathBuilder pathBuilder, IProcessManager processManager, ILogger logger)
        {
            _entryManager = entryManager;
            _registryService = registryService;
            _pathBuilder = pathBuilder;
            _processManager = processManager;
            _logger = logger;

            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Ok()
        {
            if (_entryManager.IsInvalid(CurrentEntry))
            {
                CactusMessageBox.Show("Unable to change your Entry! Please make sure all fields are:\n\n" +
                    "- Populated (Label/Flags are optional)\n" +
                    "- Path should match the rest of your Entries (.exe can vary)\n" +
                    "- No invalid characters\n\n" +
                    "If you have moved Cactus to a new machine, please manually edit the Entries.json and adjust all your paths accordingly.");
                ReverseChanges();
                return;
            }

            // If the oldEntry's platform is null, that means that the user
            // made a copy of an entry and now is trying to rename it.
            if (!string.IsNullOrWhiteSpace(_oldEntry.Platform))
            {
                // If we are switching the last ran state from on to off,
                // then we will not rename the directory, since if we do this,
                // we may effectively be renaming the storage folder which should
                // remain isolated.
                if (_oldEntry.WasLastRan == CurrentEntry.WasLastRan)
                {
                    var oldPlatformDirectory = _pathBuilder.GetPlatformDirectory(_oldEntry);
                    var newPlatformDirectory = _pathBuilder.GetPlatformDirectory(CurrentEntry);

                    var oldSavesDirectory = _pathBuilder.GetSaveDirectory(_oldEntry);
                    var newSavesDirectory = _pathBuilder.GetSaveDirectory(CurrentEntry);

                    // We can skip renaming if it's the same platform and saves directory (label change).
                    // No renaming of directories needs to happen here. Just saving.
                    if (!oldPlatformDirectory.EqualsIgnoreCase(newPlatformDirectory) ||
                        !oldSavesDirectory.EqualsIgnoreCase(newSavesDirectory))
                    {
                        // If the target platform directory name already exists, we can't allow this rename to happen.
                        if (!oldPlatformDirectory.EqualsIgnoreCase(newPlatformDirectory) && Directory.Exists(newPlatformDirectory))
                        {
                            CactusMessageBox.Show($"A platform directory with the name \"{CurrentEntry.Platform}\" already exists.");
                            ReverseChanges();
                            return;
                        }

                        // We won't allow editing the Label field if it was empty because that would
                        // mean that the user's save files would need to be moved to the target location.
                        // This could be problematic if someone has save files with the same name in different
                        // label directories (or if there was an existing file already in the flat save directory).
                        // Thus, this is completely disabled at the UI level for the Edit Window. We won't check for
                        // this (null case) here since it would prevent someone that didn't give an entry a label in
                        // the first place, the ability to edit that entry. However, we can allow Label to Label renames.

                        // Prevent renaming a label from something to nothing.
                        if (!string.IsNullOrWhiteSpace(_oldEntry.Label) && string.IsNullOrWhiteSpace(CurrentEntry.Label))
                        {
                            CactusMessageBox.Show("A label cannot be removed once created. It can only be modified.");
                            ReverseChanges();
                            return;
                        }

                        // If the target save directory name already exists, we can't allow this rename to happen.
                        if (!oldSavesDirectory.EqualsIgnoreCase(newSavesDirectory) && Directory.Exists(newSavesDirectory))
                        {
                            CactusMessageBox.Show($"A save directory with the same name exists at: \"{newSavesDirectory}\"");
                            ReverseChanges();
                            return;
                        }

                        // If this entry is currently running, then we can't complete this
                        // operation since the game is still using that directory/save path.
                        if (CurrentEntry.WasLastRan && _processManager.AreProcessesRunning)
                        {
                            _logger.LogWarning("You can't edit this entry since the game is currently running and using its save directory.");
                            _logger.LogWarning("Please close all instances of Diablo II and try again.");

                            ReverseChanges();
                            return;
                        }

                        // No need to rename if the Platform directories are the same.
                        if (!oldPlatformDirectory.EqualsIgnoreCase(newPlatformDirectory))
                        {
                            if (Directory.Exists(oldPlatformDirectory))
                            {
                                Directory.Move(oldPlatformDirectory, newPlatformDirectory);
                            }

                            // We need to rename the Saves directory root as well (with platform name but without label).
                            var oldSavesDirectoryWithoutLabel = _pathBuilder.GetSaveDirectory(_oldEntry, true);
                            var newSavesDirectoryWithoutLabel = _pathBuilder.GetSaveDirectory(CurrentEntry, true);

                            if (Directory.Exists(oldSavesDirectoryWithoutLabel))
                            {
                                Directory.Move(oldSavesDirectoryWithoutLabel, newSavesDirectoryWithoutLabel);
                            }

                            // Rename any identically named platforms
                            _entryManager.RenamePlatform(_oldEntry.Platform, CurrentEntry.Platform);
                        }

                        // No need to rename if the Saves directories are the same.
                        else if (!oldSavesDirectory.EqualsIgnoreCase(newSavesDirectory))
                        {
                            if (Directory.Exists(oldSavesDirectory))
                            {
                                Directory.Move(oldSavesDirectory, newSavesDirectory);
                            }

                            // Rename any identically named platforms with the same label name
                            _entryManager.RenameLabel(CurrentEntry.Platform, _oldEntry.Label, CurrentEntry.Label);
                        }
                    }
                }

                // If this entry was switched from not being the last ran to being the last ran,
                // Then we need to disable whichever entry was last ran if there was one.
                if (!_oldEntry.WasLastRan && CurrentEntry.WasLastRan && LastRanEntry != null)
                {
                    LastRanEntry.WasLastRan = false;
                }

                // If this was the last ran entry, then we need to update the registry.
                if (CurrentEntry.WasLastRan)
                {
                    _registryService.Update(CurrentEntry);
                }
                else if (_entryManager.DoesPlatformNameMatchLastRan(CurrentEntry.Platform))
                {
                    // If it wasn't the last ran entry but it's sharing the same platform,
                    // then we need to update the registry for the last ran entry since the
                    // shared platform name has changed and that affects the save location.
                    _registryService.Update(LastRanEntry);
                }
            }

            _entryManager.SaveEntries();

            // Clear old entry so next updates work properly.
            _oldEntry = null;
        }

        private void Cancel()
        {
            ReverseChanges();
        }

        private void ReverseChanges()
        {
            CurrentEntry.Platform = _oldEntry.Platform;
            CurrentEntry.Label = _oldEntry.Label;
            CurrentEntry.Path = _oldEntry.Path;
            CurrentEntry.Flags = _oldEntry.Flags;
            CurrentEntry.WasLastRan = _oldEntry.WasLastRan;

            _oldEntry = null;
        }

        public string Platform
        {
            get
            {
                // Kinda dirty but we are using the platform as the way to backup the entire object.
                if (_oldEntry == null)
                {
                    _oldEntry = new EntryModel
                    {
                        Platform = CurrentEntry.Platform,
                        Label = CurrentEntry.Label,
                        Path = CurrentEntry.Path,
                        Flags = CurrentEntry.Flags,
                        WasLastRan = CurrentEntry.WasLastRan
                    };
                }
                return CurrentEntry.Platform;
            }
            set
            {
                CurrentEntry.Platform = value;
            }
        }
    }
}
