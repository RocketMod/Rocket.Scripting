﻿using System.IO;
using Rocket.API.DependencyInjection;
using Rocket.API.Plugins;
using Rocket.Core.DependencyInjection;

namespace Rocket.Scripting
{
    [DontAutoRegister]
    public abstract class ScriptPlugin : IPlugin
    {
        private readonly ScriptingProvider _provider;

        protected ScriptPlugin(ScriptPluginMeta pluginMeta, IDependencyContainer container, ScriptingProvider provider, IScriptContext scriptContext)
        {
            _provider = provider;
            Container = container;
            Name = pluginMeta.Name;
            PluginMeta = pluginMeta;
            ScriptContext = scriptContext;
        }

        public ScriptPluginMeta PluginMeta { get; }

        public IScriptContext ScriptContext { get; }

        public IDependencyContainer Container { get; }

        public bool IsAlive { get; protected set; }

        public string Name { get; }

        public string WorkingDirectory => Path.Combine(_provider.WorkingDirectory, Name);
        public string ConfigurationName => Name;

        public bool Load(bool isFromReload)
        {
            IsAlive = true;
            return OnLoad(isFromReload);
        }
        protected abstract bool OnLoad(bool isFromReload);

        public bool Unload()
        {
            bool result = OnUnload();
            IsAlive = false;
            return result;
        }

        public IPluginManager PluginManager => _provider;

        protected abstract bool OnUnload();

        public void Reload()
        {
            Unload();
            Load(true);
        }
    }
}