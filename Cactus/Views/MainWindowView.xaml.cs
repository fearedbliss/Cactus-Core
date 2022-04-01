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

namespace Cactus
{
    public partial class MainWindowView : Window
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private WindowState storedWindowState = WindowState.Normal;
        private readonly ISettingsManager _settingsManager;

        public MainWindowView()
        {
            if (!ProcessManager.IsMainApplicationRunning())
            {
                InitializeComponent();

                // Get our settings manager since we will be using it for several stuff.
                var dependencyContainer = Application.Current.Resources["Locator"] as DependencyContainer;
                _settingsManager = dependencyContainer.SettingsManager;

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
                dependencyContainer.MainWindow.SelectLastRanEntry();
            }
            else
            {
                CactusMessageBox.Show("Only one instance of Cactus is allowed!");
                Environment.Exit(1);
            }
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
