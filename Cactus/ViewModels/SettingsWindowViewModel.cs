// Copyright (C) 2018-2022 Jonathan Vasquez <jon@xyinn.org>
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
using System.Collections.Generic;
using System.IO;

namespace Cactus.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase, ISettingsWindowViewModel
    {
        private readonly ISettingsManager _settingsManager;

        public string RootDirectory { get; set; }
        public bool ShouldMinimizeToTray { get; set; }
        public bool ShouldEnableDarkMode { get; set; }

        public List<string> Colors => new List<string>
        {
            "Amber",
            "Blue",
            "Blue Grey",
            "Brown",
            "Cyan",
            "Deep Orange",
            "Deep Purple",
            "Green",
            "Grey",
            "Indigo",
            "Light Blue",
            "Light Green",
            "Lime",
            "Orange",
            "Pink",
            "Purple",
            "Red",
            "Teal",
            "Yellow",
        };

        private string _preferredColor;
        public string PreferredColor
        {
            get
            {
                return _preferredColor;
            }
            set
            {
                _preferredColor = value;
                RaisePropertyChanged("PreferredColor");
            }
        }


        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public SettingsWindowViewModel(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);

            LoadSettings();
        }

        private void Save()
        {
            var settings = new SettingsModel
            {
                RootDirectory = RootDirectory,
                ShouldMinimizeToTray = ShouldMinimizeToTray,
                ShouldEnableDarkMode = ShouldEnableDarkMode,
                PreferredColor = PreferredColor,
            };

            // Verify that our Diablo II Root Directory exists.
            if (!string.IsNullOrWhiteSpace(settings.RootDirectory) && !Directory.Exists(settings.RootDirectory))
            {
                CactusMessageBox.Show("The \"Diablo II Root Directory\" specified does not exist. Make sure it exists before saving your settings.");
                LoadSettings();
                return;
            }

            _settingsManager.SaveSettings(settings);

            ProcessTriggers();
        }

        private void Cancel()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            RootDirectory = _settingsManager.RootDirectory;
            ShouldMinimizeToTray = _settingsManager.ShouldMinimizeToTray;
            ShouldEnableDarkMode = _settingsManager.ShouldEnableDarkMode;
            PreferredColor = _settingsManager.PreferredColor;
        }

        private void ProcessTriggers()
        {
            _settingsManager.LoadTheme();
        }
    }
}
