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
using System;
using System.ComponentModel;
using System.Windows;
using System.IO;

namespace Cactus
{
    public partial class MainWindowView : Window
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private WindowState storedWindowState = WindowState.Normal;

        private readonly DependencyContainer _dependencyContainer;
        private readonly ISettingsManager _settingsManager;
        private readonly IJsonManager _jsonManager;
        private readonly IEntryManager _entryManager;

        public MainWindowView()
        {
            if (!ProcessManager.IsMainApplicationRunning())
            {
                // Get dependency container so we can use some of our Managers.
                _dependencyContainer = Application.Current.Resources["Locator"] as DependencyContainer;

                // Get some dependencies since we will be using it for several stuff.
                _jsonManager = _dependencyContainer.JsonManager;

                // Ensure that all of our Cactus files can load if they exist before any component initialization.
                _jsonManager.ValidateCactusFiles();

                InitializeComponent();

                _entryManager = _dependencyContainer.EntryManager;
                _settingsManager = _dependencyContainer.SettingsManager;

                // Load our theme based on settings.
                _settingsManager.LoadTheme(true);

                // Credits to the below for the original System Tray code:
                // https://possemeeg.wordpress.com/2007/09/06/minimize-to-tray-icon-in-wpf/
                notifyIcon = new System.Windows.Forms.NotifyIcon
                {
                    Text = "Cactus",
                    BalloonTipTitle = "Cactus",
                    BalloonTipText = "Cactus has been minimized to your System Tray.",
                    Icon = System.Drawing.Icon.ExtractAssociatedIcon(AppDomain.CurrentDomain.FriendlyName)
                };
                notifyIcon.Click += new EventHandler(OnNotifyIcon_Click);

                // Now that the UI is fully initialized, select the last ran entry if any.
                _dependencyContainer.MainWindow.SelectLastRanEntry();
            }
            else
            {
                CactusMessageBox.Show("Only one instance of Cactus is allowed!");
                Environment.Exit(1);
            }
        }

        private void OnMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // If Settings.json doesn't exist, this user is either coming from Cactus 1.2.4 or earlier,
            // or is coming from Cactus 2.0.X where we didn't yet have a Settings.json.
            if (!_jsonManager.DoesSettingsFileExist())
            {
                // If the Entries file exists, that means the user is coming from Cactus 2.0.X or earlier.
                if (_jsonManager.DoesEntriesFileExist())
                {
                    MigratePath();
                }

                // This is a brand new user to Cactus. Let's run out OOTB experience.
                else
                {
                    CactusMessageBox.Show("Welcome to Cactus!\n\n" +
                        "A Modern Diablo II Version Switcher & Character Isolator\n\n" +
                        "Please set your \"Diablo II Root Directory\" by clicking the \"Settings\" button before launching Diablo II! " +
                        "You can then click the \"Add\" button to add an entry.\n\n" +
                        "Please also make sure that you are running Cactus within your Diablo II Root Directory.\n\n" +
                        "Example: C:\\Games\\Diablo II\\Cactus.exe\n\n" +
                        "https://github.com/fearedbliss/Cactus"
                    );

                    // Save a clean copy of our settings in order to "mark" this as
                    // proof that we have displayed this message. We are also marking
                    // them as having already migrated to the new format ;D.
                    _settingsManager.MarkHasMigratedToNewFormat();
                    _settingsManager.SaveSettings();
                }
            }
            else
            {
                MigratePath();
            }

            FixWhitespaceLabels();
        }

        /// <summary>
        /// Migrates Pre Cactus 2.3.0 - "Path" Variable
        /// Breaking Path To => RootDirectory + Launcher
        /// </summary>
        private void MigratePath()
        {
            if (_settingsManager.HasMigratedToNewFormat)
            {
                return;
            }

            // We need to migrate the user if necessary.
            var pathBuilder = _dependencyContainer.PathBuilder;

            if (!pathBuilder.IsRootDirectorySet())
            {
                var entries = _entryManager.GetEntries();

                // No need to migrate someone that doesn't have any entries.
                if (entries.Count == 0)
                {
                    return;
                }

                CactusMessageBox.Show("Welcome to Cactus!\n\n" +
                    "A Modern Diablo II Version Switcher & Character Isolator\n\n" +
                    "Your Cactus files will now be migrated to the new\n\"Diablo II Root Directory\" + \"Launcher\" format."
                );

                // Before we begin, let's make backups just in case.
                _jsonManager.BackupCactusFiles();

                try
                {
                    string rootDirectory = Path.GetDirectoryName(entries[0].Path);

                    foreach (var entry in entries)
                    {
                        entry.Launcher = Path.GetFileName(entry.Path);
                    }

                    // Save Root Directory
                    _settingsManager.SetRootDirectory(rootDirectory);
                    _settingsManager.MarkHasMigratedToNewFormat();
                    _settingsManager.SaveSettings();

                    // Save all entries in their new format with their corresponding Launcher.
                    _entryManager.SaveEntries();

                    // Delete the old backup files.
                    _jsonManager.DeleteCactusBackupFiles();

                    CactusMessageBox.Show("Migration Successful!\nPath => (Root Directory + Launcher)\n\n" +
                        "Please verify the information for the following directories is correct. If anything is incorrect, edit your \"Diablo II Root Directory\" by clicking the \"Settings\" button. " +
                        "Please also verify that the \"Launcher\" for each of your entries is correct. You can view it by editing the entry.\n\n" +
                        $"Diablo II Root Directory\n\n{pathBuilder.GetRootDirectory()}\n\n" +
                        $"Diablo II Platforms Directory\n\n{pathBuilder.GetPlatformsDirectory()}\n\n" +
                        $"Diablo II Saves Directory\n\n{pathBuilder.GetSavesDirectory()}"
                    );
                }
                catch (Exception)
                {
                    CactusMessageBox.Show("Migration Failed!\nPath => (Root Directory + Launcher)\n\n" +
                        "Your previous Cactus files have been backed up with a \".bak\" extension. Please inspect and rename them once they are fixed."
                    );
                }
            }
            else
            {
                // These are users that already were on 2.3.0 (Thus they had a Diablo II Directory Set w/
                // Settings file containing that setting). Let's just mark them as already having migrated
                // so everything is good to go.
                _settingsManager.MarkHasMigratedToNewFormat();
                _settingsManager.SaveSettings();
            }
        }

        /// <summary>
        /// Fixes any labels that are set to "" to null.
        /// </summary>
        /// <remarks>
        /// This is a safety check so that we can guarantee that a person
        /// doesn't bypass the TargetNullValue in the Edit Window. There
        /// was a bug in 2.2.1 that allowed this to happen. We are adopting
        /// this in all cases as part of our Integrity System.
        /// </remarks>
        private void FixWhitespaceLabels()
        {
            var entries = _entryManager.GetEntries();

            // No need to fix the labels if they don't have any entries.
            if (entries.Count == 0)
            {
                return;
            }

            foreach (var entry in entries)
            {
                if (entry.Label == "")
                {
                    entry.Label = null;
                }
            }

            // Resave the entries with the fixed null label.
            _entryManager.SaveEntries();
        }

        private void EntriesListView_DoubleClick(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as IMainWindowViewModel;
            viewModel.Launch();
        }

        #region System Tray Code
        private void OnClose(object sender, CancelEventArgs args)
        {
            // This null will be true if the user attempt to open a second instance of Cactus
            // (on shutdown of that second instance).
            if (notifyIcon == null)
            {
                return;
            }

            notifyIcon.Dispose();
            notifyIcon = null;
        }

        private void OnStateChanged(object sender, EventArgs args)
        {
            if (WindowState == WindowState.Minimized)
            {
                if (!_settingsManager.ShouldMinimizeToTray)
                {
                    return;
                }

                Hide();

                if (notifyIcon == null)
                {
                    storedWindowState = WindowState;
                    return;
                }

                // Timeout is deprecated (not used) and it's now based on system accessibility settings.
                // https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.notifyicon.showballoontip?view=windowsdesktop-6.0
                notifyIcon.ShowBalloonTip(2000);
            }
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (notifyIcon == null)
            {
                return;
            }

            notifyIcon.Visible = !IsVisible;
        }


        private void OnNotifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = storedWindowState;
        }
        #endregion

        private void EntriesListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ScrollIntoView();
        }

        private void OnEntries_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollIntoView();
        }

        private void ScrollIntoView()
        {
            if (EntriesListView.SelectedItem == null)
            {
                return;
            }

            EntriesListView.ScrollIntoView(EntriesListView.SelectedItem);
        }
    }
}
