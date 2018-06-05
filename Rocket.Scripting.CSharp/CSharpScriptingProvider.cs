using System;
using System.Collections.Generic;
using System.IO;
using CSScriptLibrary;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;
using Rocket.API.Plugins;
using Rocket.Core.Logging;

namespace Rocket.Scripting.CSharp
{
    class CSharpScriptingProvider : ScriptingProvider
    {
        public CSharpScriptingProvider(IDependencyContainer container) : base(container)
        {
        }

        public override string[] FileTypes => new[] { "cs" };
        public override string ScriptName => "C#";
        public override IEnumerable<IPlugin> Plugins { get; }
        private List<IPlugin> _plugins = new List<IPlugin>();

        public override IScriptContext CreateScriptContext(IDependencyContainer container)
        {
            IEvaluator evaluator = CSScript.CodeDomEvaluator.Clone();
            evaluator.ReferenceDomainAssemblies();

            var ctx = new CSharpScriptContext(container, evaluator);
            RegisterContext(ctx);
            return ctx;
        }

        public override ScriptResult ExecuteFile(
            string path,
            IDependencyContainer container,
            ref IScriptContext context,
            ScriptPluginMeta meta,
            bool createPluginInstanceOnNull = false,
            string entryPoint = null,
            params object[] arguments
        )
        {
            if (context == null)
            {
                context = CreateScriptContext(container.CreateChildContainer());

                if (createPluginInstanceOnNull)
                {
                    var plugin = (CSharpPlugin)GetPlugin(meta.Name);
                    if (plugin == null)
                    {
                        plugin = new CSharpPlugin(meta, context.Container, this, (CSharpScriptContext)context);
                        _plugins.Add(plugin);
                    }

                    ((CSharpScriptContext)context).Plugin = plugin;
                    context.SetGlobalVariables();

                    plugin.Load(false);
                    return new ScriptResult(ScriptExecutionResult.Success);
                }
            }

            context.SetGlobalVariables();

            var engine = ((CSharpScriptContext)context).Evaluator;
            if (engine == null)
            {
                return new ScriptResult(ScriptExecutionResult.FailedMisc);
            }

            engine.LoadFile(path);

            /*
            var ret = engine.CallGlobalFunction(entryPoint, context);
            var res = new ScriptResult(ScriptExecutionResult.Success)
            {
                HasReturn = ret is Undefined,
                Return = ret is Null ? null : ret
            };
            return res;
            */

            return new ScriptResult(ScriptExecutionResult.Success);
        }

        public override void LoadPlugins()
        {
            foreach (var directory in Directory.GetDirectories(WorkingDirectory))
            {
                var pluginFile = Path.Combine(directory, "plugin.json");
                if (!File.Exists(pluginFile))
                    continue;

                var meta = GetPluginMeta(directory);
                var entryFile = Path.Combine(directory, meta.EntryFile);
                if (!File.Exists(entryFile))
                    throw new FileNotFoundException(null, entryFile);

                IScriptContext context = null;

                try
                {
                    ExecuteFile(entryFile, Container, ref context, meta, true);
                }
                catch (Exception ex)
                {
                    Container.Resolve<ILogger>().LogFatal("Failed to load script: " + meta.Name, ex);
                }
            }
        }

        public override void UnloadPlugins()
        {
            foreach (var plugin in _plugins)
            {
                plugin.Unload();
            }

            _plugins.Clear();
        }

        public override string ServiceName => "C#";
    }
}
