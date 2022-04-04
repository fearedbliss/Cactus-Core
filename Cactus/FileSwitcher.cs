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
using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Security.Principal;
using System.Threading;

namespace Cactus
{
    /// <summary>
    /// This class is responsible for the file switching that occurs when
    /// the user requests to run a version of Diablo II.
    /// </summary>
    public class FileSwitcher : IFileSwitcher
    {
        private readonly IEntryManager _entries;
        private readonly IFileGenerator _fileGenerator;
        private readonly IProcessManager _processManager;
        private readonly IRegistryService _registryService;
        private readonly ILogger _logger;
        private readonly IPathBuilder _pathBuilder;
        private readonly IJsonManager _jsonManager;

        private EntryModel _currentEntry;
        private EntryModel _lastRanEntry;

        public FileSwitcher(IEntryManager entries, IFileGenerator fileGenerator,
                            IProcessManager processManager, IRegistryService registryService,
                            ILogger logger, IPathBuilder pathBuilder, IJsonManager jsonManager)
        {
            _entries = entries;
            _fileGenerator = fileGenerator;
            _processManager = processManager;
            _registryService = registryService;
            _logger = logger;
            _pathBuilder = pathBuilder;
            _jsonManager = jsonManager;
        }

        public void Run(EntryModel entry)
        {
            // Make sure that we have a Diablo II Root Directory set.
            if (!_pathBuilder.IsRootDirectorySet())
            {
                CactusMessageBox.Show("Please set your \"Diablo II Root Directory\" by clicking the \"Settings\" button before launching Diablo II!");
                return;
            }

            // Make sure that the entry has a platform set.
            if (string.IsNullOrWhiteSpace(entry.Platform))
            {
                CactusMessageBox.Show("Please make sure your entry has a Platform set before launching Diablo II!");
                return;
            }

            // Before we do anything, make sure that this platform exists in the first place.
            var proposedPlatformDirectory = _pathBuilder.GetPlatformDirectory(entry);

            if (!Directory.Exists(proposedPlatformDirectory))
            {
                CactusMessageBox.Show("This platform doesn't exist in your Platforms folder.");
                return;
            }

            // Let the games begin (pun intended).
            _currentEntry = entry;
            _lastRanEntry = _entries.GetLastRan();

            if (_lastRanEntry == null)
            {
                _logger.LogInfo("No version was ever ran. Running this and setting it as main version.");

                _lastRanEntry = _currentEntry;
                SwitchFiles();

                _entries.MarkLastRan(_currentEntry);
                _registryService.Update(_currentEntry);
                _entries.SaveEntries();

                LaunchGame();
            }
            else if (_lastRanEntry.Platform.EqualsIgnoreCase(_currentEntry.Platform))
            {
                _logger.LogInfo("Running the same platform.");

                // Update the registry if we are launching the same platform but with a different label.
                if (!_lastRanEntry.Label.EqualsIgnoreCase(_currentEntry.Label))
                {
                    _logger.LogInfo("Same platform but different labels. Updating.");

                    // Do not allow the switch if the user has the game running but the labels are different.
                    if (IsGameRunning())
                    {
                        return;
                    }

                    UpdateEntryAndRegistry();
                }
                else
                {
                    _logger.LogInfo("Same platform and same label.");

                    // We will be setting the _lastRanEntry to the CurrentEntry since
                    // even though the platform and labels are the same, the flags may differ.
                    _entries.SwapLastRan(_lastRanEntry, _currentEntry);
                    _lastRanEntry = _currentEntry;
                    _entries.SaveEntries();
                }

                // The user can launch two different entries that are identical (Except for flags since
                // you may have different flags for the same platform) without switching files completely.
                LaunchGame();
            }
            else
            {
                _logger.LogInfo("A different version has been selected. Switching.");

                // Do not allow the switch if the user has the game running but the platforms are different.
                if (IsGameRunning())
                {
                    return;
                }

                SwitchFiles();
                UpdateEntryAndRegistry();
                LaunchGame();
            }
        }

        private bool IsGameRunning()
        {
            if (_processManager.AreProcessesRunning)
            {
                CactusMessageBox.Show("Diablo II is still running!\n\n" +
                    "Please close the game before attempting to:\n\n" +
                    "- Switch to a different platform.\n" +
                    "- Switch to the same platform but with a different label.\n" +
                    "- Resetting your directory.");

                return true;
            }

            return false;
        }

        /// <summary>
        /// Switches the files in the root directory with the ones needed for this specific entry.
        /// </summary>
        private void SwitchFiles()
        {
            try
            {
                string rootDirectory = _pathBuilder.GetRootDirectory();
                string platformDirectory = _pathBuilder.GetPlatformDirectory(_currentEntry);

                // Retrieve the old required files so we can clean them up when we switch entries.
                var lastRequiredFiles = _jsonManager.GetLastRequiredFiles();

                if (lastRequiredFiles == null)
                {
                    _logger.LogWarning("The last required files file doesn't exist. Using current version as a bases.");
                    lastRequiredFiles = _fileGenerator.GetRequiredFiles(_lastRanEntry);
                }

                var targetVersionRequiredFiles = _fileGenerator.GetRequiredFiles(_currentEntry);

                DeleteRequiredFiles(rootDirectory, lastRequiredFiles);
                InstallRequiredFiles(platformDirectory, rootDirectory, targetVersionRequiredFiles);

                // Save the required files for the target since we will use these to clean up when we switch.
                _jsonManager.SaveLastRequiredFiles(targetVersionRequiredFiles);
            }
            catch (UnauthorizedAccessException ex)
            {
                CactusMessageBox.Show("A file is still being used (You are probably switching entries too fast?). " +
                                "Switch back to the previous version and wait a few seconds after you exit the game " +
                               $"so that Windows stops using the file.\n\nError\n--------\n{ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Restores the hidden Expansion MPQs from previous Cactus behavior to their original location.
        /// </summary>
        /// <remarks>
        /// This function is only here for migration purposes only for existing Cactus users
        /// who have been running Classic versions. Thus it shouldn't be used for anything other
        /// than that purpose since we don't actually need to move any of the MPQs. The classic versions
        /// of the game work perfectly fine with the expansion MPQs in place (Since the old versions didn't
        /// use them). 1.07+ versions of a Classic-only installation also work perfectly fine without them
        /// because the user didn't buy LOD so it isn't expecting them. If a person with a Classic-only
        /// install places the four expansion MPQs in their root directory, the game automatically becomes
        /// an LOD install and everything is unlocked.
        /// </remarks>
        private void RestoreMpqs()
        {
            string rootDirectory = _pathBuilder.GetRootDirectory();
            var expansionMpqs = _fileGenerator.ExpansionMpqs;

            foreach (string mpqFile in expansionMpqs)
            {
                string originalPath = Path.Combine(rootDirectory, mpqFile);
                string hiddenPath = originalPath + ".bak";

                if (File.Exists(hiddenPath) && !File.Exists(originalPath))
                {
                    _logger.LogInfo($"Moving: {hiddenPath} -> {originalPath}");
                    File.Move(hiddenPath, originalPath);
                }
            }
        }

        private void LaunchGame()
        {
            // Make sure save directory exists before we launch the game or else
            // the saves will be in the wrong location. Diablo II automatically
            // creates a 'Save' folder at the D2 root directory if it can't get
            // to the location specified in the 'Save Path'.
            CreateSaveDirectory();

            // [Migration Purposes Only. Read Function Comment/Remarks.]
            RestoreMpqs();

            WindowsIdentity user = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(user);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            string launcherPath = _pathBuilder.GetLauncherPath(_lastRanEntry);
            string launcherFlags = _lastRanEntry.Flags;

            // Make sure that the Launcher exists.
            if (!File.Exists(launcherPath))
            {
                CactusMessageBox.Show($"The launcher doesn't exist!\n\n{launcherPath}");
                return;
            }

            var launchThread = new Thread(() => _processManager.Launch(launcherPath, launcherFlags, isAdmin));
            launchThread.Start();
        }

        /// <summary>
        /// Resets the directory.
        /// </summary>
        public void ResetDirectory()
        {
            _logger.LogWarning("Resetting Directory ...");

            if (IsGameRunning())
            {
                return;
            }

            _lastRanEntry = _entries.GetLastRan();
            if (_lastRanEntry == null)
            {
                _logger.LogWarning("No last ran entry detected. Aborting.");
                return;
            }

            var rootDirectory = _pathBuilder.GetRootDirectory();
            var lastRequiredFiles = _jsonManager.GetLastRequiredFiles();

            if (lastRequiredFiles == null)
            {
                _logger.LogWarning("The last required files file doesn't exist or the collections are empty. Using current version as a bases.");
                lastRequiredFiles = _fileGenerator.GetRequiredFiles(_lastRanEntry);
            }

            // Delete the files in our current "LastRequiredFiles.json" if any.
            if (lastRequiredFiles.Directories.Count != 0 || lastRequiredFiles.Files.Count != 0)
            {
                DeleteRequiredFiles(rootDirectory, lastRequiredFiles);

                // Update our "LastRequiredFiles.json" so that it doesn't have any files.
                _jsonManager.SaveLastRequiredFiles(_fileGenerator.GetEmptyRequiredFiles());
            }

            // [Migration Purposes Only. Read Function Comment/Remarks.]
            RestoreMpqs();

            // Unset the last ran entry.
            _lastRanEntry.WasLastRan = false;
            _entries.SaveEntries();
            _lastRanEntry = null;
        }

        private void InstallRequiredFiles(string platformDirectory, string rootDirectory, RequiredFilesModel requiredFiles)
        {
            // [Safety Check] Validate and strip out anything that is protected.
            _fileGenerator.ValidateRequiredFiles(requiredFiles);

            // In with the new
            foreach (var file in requiredFiles.Files)
            {
                var sourceFile = Path.Combine(platformDirectory, file);
                var targetFile = Path.Combine(rootDirectory, file);
                if (File.Exists(sourceFile))
                {
                    _logger.LogInfo($"Copying: {sourceFile} -> {targetFile}");
                    File.Copy(sourceFile, targetFile, true);
                }
            }

            foreach (var file in requiredFiles.Directories)
            {
                var sourceDirectory = Path.Combine(platformDirectory, file);
                var targetDirectory = Path.Combine(rootDirectory, file);
                if (Directory.Exists(sourceDirectory))
                {
                    _logger.LogInfo($"Copying: {sourceDirectory} -> {targetDirectory}");
                    FileSystem.CopyDirectory(sourceDirectory, targetDirectory, true);
                }
            }
        }

        private void DeleteRequiredFiles(string rootDirectory, RequiredFilesModel requiredFiles)
        {
            // [Safety Check] Before we attempt to delete any files, validate entries since we will
            // not remove anything that is protected. This is more for protection if someone tries to
            // manually insert something into their "LastRequiredFiles.json" that is protected.
            // But I suppose it's nice to have a safety check at this location as well since
            // safety is paramount.
            _fileGenerator.ValidateRequiredFiles(requiredFiles);

            // Out with the old
            foreach (var file in requiredFiles.Files)
            {
                string targetFile = Path.Combine(rootDirectory, file);

                if (File.Exists(targetFile))
                {
                    _logger.LogInfo($"Deleting: {targetFile}");
                    File.Delete(targetFile);
                }
            }

            foreach (var directory in requiredFiles.Directories)
            {
                string targetDirectory = Path.Combine(rootDirectory, directory);

                if (Directory.Exists(targetDirectory))
                {
                    _logger.LogError($"Deleting: {targetDirectory}");
                    Directory.Delete(targetDirectory, true);
                }
            }
        }
        
        private void CreateSaveDirectory()
        {
            string saveDirectory = _pathBuilder.GetSaveDirectory(_lastRanEntry);

            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
        }

        private void UpdateEntryAndRegistry()
        {
            _entries.SwapLastRan(_lastRanEntry, _currentEntry);
            _lastRanEntry = _currentEntry;
            _registryService.Update(_currentEntry);
            _entries.SaveEntries();
        }
    }
}
