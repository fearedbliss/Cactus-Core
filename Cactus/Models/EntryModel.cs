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

using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace Cactus.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EntryModel : ViewModelBase
    {
        private bool _wasLastRan;
        private string _platform;
        private string _label;
        private string _path;
        private string _flags;
        private bool _isExpansion;

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

        [JsonProperty("Path", Order = 3)]
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                RaisePropertyChanged("Path");
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

        [JsonProperty("IsExpansion", Order = 5)]
        public bool IsExpansion
        {
            get
            {
                return _isExpansion;
            }
            set
            {
                _isExpansion = value;
                RaisePropertyChanged("IsExpansion");
            }
        }

        [JsonProperty("WasLastRan", Order = 6)]
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
    }
}
