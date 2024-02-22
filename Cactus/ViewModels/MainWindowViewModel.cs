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
using Cactus.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Cactus.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel, IDragSource
    {
        private readonly IEntryManager _entryManager;
        private readonly IFileSwitcher _fileSwitcher;
        private readonly IPathBuilder _pathBuilder;
        private readonly IBackupManager _backupManager;

        // Child View Models
        private readonly IAddWindowViewModel _addWindowViewModel;
        private readonly IEditWindowViewModel _editWindowViewModel;

        // Commands
        public RelayCommand AddCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand CopyCommand { get; private set; }
        public RelayCommand UpCommand { get; private set; }
        public RelayCommand DownCommand { get; private set; }
        public RelayCommand ResetCommand { get; private set; }
        public RelayCommand SettingsCommand { get; private set; }
        public RelayCommand LaunchCommand { get; private set; }

        public RelayCommand BackupCommand { get; private set; }

        private readonly string _appName = "Cactus";
        private readonly string _version = "2.6.1";

        public MainWindowViewModel(IEntryManager entryManager, IFileSwitcher fileSwitcher,
            IAddWindowViewModel addWindowViewModel, IEditWindowViewModel editWindowViewModel,
            IPathBuilder pathBuilder, IBackupManager backupManager)
        {
            _entryManager = entryManager;
            _fileSwitcher = fileSwitcher;
            _addWindowViewModel = addWindowViewModel;
            _editWindowViewModel = editWindowViewModel;
            _pathBuilder = pathBuilder;
            _backupManager = backupManager;

            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit);
            DeleteCommand = new RelayCommand(Delete);
            CopyCommand = new RelayCommand(Copy);
            UpCommand = new RelayCommand(Up);
            DownCommand = new RelayCommand(Down);
            ResetCommand = new RelayCommand(Reset);
            SettingsCommand = new RelayCommand(Settings);
            BackupCommand = new RelayCommand(Backup);
            LaunchCommand = new RelayCommand(Launch);

            RefreshEntriesList();
        }

        public string Title
        {
            get
            {
                return $"{_appName} - {_version}";
            }
        }

        private ObservableCollection<EntryModel> _entries;
        public ObservableCollection<EntryModel> Entries
        {
            get
            {
                return _entries;
            }
            set
            {
                _entries = value;
                RaisePropertyChanged("Entries");
            }
        }

        private EntryModel _selectedEntry;
        public EntryModel SelectedEntry
        {
            get
            {
                return _selectedEntry;
            }
            set
            {
                _selectedEntry = value;
                RaisePropertyChanged("SelectedEntry");
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
            }
        }

        public void Add()
        {
            if (!MessageIfRootDirectoryNotSet())
            {
                return;
            }

            var addWindow = new AddView()
            {
                Owner = Application.Current.MainWindow
            };

            addWindow.ShowDialog();

            if (_addWindowViewModel.AddedEntry == null)
            {
                return;
            }

            SelectedEntry = _addWindowViewModel.AddedEntry;

            RefreshEntriesList();
        }

        public void Edit()
        {
            if (!MessageIfRootDirectoryNotSet())
            {
                return;
            }

            if (SelectedEntry == null)
            {
                CactusMessageBox.Show("No entry to edit was selected.");
                return;
            }

            _editWindowViewModel.CurrentEntry = SelectedEntry;
            _editWindowViewModel.LastRanEntry = GetLastRanEntry();

            var editWindow = new EditView()
            {
                Owner = Application.Current.MainWindow
            };

            editWindow.ShowDialog();
        }

        public void Delete()
        {
            if (SelectedEntry == null)
            {
                CactusMessageBox.Show("No entry to delete was selected.");
                return;
            }

            // Storing this so we can reposition the cursor later since once
            // we delete this entry, the SelectedIndex will be -1.
            int storedSelectedIndex = SelectedIndex;

            _entryManager.Delete(SelectedIndex);
            _entryManager.SaveEntries();

            RefreshEntriesList();

            if (_entries.Count != 0)
            {
                int targetIndex = storedSelectedIndex - 1;

                // If we are already at the top but we still have entries,
                // then set the position to the next one on the top.
                if (targetIndex < 0)
                {
                    targetIndex = 0;
                }

                SelectedEntry = _entries[targetIndex];
            }
        }

        public void Copy()
        {
            if (SelectedEntry == null)
            {
                CactusMessageBox.Show("No entry to copy was selected.");
                return;
            }

            var newEntry = _entryManager.Copy(SelectedEntry);
            _entryManager.SaveEntries();

            RefreshEntriesList();
            SelectedEntry = newEntry;
        }

        public void Up()
        {
            if (SelectedEntry == null)
            {
                CactusMessageBox.Show("No entry to move up was selected.");
                return;
            }

            _entryManager.MoveUp(SelectedEntry);
            _entryManager.SaveEntries();

            RefreshEntriesList();
        }

        public void Down()
        {
            if (SelectedEntry == null)
            {
                CactusMessageBox.Show("No entry to move down was selected.");
                return;
            }

            _entryManager.MoveDown(SelectedEntry);
            _entryManager.SaveEntries();

            RefreshEntriesList();
        }

        public void Reset()
        {
            _fileSwitcher.ResetDirectory();

            RefreshEntriesList();
        }

        public void Settings()
        {
            var settingsWindow = new SettingsView()
            {
                Owner = Application.Current.MainWindow
            };

            settingsWindow.ShowDialog();
        }

        public void Backup()
        {
            _backupManager.CreateBackup();
        }

        public void Launch()
        {
            if (SelectedEntry == null)
            {
                CactusMessageBox.Show("No entry to launch was selected.");
                return;

            }

            _fileSwitcher.Run(SelectedEntry);
        }

        private void RefreshEntriesList()
        {
            Entries = new ObservableCollection<EntryModel>(_entryManager.GetEntries());
        }

        public void SelectLastRanEntry()
        {
            var lastRanEntry = GetLastRanEntry();
            if (lastRanEntry != null)
            {
                SelectedEntry = lastRanEntry;
            }
        }

        private EntryModel GetLastRanEntry()
        {
            foreach (var entry in _entries)
            {
                if (entry.WasLastRan)
                {
                    return entry;
                }
            }
            return null;
        }

        /// <summary>
        /// Displays a messge to the user regarding setting the Diablo II Root Directory if needed.
        /// </summary>
        private bool MessageIfRootDirectoryNotSet()
        {
            if (!_pathBuilder.IsRootDirectorySet())
            {
                CactusMessageBox.Show("You must set your \"Diablo II Root Directory\" in \"Settings\" before adding or modifying an entry.");
                return false;
            }
            return true;
        }

        #region Drag & Drop
        public void StartDrag(IDragInfo dragInfo)
        {
            EntryModel entry = dragInfo.SourceItem as EntryModel;

            if (entry != null)
            {
                // Only allow dragging if we have more than one element.
                if (_entries.Count > 1)
                {
                    dragInfo.Effects = DragDropEffects.Move;
                    dragInfo.Data = entry;
                }
            }
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            return true;
        }

        public void Dropped(IDropInfo dropInfo)
        {
            EntryModel entry = dropInfo.Data as EntryModel;

            int sourceIndex = dropInfo.DragInfo.SourceIndex;
            int targetIndex = dropInfo.InsertIndex;

            _entryManager.Move(sourceIndex, targetIndex, entry);
        }

        public void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo)
        {
            // Save entries and refresh our UI.
            _entryManager.SaveEntries();
            RefreshEntriesList();

            // Select our dragged entry at its new location.
            SelectedEntry = dragInfo.SourceItem as EntryModel;
        }

        public void DragCancelled()
        {
            // Not Used. Nothing special.
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            CactusMessageBox.Show("An exception has occurred. Please report this to upstream. Closing Cactus.\n\n" +
                exception.Message);
            Environment.Exit(1);

            // An exception while re-ordering shouldn't happen :P.
            return true;
        }
        #endregion
    }
}
