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

namespace Cactus.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase, ISettingsWindowViewModel
    {
        private readonly ISettingsManager _settingsManager;

        public bool ShouldMinimizeToTray { get; set; }
        public bool ShouldEnableDarkMode { get; set; }

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
                ShouldMinimizeToTray = ShouldMinimizeToTray,
                ShouldEnableDarkMode = ShouldEnableDarkMode,
            };

            _settingsManager.SaveSettings(settings);

            ProcessTriggers();
        }

        private void Cancel()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            ShouldMinimizeToTray = _settingsManager.ShouldMinimizeToTray;
            ShouldEnableDarkMode = _settingsManager.ShouldEnableDarkMode;
        }

        private void ProcessTriggers()
        {
            _settingsManager.LoadTheme();
        }
    }
}
