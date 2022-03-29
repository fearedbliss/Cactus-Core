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
using Cactus.Models;
using Cactus.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Windows;

namespace Cactus.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private IEntryManager _entryManager;
        private IFileSwitcher _fileSwitcher;

        // Child View Models
        private IAddWindowViewModel _addWindowViewModel;
        private IEditWindowViewModel _editWindowViewModel;

        // Commands
        public RelayCommand AddCommand { get; private set; }
        public RelayCommand EditCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }
        public RelayCommand CopyCommand { get; private set; }
        public RelayCommand UpCommand { get; private set; }
        public RelayCommand DownCommand { get; private set; }
        public RelayCommand ResetCommand { get; private set; }
        public RelayCommand LaunchCommand { get; private set; }

        private readonly string _appName = "Cactus";
        private readonly string _version = "2.0.0";

        public MainWindowViewModel(IEntryManager entryManager, IFileSwitcher fileSwitcher, IAddWindowViewModel addWindowViewModel, IEditWindowViewModel editWindowViewModel)
        {
            _entryManager = entryManager;
            _fileSwitcher = fileSwitcher;
            _addWindowViewModel = addWindowViewModel;
            _editWindowViewModel = editWindowViewModel;

            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit);
            DeleteCommand = new RelayCommand(Delete);
            CopyCommand = new RelayCommand(Copy);
            UpCommand = new RelayCommand(Up);
            DownCommand = new RelayCommand(Down);
            ResetCommand = new RelayCommand(Reset);
            LaunchCommand = new RelayCommand(Launch);

            RefreshEntriesList();
            SelectLastRanEntry();
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

        public void Add()
        {
            var addWindow = new AddView()
            {
                Owner = Application.Current.MainWindow
            };

            addWindow.ShowDialog();

            SelectedEntry = _addWindowViewModel.AddedEntry;

            RefreshEntriesList();
        }

        public void Edit()
        {
            if (SelectedEntry == null)
            {
                MessageBox.Show("No entry to edit was selected.");
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
                MessageBox.Show("No entry to delete was selected.");
                return;
            }

            int deletedIndex = _entryManager.Delete(SelectedEntry);
            _entryManager.SaveEntries();

            RefreshEntriesList();

            if (_entries.Count != 0)
            {
                int targetIndex = 0;

                // If there are still entries but it isn't the ceiling entry, then adjust one up.
                if (deletedIndex != 0)
                {
                    targetIndex = deletedIndex - 1;
                }
                SelectedEntry = _entries[targetIndex];
            }
        }

        public void Copy()
        {
            if (SelectedEntry == null)
            {
                MessageBox.Show("No entry to copy was selected.");
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
                MessageBox.Show("No entry to move up was selected.");
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
                MessageBox.Show("No entry to move down was selected.");
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

        public void Launch()
        {
            if (SelectedEntry == null)
            {
                MessageBox.Show("No entry to launch was selected.");
                return;

            }

            if (string.IsNullOrWhiteSpace(SelectedEntry.Path) || string.IsNullOrWhiteSpace(SelectedEntry.Platform))
            {
                MessageBox.Show("This entry has no platform or path set.");
                return;
            }

            _fileSwitcher.Run(SelectedEntry);
        }

        private void RefreshEntriesList()
        {
            Entries = new ObservableCollection<EntryModel>(_entryManager.GetEntries());
        }

        private void SelectLastRanEntry()
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
    }
}

