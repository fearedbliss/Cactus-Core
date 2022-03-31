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

        private enum Mode
        {
            Classic,
            Expansion
        }

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
            else if (_lastRanEntry.Platform.EqualsIgnoreCase(_currentEntry.Platform) &&
                     _lastRanEntry.Path.EqualsIgnoreCase(_currentEntry.Path) &&
                     _lastRanEntry.IsExpansion == _currentEntry.IsExpansion)
            {
                _logger.LogInfo("Running the same platform.");

                // Update the registry if we are launching the same platform but with a different label.
                if (!_lastRanEntry.Label.EqualsIgnoreCase(_currentEntry.Label))
                {
                    _logger.LogInfo("Same platform but different labels. Updating.");
                    UpdateEntryAndRegistry();
                }
                else
                {
                    _logger.LogInfo("Same platform and same label.");

                    // We will be setting the _lastRanEntry to the CurrentEntry since
                    // even though the platforms are the same, the flags may differ.
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

                // If there is an existing process running, do not allow a switch.
                // Only identical versions can be launched.
                if (_processManager.AreProcessesRunning)
                {
                    CactusMessageBox.Show("Diablo II is still running. Please close the game before attempting to switch to a different Platform.");
                    return;
                }

                SwitchFiles();
                UpdateEntryAndRegistry();
                LaunchGame();
            }
        }

        /// <summary>
        /// Switches the files in the root directory with the ones needed for this specific entry.
        /// </summary>
        private void SwitchFiles()
        {
            try
            {
                string rootDirectory = _pathBuilder.GetRootDirectory(_lastRanEntry);
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

                // Move the MPQ files away if it's Classic, but make sure they are in the root if it's expansion.
                if (_lastRanEntry.IsExpansion)
                {
                    // If our last entry is Expansion and current entry is Classic, then move files.
                    if (!_currentEntry.IsExpansion)
                    {
                        SwitchMpqs(Mode.Classic);
                    }
                }
                else
                {
                    // If our last entry is Classic and current entry is Expansion, then move files.
                    if (_currentEntry.IsExpansion)
                    {
                        SwitchMpqs(Mode.Expansion);
                    }
                }

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
        /// Switches the Expansion MPQs in order to Enable/Disable Expansion or Classic modes.
        /// </summary>
        /// <param name="mode">Mode you want to switch to</param>
        private void SwitchMpqs(Mode mode)
        {
            string rootDirectory = _pathBuilder.GetRootDirectory(_lastRanEntry);
            var expansionMpqs = _fileGenerator.ExpansionMpqs;

            foreach (var mpqFile in expansionMpqs)
            {
                string basePath = Path.Combine(rootDirectory, mpqFile);
                string backupMpqPath = basePath + ".bak";

                string sourceMpqPath = mode == Mode.Classic ? basePath : backupMpqPath;
                string targetMpqPath = mode == Mode.Classic ? backupMpqPath : basePath;

                _logger.LogInfo($"Moving: {sourceMpqPath} -> {targetMpqPath}");
                File.Move(sourceMpqPath, targetMpqPath);
            }
        }

        private void LaunchGame()
        {
            // Make sure save directory exists before we launch the game or else
            // the saves will be in the wrong location. Diablo II automatically
            // creates a 'Save' folder at the D2 root directory if it can't get
            // to the location specified in the 'Save Path'.
            CreateSaveDirectoryIfNeeded(_lastRanEntry);

            WindowsIdentity user = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(user);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            var launchThread = new Thread(() => _processManager.Launch(_lastRanEntry, isAdmin));
            launchThread.Start();
        }

        private void InstallRequiredFiles(string platformDirectory, string rootDirectory, RequiredFilesModel requiredFiles)
        {
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

        /// <summary>
        /// Resets the directory.
        /// </summary>
        public void ResetDirectory()
        {
            _logger.LogWarning("Resetting Directory ...");

            if (_processManager.AreProcessesRunning)
            {
                CactusMessageBox.Show("Diablo II is currently running. Please close the game before attempting to reset your directory.");
                return;
            }

            _lastRanEntry = _entries.GetLastRan();
            if (_lastRanEntry == null)
            {
                _logger.LogWarning("No last ran entry detected. Aborting.");
                return;
            }

            var rootDirectory = _pathBuilder.GetRootDirectory(_lastRanEntry);
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

            // Unset the last ran entry.
            _lastRanEntry.WasLastRan = false;
            _entries.SaveEntries();
            _lastRanEntry = null;
        }

        private void DeleteRequiredFiles(string rootDirectory, RequiredFilesModel requiredFiles)
        {
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
        
        private void CreateSaveDirectoryIfNeeded(EntryModel entry)
        {
            string saveDirectory = _pathBuilder.GetSaveDirectory(entry);

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
