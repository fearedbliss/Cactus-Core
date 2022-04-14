// Copyright © 2018-2022 Jonathan Vasquez <jon@xyinn.org>
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
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

using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace Cactus.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EntryModel : ViewModelBase
    {
        private string _platform;
        private string _label;
        private string _launcher;
        private string _flags;
        private bool _wasLastRan;

        [JsonProperty("Platform", Order = 1)]
        public string Platform
        {
            get
            {
                return _platform;
            }
            set
            {
                _platform = value;
                RaisePropertyChanged("Platform");
            }
        }

        [JsonProperty("Label", Order = 2)]
        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                _label = value;
                RaisePropertyChanged("Label");
            }
        }

        [JsonProperty("Launcher", Order = 3)]
        public string Launcher
        {
            get
            {
                return _launcher;
            }
            set
            {
                _launcher = value;
                RaisePropertyChanged("Launcher");
            }
        }

        [JsonProperty("Flags", Order = 4)]
        public string Flags
        {
            get
            {
                return _flags;
            }
            set
            {
                _flags = value;
                RaisePropertyChanged("Flags");
            }
        }

        [JsonProperty("WasLastRan", Order = 5)]
        public bool WasLastRan
        {
            get
            {
                return _wasLastRan;
            }
            set
            {
                _wasLastRan = value;
                RaisePropertyChanged("WasLastRan");
            }
        }

        #region Migration Only
        public bool ShouldSerializePath()
        {
            return false;
        }

        private string _path;

        [JsonProperty("Path")]
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
            }
        }
        #endregion
    }
}
