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
            BackupCactusFile(EntriesJsonPath);
            BackupCactusFile(LastRequiredJsonPath);
            BackupCactusFile(SettingsJsonPath);
        }

        public void DeleteCactusBackupFiles()
        {
            DeleteCactusFile(EntriesJsonPath + _backupExtension);
            DeleteCactusFile(LastRequiredJsonPath + _backupExtension);
            DeleteCactusFile(SettingsJsonPath + _backupExtension);
        }

        public void BackupAndDeleteCactusFiles()
        {
            BackupAndDeleteCactusFile(EntriesJsonPath);
            BackupAndDeleteCactusFile(LastRequiredJsonPath);
            BackupAndDeleteCactusFile(SettingsJsonPath);
        }

        private void BackupAndDeleteCactusFile(string path)
        {
            BackupCactusFile(path);
            DeleteCactusFile(path);
        }
    }
}
