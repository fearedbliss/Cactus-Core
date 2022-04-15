// Copyright © 2018-2022 Jonathan Vasquez <jon@xyinn.org>
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
using System.Collections.Generic;
using System.IO;

namespace Cactus
{
    /// <summary>
    /// This class is responsible for returning a list of all of
    /// the files corresponding to a specific patch.
    /// </summary>
    public class FileGenerator : IFileGenerator
    {
        private readonly IPathBuilder _pathBuilder;
        private readonly IJsonManager _jsonManager;
        private readonly ILogger _logger;

        public FileGenerator(IPathBuilder pathBuilder, IJsonManager jsonManager, ILogger logger)
        {
            _pathBuilder = pathBuilder;
            _jsonManager = jsonManager;
            _logger = logger;
        }

        private List<string> ProtectedDocuments
        {
            get
            {
                var protectedDocuments = new List<string>()
                {
                    "Platforms",
                    "Saves",
                    "Save",
                    "d2char.mpq",
                    "d2data.mpq",
                    "d2music.mpq",
                    "d2sfx.mpq",
                    "d2speech.mpq",
                    "d2video.mpq",
                    "D2.LNG",
                };

                var expansionDocuments = ExpansionMpqs;
                protectedDocuments.AddRange(expansionDocuments);

                var cactusManagedFiles = _jsonManager.ManagedFiles;
                protectedDocuments.AddRange(cactusManagedFiles);

                return protectedDocuments;
            }
        }

        public List<string> ExpansionMpqs
        {
            get
            {
                return new List<string>()
                {
                    "d2exp.mpq",
                    "d2xmusic.mpq",
                    "d2xvideo.mpq",
                    "d2xtalk.mpq"
                };
            }
        }

        public RequiredFilesModel GetRequiredFiles(EntryModel entry)
        {
            var requiredFiles = new RequiredFilesModel();
            var platformDirectory = _pathBuilder.GetPlatformDirectory(entry);

            if (Directory.Exists(platformDirectory))
            {
                var directories = Directory.GetDirectories(platformDirectory);
                var processedDirectories = new List<string>();

                foreach (var directory in directories)
                {
                    var directoryName = Path.GetFileName(directory);
                    processedDirectories.Add(directoryName);
                }

                var files = Directory.GetFiles(platformDirectory);
                var processedFiles = new List<string>();

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    processedFiles.Add(fileName);
                }

                requiredFiles.Directories = processedDirectories;
                requiredFiles.Files = processedFiles;

                ValidateRequiredFiles(requiredFiles);
            }

            return requiredFiles;
        }

        /// <summary>
        /// Returns an empty required files collection.
        /// </summary>
        public RequiredFilesModel GetEmptyRequiredFiles()
        {
            return new RequiredFilesModel();
        }

        /// <summary>
        /// Scans all of the files in the list and removes any files that are protected.
        /// </summary>
        public void ValidateRequiredFiles(RequiredFilesModel requiredFiles)
        {
            var directoriesToRemove = new List<string>();
            var filesToRemove = new List<string>();

            foreach (var directory in requiredFiles.Directories)
            {
                if (IsProtected(directory))
                {
                    directoriesToRemove.Add(directory);
                }
            }

            foreach (var file in requiredFiles.Files)
            {
                if (IsProtected(file))
                {
                    filesToRemove.Add(file);
                }
            }

            foreach (var directory in directoriesToRemove)
            {
                requiredFiles.Directories.Remove(directory);
            }

            foreach (var file in filesToRemove)
            {
                requiredFiles.Files.Remove(file);
            }
        }

        private bool IsProtected(string document)
        {
            // No files or directories that are within the protected list are allowed to be tracked/deleted.
            foreach (var protectedDocument in ProtectedDocuments)
            {
                if (document.EqualsIgnoreCase(protectedDocument))
                {
                    _logger.LogWarning($"Protected file/directory \"{document}\" detected in list. Skipping it for protection.");
                    return true;
                }
            }
            return false;
        }
    }
}
