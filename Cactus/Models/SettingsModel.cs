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

using Newtonsoft.Json;

namespace Cactus.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SettingsModel
    {
        [JsonProperty("rootDirectory", Order = 1)]
        public string RootDirectory { get; set; } = "";

        [JsonProperty("shouldMinimizeToTray", Order = 2)]
        public bool ShouldMinimizeToTray { get; set; }

        [JsonProperty("shouldEnableDarkMode", Order = 3)]
        public bool ShouldEnableDarkMode { get; set; }

        [JsonProperty("preferredColor", Order = 4)]
        public string PreferredColor { get; set; } = "Teal";

        [JsonProperty("hasMigratedToNewFormat", Order = 5)]
        public bool HasMigratedToNewFormat { get; set; }
    }
}
