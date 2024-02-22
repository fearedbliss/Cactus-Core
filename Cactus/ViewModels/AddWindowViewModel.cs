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
                if (!_fileSwitcher.IfPlatformDirectoryMissingThenAlert(entry))
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
