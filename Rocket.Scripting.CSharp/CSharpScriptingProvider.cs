using System.Collections.Generic;
using System.IO;
using CSScriptLibrary;
using Rocket.API.DependencyInjection;
using Rocket.API.Plugins;

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

        public override ScriptResult ExecuteFile(string path, string entryPoint, IDependencyContainer container, ref IScriptContext context,
            ScriptPluginMeta meta, bool createPluginInstanceOnNull = false)
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

                    context.SetGlobalVariable("plugin", plugin);
                }
            }

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

        protected override void OnInit()
        {
            foreach (var directory in Directory.GetDirectories(WorkingDirectory))
            {
                var pluginFile = Path.Combine(directory, "plugin.json");
                if (!File.Exists(pluginFile))
                    continue;

                var meta = GetPluginMeta(pluginFile);
                var entryFile = Path.Combine(directory, meta.EntryFile);
                if (!File.Exists(entryFile))
                    throw new FileNotFoundException(null, entryFile);

                IScriptContext context = null;
                ExecuteFile(entryFile, meta.EntryPoint, Container, ref context, meta, true);
            }
        }

        public override string ServiceName => "C#";
    }
}
