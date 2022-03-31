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

namespace Cactus
{
    public class JsonManager : IJsonManager
    {
        private readonly IFileGenerator _fileGenerator;

        private readonly string _jsonDirectory;
        private readonly string _entriesJsonFile = "Entries.json";
        private readonly string _lastRequiredJsonFile = "LastRequiredFiles.json";
        private readonly string _settingsJsonFile = "Settings.json";

        private string EntriesJsonPath { get; }
        private string LastRequiredJsonPath { get; }
        private string SettingsJsonPath { get; }

        public JsonManager(IFileGenerator fileGenerator)
        {
            _fileGenerator = fileGenerator;

            _jsonDirectory = Directory.GetCurrentDirectory();
            EntriesJsonPath = Path.Combine(_jsonDirectory, _entriesJsonFile);
            LastRequiredJsonPath = Path.Combine(_jsonDirectory, _lastRequiredJsonFile);
            SettingsJsonPath = Path.Combine(_jsonDirectory, _settingsJsonFile);
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

                _fileGenerator.ValidateRequiredFiles(deserializedModel);
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

        private void SaveToJsonFile(string serializedText, string outputFile)
        {
            File.WriteAllText(outputFile, serializedText);
        }
    }
}
