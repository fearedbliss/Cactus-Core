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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;

namespace Cactus
{
    public class JsonManager : IJsonManager
    {
        private readonly string _jsonDirectory;
        private readonly string _entriesJsonFile = "Entries.json";
        private readonly string _lastRequiredJsonFile = "LastRequiredFiles.json";
        private readonly string _settingsJsonFile = "Settings.json";
        private readonly string _backupExtension = ".bak";

        private string EntriesJsonPath { get; }
        private string LastRequiredJsonPath { get; }
        private string SettingsJsonPath { get; }

        public JsonManager()
        {
            _jsonDirectory = Directory.GetCurrentDirectory();
            EntriesJsonPath = Path.Combine(_jsonDirectory, _entriesJsonFile);
            LastRequiredJsonPath = Path.Combine(_jsonDirectory, _lastRequiredJsonFile);
            SettingsJsonPath = Path.Combine(_jsonDirectory, _settingsJsonFile);
        }

        /// <summary>
        /// Gets the list of files managed by this JsonManager.
        /// </summary>
        public List<string> ManagedFiles
        {
            get
            {
                return new List<string>()
                {
                    _entriesJsonFile,
                    _lastRequiredJsonFile,
                    _settingsJsonFile
                };
            }
        }

        /// <summary>
        /// Gets the list of files (with Path) managed by this JsonManager.
        /// </summary>
        public List<string> ManagedFilesPath
        {
            get
            {
                return new List<string>()
                {
                    EntriesJsonPath,
                    LastRequiredJsonPath,
                    SettingsJsonPath
                };
            }
        }

        public void ValidateCactusFiles()
        {
            // Kinda nasty right (index)? Haha.
            int index = 0;

            try
            {
                GetEntries(); index = 1;
                GetLastRequiredFiles(); index = 2;
                GetSettings(); index = 3;
            }
            catch (Exception ex)
            {
                string failedFile;

                switch (index)
                {
                    case 0:
                        failedFile = _entriesJsonFile;
                        break;
                    case 1:
                        failedFile = _lastRequiredJsonFile;
                        break;
                    case 2:
                        failedFile = _settingsJsonFile;
                        break;
                    default:
                        failedFile = "Unknown";
                        break;
                }

                CactusMessageBox.Show("At least one of your Cactus files failed to load. Moving your old files out of the way (.bak). " +
                    "Please restart Cactus for a fresh install.\n\n" +
                    $"Error Message ({failedFile})\n\n" + ex.Message);
                BackupAndDeleteCactusFiles();
                Environment.Exit(1);
            }
        }

        public void SaveEntries(List<EntryModel> entries)
        {
            string serializedEntries = JsonConvert.SerializeObject(entries, Formatting.Indented);
            File.WriteAllText(EntriesJsonPath, serializedEntries);
        }

        public List<EntryModel> GetEntries()
        {
            if (File.Exists(EntriesJsonPath))
            {
                var serializedEntries = File.ReadAllText(EntriesJsonPath);
                return JsonConvert.DeserializeObject<List<EntryModel>>(serializedEntries);
            }
            return new List<EntryModel>();
        }

        public void SaveLastRequiredFiles(RequiredFilesModel requiredFiles)
        {
            string serializedFiles = JsonConvert.SerializeObject(requiredFiles, Formatting.Indented);
            SaveToJsonFile(serializedFiles, LastRequiredJsonPath);
        }

        public RequiredFilesModel GetLastRequiredFiles()
        {
            if (File.Exists(LastRequiredJsonPath))
            {
                var serializedFiles = File.ReadAllText(LastRequiredJsonPath);
                var deserializedModel = JsonConvert.DeserializeObject<RequiredFilesModel>(serializedFiles);
                return deserializedModel;
            }
            return null;
        }

        public void SaveSettings(SettingsModel settings)
        {
            string serializedSettings = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsJsonPath, serializedSettings);
        }

        public SettingsModel GetSettings()
        {
            if (File.Exists(SettingsJsonPath))
            {
                var serializedSettings = File.ReadAllText(SettingsJsonPath);
                return JsonConvert.DeserializeObject<SettingsModel>(serializedSettings);
            }
            return new SettingsModel();
        }

        public bool DoesSettingsFileExist()
        {
            return File.Exists(SettingsJsonPath);
        }

        public bool DoesEntriesFileExist()
        {
            return File.Exists(EntriesJsonPath);
        }

        private void SaveToJsonFile(string serializedText, string outputFile)
        {
            File.WriteAllText(outputFile, serializedText);
        }

        private void BackupCactusFile(string path)
        {
            if (File.Exists(path))
            {
                string targetPath = path + _backupExtension;
                File.Copy(path, targetPath, true);
            }
        }

        private void DeleteCactusFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public void BackupCactusFiles()
        {
            foreach (var file in ManagedFilesPath)
            {
                BackupCactusFile(file);
            }
        }

        public void DeleteCactusBackupFiles()
        {
            foreach (var file in ManagedFilesPath)
            {
                string targetFile = file + _backupExtension;
                DeleteCactusFile(targetFile);
            }
        }

        public void BackupAndDeleteCactusFiles()
        {
            foreach (var file in ManagedFilesPath)
            {
                BackupAndDeleteCactusFile(file);
            }
        }

        private void BackupAndDeleteCactusFile(string path)
        {
            BackupCactusFile(path);
            DeleteCactusFile(path);
        }
    }
}
