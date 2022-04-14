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
        private readonly IFileSwitcher _fileSwitcher;

        public EntryModel CurrentEntry { get; set; }
        public EntryModel LastRanEntry { get; set; }

        // Keep the old entry since we need it to restore all of the UI if the user cancels.
        private EntryModel _oldEntry;

        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public EditWindowViewModel(IEntryManager entryManager, IRegistryService registryService,
                                   IPathBuilder pathBuilder, IProcessManager processManager, IFileSwitcher fileSwitcher)
        {
            _entryManager = entryManager;
            _registryService = registryService;
            _pathBuilder = pathBuilder;
            _processManager = processManager;
            _fileSwitcher = fileSwitcher;

            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Ok()
        {
            if (_entryManager.IsInvalid(CurrentEntry))
            {
                CactusMessageBox.Show("Please make sure the required fields are populated and contain no invalid characters.\n\n" +
                    "If you have moved Cactus to a new machine, please edit your \"Diablo II Root Directory\" by clicking the \"Settings\" button.");
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

                        // If the label is changing from something A to something B, but another entry already
                        // exists for that platform + label combination, then we need to prevent this change.
                        if (!_oldEntry.Label.EqualsIgnoreCase(CurrentEntry.Label))
                        {
                            // If this platform/label combination already exists, we can't allow this rename to happen.
                            if (_entryManager.DoesPlatformAndLabelExist(CurrentEntry, true))
                            {
                                CactusMessageBox.Show("A platform and label already exists with combination:\n\n" +
                                    $"{CurrentEntry.Platform}\n" +
                                    $"{CurrentEntry.Label}");
                                ReverseChanges();
                                return;
                            }
                        }

                        // If the target save directory name already exists, we can't allow this rename to happen.
                        if (!oldSavesDirectory.EqualsIgnoreCase(newSavesDirectory) && Directory.Exists(newSavesDirectory))
                        {
                            CactusMessageBox.Show($"A save directory with the same name exists at: \n\n{newSavesDirectory}");
                            ReverseChanges();
                            return;
                        }

                        // If there are processes running, we need to be careful with what we allow to be edited at this time.
                        if (_processManager.AreProcessesRunning)
                        {
                            bool stopEdit = false;

                            // If the current entry was the last ran, then that means we are currently running this one
                            // and cannot allow an edit.
                            if (CurrentEntry.WasLastRan)
                            {
                                stopEdit = true;
                            }

                            // If the entry we are editing is related to the same platform that is running...
                            else if (_oldEntry.Platform.EqualsIgnoreCase(LastRanEntry.Platform))
                            {
                                // If we are attempting to rename the platform name, we can't allow this edit.
                                if (!CurrentEntry.Platform.EqualsIgnoreCase(LastRanEntry.Platform))
                                {
                                    stopEdit = true;
                                }

                                // If we are attempting to edit the label of a platform that is identical to the
                                // one that is currently running, we can't allow this edit.
                                else if (_oldEntry.Label.EqualsIgnoreCase(LastRanEntry.Label))
                                {
                                    stopEdit = true;
                                }
                            }

                            if (stopEdit)
                            {
                                CactusMessageBox.Show($"You can't edit this entry since the game is currently running " +
                                    "and the running instance is related to this platform and/or label. " +
                                    "Please close all instances of Diablo II and try again.");
                                ReverseChanges();
                                return;
                            }
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
            else
            {
                // Make sure that the Platforms directory exists before we continue.
                if (_fileSwitcher.IsPlatformDirectoryMissingThenAlert(CurrentEntry))
                {
                    ReverseChanges();
                    return;
                }

                // If we are editing an entry that was copied (null), and we detect that the label
                // was changed from something to nothing, then we need to set the label to null
                // rather than letting it be an empty string.
                if (!string.IsNullOrWhiteSpace(_oldEntry.Label) && string.IsNullOrWhiteSpace(CurrentEntry.Label))
                {
                    CurrentEntry.Label = null;
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
            CurrentEntry.Launcher = _oldEntry.Launcher;
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
                        Launcher = CurrentEntry.Launcher,
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
