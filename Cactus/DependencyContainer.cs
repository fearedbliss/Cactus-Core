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