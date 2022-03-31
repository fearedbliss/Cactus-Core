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

using System.Windows;

namespace Cactus
{
    public static class ResourceUtility
    {
        public static int GetResourceIndex(string uriString)
        {
            int themeIndex = -1;

            foreach (var resource in Application.Current.Resources.MergedDictionaries)
            {
                themeIndex += 1;

                if (resource.Source == null || !resource.Source.IsAbsoluteUri)
                {
                    continue;
                }

                if (resource.Source.AbsoluteUri.EqualsIgnoreCase(uriString))
                {
                    return themeIndex;
                }
            }

            return -1;
        }

        public static void AddResource(int index, ResourceDictionary resource)
        {
            Application.Current.Resources.MergedDictionaries.Insert(index, resource);
        }

        public static int RemoveResource(string uriString)
        {
            int removedIndex = GetResourceIndex(uriString);
            Application.Current.Resources.MergedDictionaries.RemoveAt(removedIndex);
            return removedIndex;
        }
    }
}
