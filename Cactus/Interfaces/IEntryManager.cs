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

using Cactus.Models;
using System.Collections.Generic;

namespace Cactus.Interfaces
{
    /// <summary>
    /// The Entry Manager is responsible for managing all of the entries in the
    /// collection of Diablo II versions that player wants to play.
    /// </summary>
    public interface IEntryManager
    {
        EntryModel GetLastRan();
        void Add(EntryModel entry);
        void Delete(int targetIndex);
        EntryModel Copy(EntryModel entry);
        void MoveUp(EntryModel entry);
        void MoveDown(EntryModel entry);
        List<EntryModel> GetEntries();
        void MarkLastRan(EntryModel entry);
        void SwapLastRan(EntryModel oldEntry, EntryModel newEntry);
        void SaveEntries();
        void RenamePlatform(string oldPlatformName, string newPlatformName);
        void RenameLabel(string platformName, string oldLabelName, string newLabelName);
        bool DoesPlatformNameMatchLastRan(string platformName);
        bool IsInvalid(EntryModel entry);
        bool DoesPlatformAndLabelExist(EntryModel entry, bool excludeSelf = false);
    }
}
