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
using Microsoft.Win32;

namespace Cactus
{
    public class RegistryService : IRegistryService
    {
        private readonly IPathBuilder _pathBuilder;

        public RegistryService(IPathBuilder pathBuilder)
        {
            _pathBuilder = pathBuilder;
        }

        public void Update(EntryModel entry)
        {
            string diabloSubKey = @"Software\Blizzard Entertainment\Diablo II";

            // This check will help us only apply our "Save Directory when Registry Subtree Doesn't Exist Fix"
            // in this particular situation. If the user is in another type of weird state, then that most likely
            // means that they have been messing with their registry, and thus they should fix those issues.
            bool doesDiabloSubkeyExist;
            using (var key = Registry.CurrentUser.OpenSubKey(diabloSubKey))
            {
                doesDiabloSubkeyExist = key != null;
            }

            using (var key = Registry.CurrentUser.CreateSubKey(diabloSubKey))
            {
                string saveDirectory = _pathBuilder.GetSaveDirectory(entry);
                string rootDirectory = _pathBuilder.GetRootDirectory();

                key.SetValue("Save Path", saveDirectory);
                key.SetValue("NewSavePath", saveDirectory);
                key.SetValue("InstallPath", rootDirectory);

                if (!doesDiabloSubkeyExist)
                {
                    // Prevent the Hireling UI from automatically showing up when you enter a game.
                    // Whenever we are in this state, if the character has a Merc, the game will automatically
                    // show the Hireling UI upon entering a game. This never happens. Thus we are only fixing
                    // this registry key when we detect this situation.
                    key.SetValue("PopupHireling", 1);
                }
            }
        }
    }
}
