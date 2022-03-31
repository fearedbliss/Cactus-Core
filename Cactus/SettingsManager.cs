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

using Cactus.Models;
using Cactus.Interfaces;
using System;
using System.Windows;

namespace Cactus
{
    public class SettingsManager : ISettingsManager
    {
        private readonly IJsonManager _jsonManager;

        private SettingsModel _settings;

        public SettingsManager(IJsonManager jsonManager)
        {
            _jsonManager = jsonManager;

            LoadSettings();
        }

        public void SaveSettings(SettingsModel settings)
        {
            _settings = settings;
            _jsonManager.SaveSettings(settings);
        }

        private void LoadSettings()
        {
            _settings = _jsonManager.GetSettings();
        }

        public void LoadTheme()
        {
            string colorMode = ShouldEnableDarkMode ? "Dark" : "Light";
            string themeUriString = $"pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.{colorMode}.xaml";

            ResourceDictionary overallTheme = new ResourceDictionary
            {
                Source = new Uri(themeUriString)
            };

            // TODO: In the future we can implement a more intelligent search function to find
            // the entries we need to remove rather than relying on the index position since this
            // is error prone if someone were to re-order our resource dictionaries. This is safe
            // for now since nobody else is changing this source.

            // TODO: We could implement some UI options to allow users to change the "colorTheme"
            // to a variety of styles. Just sticking with Light/Dark + Teal for now.

            //ResourceDictionary colorTheme = new ResourceDictionary
            //{
            //    Source = new Uri($"pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.Teal.xaml")
            //};

            // Add Overall Theme
            Application.Current.Resources.MergedDictionaries.RemoveAt(2);
            Application.Current.Resources.MergedDictionaries.Insert(2, overallTheme);

            // Add Color Theme
            //Application.Current.Resources.MergedDictionaries.RemoveAt(3);
            //Application.Current.Resources.MergedDictionaries.Insert(3, colorTheme);
        }

        public bool ShouldMinimizeToTray
        {
            get
            {
                return _settings.ShouldMinimizeToTray;
            }
        }

        public bool ShouldEnableDarkMode
        {
            get
            {
                return _settings.ShouldEnableDarkMode;
            }
        }
    }
}
