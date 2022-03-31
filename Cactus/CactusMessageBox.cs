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

using System;
using System.Windows;
using Cactus.Views;

namespace Cactus
{
    public class CactusMessageBox
    {
        public static void Show(string message)
        {
            try
            {
                var messageBoxWindow = new CactusMessageBoxView(message)
                {
                    Owner = Application.Current.MainWindow,
                };

                messageBoxWindow.ShowDialog();
            }
            catch (InvalidOperationException)
            {
                // If we failed to display our pretty message box, we will
                // fallback to the system version so that the user can
                // still receive the message even if it looks uglier.
                MessageBox.Show(message);
            }
        }
    }
}
