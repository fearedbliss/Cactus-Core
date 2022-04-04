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

using Cactus.Interfaces;
using Cactus.ViewModels;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace Cactus
{
    /// <summary>
    /// This class is responsible for providing all of the dependencies for the application.
    /// </summary>
    public class DependencyContainer
    {
        private readonly IWindsorContainer _container;

        public DependencyContainer()
        {
            _container = ConfigureServices();
        }

        private IWindsorContainer ConfigureServices()
        {
            var container = new WindsorContainer();
            container.Register(Component.For<IFileSwitcher>().ImplementedBy<FileSwitcher>());
            container.Register(Component.For<IEntryManager>().ImplementedBy<EntryManager>());
            container.Register(Component.For<ISettingsManager>().ImplementedBy<SettingsManager>());
            container.Register(Component.For<IProcessManager>().ImplementedBy<ProcessManager>());
            container.Register(Component.For<ILogger>().ImplementedBy<Logger>());
            container.Register(Component.For<IRegistryService>().ImplementedBy<RegistryService>());
            container.Register(Component.For<IMainWindowViewModel>().ImplementedBy<MainWindowViewModel>());
            container.Register(Component.For<IEditWindowViewModel>().ImplementedBy<EditWindowViewModel>());
            container.Register(Component.For<IAddWindowViewModel>().ImplementedBy<AddWindowViewModel>());
            container.Register(Component.For<ISettingsWindowViewModel>().ImplementedBy<SettingsWindowViewModel>());
            container.Register(Component.For<IFileGenerator>().ImplementedBy<FileGenerator>());
            container.Register(Component.For<IPathBuilder>().ImplementedBy<PathBuilder>());
            container.Register(Component.For<IJsonManager>().ImplementedBy<JsonManager>());
            return container;
        }

        public IMainWindowViewModel MainWindow
        {
            get
            {
                return _container.Resolve<IMainWindowViewModel>();
            }
        }

        public IEditWindowViewModel EditWindow
        {
            get
            {
                return _container.Resolve<IEditWindowViewModel>();
            }
        }

        public IAddWindowViewModel AddWindow
        {
            get
            {
                return _container.Resolve<IAddWindowViewModel>();
            }
        }

        public ISettingsWindowViewModel SettingsWindow
        {
            get
            {
                return _container.Resolve<ISettingsWindowViewModel>();
            }
        }

        public ISettingsManager SettingsManager
        {
            get
            {
                return _container.Resolve<ISettingsManager>();
            }
        }

        public IJsonManager JsonManager
        {
            get
            {
                return _container.Resolve<IJsonManager>();
            }
        }

        public IPathBuilder PathBuilder
        {
            get
            {
                return _container.Resolve<IPathBuilder>();
            }
        }

        public IEntryManager EntryManager
        {
            get
            {
                return _container.Resolve<IEntryManager>();
            }
        }
    }
}