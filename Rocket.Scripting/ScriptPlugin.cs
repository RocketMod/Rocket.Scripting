using System.IO;
using Rocket.API.Configuration;
using Rocket.API.DependencyInjection;
using Rocket.API.Plugins;
using Rocket.Core.Configuration;
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

        protected IDependencyContainer Container { get; }

        public bool IsAlive { get; protected set; }

        public string Name { get; }

        public string WorkingDirectory => Path.Combine(_provider.WorkingDirectory, Name);
        public string ConfigurationName => Name;

        public bool Activate()
        {
            IsAlive = true;
            return OnActivate();
        }
        protected abstract bool OnActivate();

        public bool Deactivate()
        {
            bool result = OnDeactivate();
            IsAlive = false;
            return result;
        }

        protected abstract bool OnDeactivate();

        public void Reload()
        {
            Deactivate();
            Activate();
        }
    }
}