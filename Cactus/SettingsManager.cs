// Copyright © 2018-2023 Jonathan Vasquez <jon@xyinn.org>
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

using Cactus.Models;
using Cactus.Interfaces;
using System;
using System.Windows;

namespace Cactus
{
    public class SettingsManager : ISettingsManager
    {
        private readonly IJsonManager _jsonManager;

        // We will store the old settings model so that we can know what things have changed (i.e Themes).
        private SettingsModel _oldSettings;
        private SettingsModel _settings;

        public SettingsManager(IJsonManager jsonManager)
        {
            _jsonManager = jsonManager;

            LoadSettings();
        }

        public void SaveSettings()
        {
            SaveSettings(_settings);
        }

        public void SaveSettings(SettingsModel settings)
        {
            _oldSettings = _settings;
            _settings = settings;
            _jsonManager.SaveSettings(settings);
        }

        private void LoadSettings()
        {
            _settings = _jsonManager.GetSettings();

            // Set the oldSettings on load so we have a reliable base (Let's say we open
            // the Settings but don't change anything), code paths checking oldSettings
            // will still work correctly.
            _oldSettings = _settings;
        }

        public void LoadTheme(bool isStartUp = false)
        {
            int themeIndex;
            int colorIndex;

            if (isStartUp)
            {
                // The default color combination for Cactus is Light + Teal. This allows us
                // to continue to see pretty designs in our designer. If these change, this
                // code would need to get updated. We will pretty much always be removing
                // these at start up and re-inserting them if the user never changed their
                // them preferences.
                string defaultTheme = "Light";
                string defaultColor = "Teal";
                string defaultThemeUriString = GetThemePath(defaultTheme);
                string defaultColorUriString = GetColorPath(defaultColor);

                themeIndex = ResourceUtility.RemoveResource(defaultThemeUriString);
                ResourceUtility.AddResource(themeIndex, GetPreferredThemeResource());

                colorIndex = ResourceUtility.RemoveResource(defaultColorUriString);
                ResourceUtility.AddResource(colorIndex, GetPreferredColorResource());
            }
            else
            {
                string oldTheme = GetOldThemeName();

                // Only perform this if there is a theme change.
                if (!oldTheme.EqualsIgnoreCase(GetPreferredThemeName()))
                {
                    // Replace old theme and install new one.
                    string oldThemeUriString = GetThemePath(oldTheme);
                    themeIndex = ResourceUtility.RemoveResource(oldThemeUriString);
                    ResourceUtility.AddResource(themeIndex, GetPreferredThemeResource());
                }

                string oldColor = GetOldColor();
                if (!oldColor.EqualsIgnoreCase(GetPreferredColor()))
                {
                    // Replace old color and install new one.
                    string oldColorUriString = GetColorPath(oldColor);
                    colorIndex = ResourceUtility.RemoveResource(oldColorUriString);
                    ResourceUtility.AddResource(colorIndex, GetPreferredColorResource());
                }
            }
        }

        private ResourceDictionary GetPreferredThemeResource()
        {
            string preferredTheme = GetPreferredThemeName();
            string preferredThemeUriString = GetThemePath(preferredTheme);
            return new ResourceDictionary
            {
                Source = new Uri(preferredThemeUriString)
            };
        }

        private ResourceDictionary GetPreferredColorResource()
        {
            string preferredColorUriString = GetColorPath(GetPreferredColor());
            return new ResourceDictionary
            {
                Source = new Uri(preferredColorUriString)
            };
        }

        private string GetOldColor()
        {
            return StripAllSpaces(_oldSettings.PreferredColor);
        }

        private string GetPreferredColor()
        {
            return StripAllSpaces(PreferredColor);
        }

        private string StripAllSpaces(string value)
        {
            return value.Replace(" ", "");
        }

        private string GetThemePath(string color)
        {
            return $"pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.{color}.xaml";
        }

        private string GetColorPath(string color)
        {
            return $"pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.{color}.xaml";
        }

        private string GetPreferredThemeName()
        {
            return GetThemeModeFromValue(ShouldEnableDarkMode);
        }

        private string GetOldThemeName()
        {
            return GetThemeModeFromValue(_oldSettings.ShouldEnableDarkMode);
        }

        private string GetThemeModeFromValue(bool shouldEnableDarkMode)
        {
            return shouldEnableDarkMode ? "Dark" : "Light";
        }

        public void SetRootDirectory(string rootDirectory)
        {
            _settings.RootDirectory = rootDirectory;
        }

        /// <summary>
        /// Marks that the user has migrated to the new format.
        /// </summary>
        public void MarkHasMigratedToNewFormat()
        {
            _settings.HasMigratedToNewFormat = true;
        }

        public string RootDirectory => _settings.RootDirectory;
        public string BackupsDirectory => _settings.BackupsDirectory;
        public bool ShouldMinimizeToTray => _settings.ShouldMinimizeToTray;
        public bool ShouldEnableDarkMode => _settings.ShouldEnableDarkMode;
        public string PreferredColor => _settings.PreferredColor;
        public bool HasMigratedToNewFormat => _settings.HasMigratedToNewFormat;
    }
}
