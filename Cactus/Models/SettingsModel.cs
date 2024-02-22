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

using Newtonsoft.Json;

namespace Cactus.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SettingsModel
    {
        [JsonProperty("rootDirectory", Order = 1)]
        public string RootDirectory { get; set; } = "";

        [JsonProperty("backupsDirectory", Order = 2)]
        public string BackupsDirectory { get; set; } = "";

        [JsonProperty("shouldMinimizeToTray", Order = 3)]
        public bool ShouldMinimizeToTray { get; set; }

        [JsonProperty("shouldEnableDarkMode", Order = 4)]
        public bool ShouldEnableDarkMode { get; set; }

        [JsonProperty("preferredColor", Order = 5)]
        public string PreferredColor { get; set; } = "Teal";

        [JsonProperty("hasMigratedToNewFormat", Order = 6)]
        public bool HasMigratedToNewFormat { get; set; }
    }
}
