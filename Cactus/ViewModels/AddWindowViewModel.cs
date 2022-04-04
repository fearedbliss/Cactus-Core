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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Cactus.ViewModels
{
    public class AddWindowViewModel : ViewModelBase, IAddWindowViewModel
    {
        private readonly IEntryManager _entryManager;
        private readonly IFileSwitcher _fileSwitcher;

        // Properties for new entry
        public string Platform { get; set; }
        public string Label { get; set; }
        public string Launcher { get; set; }
        public string Flags { get; set; }

        // Allow parent view model to retrieve this property.
        public EntryModel AddedEntry { get; set; }

        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public AddWindowViewModel(IEntryManager entryManager, IFileSwitcher fileSwitcher)
        {
            _entryManager = entryManager;
            _fileSwitcher = fileSwitcher;

            OkCommand = new RelayCommand(Ok);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Ok()
        {
            var entry = new EntryModel
            {
                Platform = Platform,
                Label = Label,
                Launcher = Launcher,
                Flags = Flags,
            };

            if (_entryManager.IsInvalid(entry))
            {
                CactusMessageBox.Show("Please make sure the required fields are populated and contain no invalid characters.");
            }
            else
            {
                if (!_fileSwitcher.IsPlatformDirectoryMissingThenAlert(entry))
                {
                    _entryManager.Add(entry);
                    _entryManager.SaveEntries();

                    AddedEntry = entry;
                }
            }

            ResetUI();
        }

        private void Cancel()
        {
            ResetUI();
        }

        private void ResetUI()
        {
            Platform = null;
            Label = null;
            Launcher = null;
            Flags = null;
        }
    }
}
