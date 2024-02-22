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
        private readonly IProcessManager _processManager;

        public string RootDirectory { get; set; }
        public string BackupsDirectory { get; set; }
        public bool ShouldMinimizeToTray { get; set; }
        public bool ShouldEnableDarkMode { get; set; }
        public bool HasMigratedToNewFormat { get; set; }

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

        public SettingsWindowViewModel(ISettingsManager settingsManager, IProcessManager processManager)
        {
            _settingsManager = settingsManager;
            _processManager = processManager;

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);

            LoadSettings();
        }

        private void Save()
        {
            var settings = new SettingsModel
            {
                RootDirectory = RootDirectory,
                BackupsDirectory = BackupsDirectory,
                ShouldMinimizeToTray = ShouldMinimizeToTray,
                ShouldEnableDarkMode = ShouldEnableDarkMode,
                PreferredColor = PreferredColor,
                HasMigratedToNewFormat = HasMigratedToNewFormat,
            };

            // Prevent the root directory from being modified while the game is running.
            if (!_settingsManager.RootDirectory.EqualsIgnoreCase(settings.RootDirectory) && _processManager.IsGameRunning())
            {
                LoadSettings();
                return;
            }

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
            BackupsDirectory = _settingsManager.BackupsDirectory;
            ShouldMinimizeToTray = _settingsManager.ShouldMinimizeToTray;
            ShouldEnableDarkMode = _settingsManager.ShouldEnableDarkMode;
            PreferredColor = _settingsManager.PreferredColor;
            HasMigratedToNewFormat = _settingsManager.HasMigratedToNewFormat;
        }

        private void ProcessTriggers()
        {
            _settingsManager.LoadTheme();
        }
    }
}
