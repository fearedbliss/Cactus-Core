// Copyright © 2018-2024 Jonathan Vasquez <jon@xyinn.org>
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
//
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
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using Castle.Core.Internal;

namespace Cactus
{
    /// <summary>
    /// This class is responsible for managing the creation of our backups.
    /// </summary>
    public class BackupManager: IBackupManager
    {
        private readonly IProcessManager _processManager;
        private readonly IPathBuilder _pathBuilder;
        private readonly IJsonManager _jsonManager;
        private readonly IFileSwitcher _fileSwitcher;
        private readonly string _rootDirectory;

        public BackupManager(IProcessManager processManager, IPathBuilder pathBuilder, IJsonManager jsonManager, IFileSwitcher fileSwitcher)
        {
            _processManager = processManager;
            _pathBuilder = pathBuilder;
            _jsonManager = jsonManager;
            _fileSwitcher = fileSwitcher;
            _rootDirectory = _pathBuilder.GetRootDirectory();
        }

        public void CreateBackup()
        {
            var platformDirectory = _pathBuilder.GetPlatformsDirectory();
            var savesDirectory = _pathBuilder.GetSavesDirectory();
            var jsonFiles = _jsonManager.ManagedFiles;

            if (!DoRequiredDocumentsExist(platformDirectory, savesDirectory, jsonFiles))
            {
                return;
            }

            // Make sure that the base backup directory exists.
            CreateBackupsDirectory();

            // Do not allow the backup to be created if the game is running.
            if (_processManager.IsGameRunning())
            {
                return;
            }

            // Generate a name and create a directory for this specific backup.
            string backupName = GenerateBackupName();
            string backupDirectoryName = CreateBackupDirectory(backupName);
            if (backupDirectoryName.IsNullOrEmpty())
            {
                return;
            }

            // Copy the relevant documents.
            GenerateBackup(backupDirectoryName, platformDirectory, savesDirectory, jsonFiles);

            CactusMessageBox.Show("Backup was successfully created at: \n\n" + backupDirectoryName);
        }

        private void CreateBackupsDirectory()
        {
            string backupDirectory = _pathBuilder.GetBackupsDirectory();
            _fileSwitcher.CreateDirectory(backupDirectory);
        }

        private string CreateBackupDirectory(string directory)
        {
            string backupDirectory = _pathBuilder.GetBackupDirectory(directory);

            // Do not create the backup if we detect that it already exists.
            if (Directory.Exists(directory))
            {
                CactusMessageBox.Show("A backup directory with the name: " + backupDirectory + " already exists. Aborting backup.");
                return null;
            }

            _fileSwitcher.CreateDirectory(backupDirectory);
            return backupDirectory;
        }

        /// <summary>
        /// Verifies that all required documents exist before creating a backup.
        /// </summary>
        private bool DoRequiredDocumentsExist(string platformDirectory, string savesDirectory, List<string> jsonFiles)
        {
            if (!Directory.Exists(platformDirectory))
            {
                CactusMessageBox.Show("Your Platforms directory doesn't exist. Aborting backup.");
                return false;
            }

            if (!Directory.Exists(savesDirectory))
            {
                CactusMessageBox.Show("Your Saves directory doesn't exist. Aborting backup.");
                return false;
            }

            foreach (var file in jsonFiles)
            {
                var targetFile = Path.Combine(_rootDirectory, file);
                if (!File.Exists(targetFile))
                {
                    CactusMessageBox.Show("The following required file is missing: " + targetFile + ". Aborting backup.");
                    return false;
                }
            }

            return true;
        }
    
       private void GenerateBackup(string backupDirectoryName, string platformDirectory, string savesDirectory, List<string> jsonFiles)
       {
            // Copy Platforms and Saves Directories.
            var backupPlatformDirectory = Path.Combine(backupDirectoryName, _pathBuilder.PlatformsDirectoryName);
            var backupSavesDirectory = Path.Combine(backupDirectoryName, _pathBuilder.SavesDirectoryName);

            FileSystem.CopyDirectory(platformDirectory, backupPlatformDirectory);
            FileSystem.CopyDirectory(savesDirectory, backupSavesDirectory);

            // Copy the json files.
            foreach(var file in jsonFiles)
            {
                var sourceFile = Path.Combine(_rootDirectory, file);
                var targetFile = Path.Combine(backupDirectoryName, file);
                File.Copy(sourceFile, targetFile);
            }
        }

        /// <summary>
        /// Generates a timestamp to be used for the backup directory in the following format: YYYY-MM-DD-HHMM-SS.
        /// </summary>
        private string GenerateBackupName()
        {
            return DateTime.Now.ToString("yyyy-MM-dd-HHmm-ss", CultureInfo.InvariantCulture);
        }
    }
}
