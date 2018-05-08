using System;
using System.Collections.Generic;
using System.IO;
using Jurassic;
using Rocket.API.DependencyInjection;
using Rocket.API.Plugins;

namespace Rocket.Scripting.JavaScript
{
    public class JavaScriptScriptingProvider : ScriptingProvider
    {
        public JavaScriptScriptingProvider(IDependencyContainer container) : base(container)
        {
        }

        public override List<string> FileTypes => new List<string> { "js" };
        public override string ScriptName => "JavaScript";
        public override IEnumerable<IPlugin> Plugins { get; } = new List<IPlugin>();
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

        public override ScriptResult ExecuteFile(string path, string entryPoint, IDependencyContainer container, ref IScriptContext context, ScriptPluginMeta meta, bool createPluginInstanceOnNull = false)
        {
            if (context == null)
            {
                context = CreateScriptContext(container.CreateChildContainer());

                if (createPluginInstanceOnNull)
                {
                    var plugin = (JavaScriptPlugin)GetPlugin(meta.Name);
                    if (plugin == null)
                    {
                        plugin = new JavaScriptPlugin(meta, context.Container, this, (JavaScriptContext)context);
                        AddPlugin(plugin);
                    }

                    context.SetGlobalVariable("plugin", plugin);
                }
            }

            var engine = ((JavaScriptContext)context).ScriptEngine;
            if (engine == null)
            {
                return new ScriptResult(ScriptExecutionResult.FailedMisc);
            }

            var ret = engine.CallGlobalFunction(entryPoint, context);
            var res = new ScriptResult(ScriptExecutionResult.Success)
            {
                HasReturn = ret is Undefined,
                Return = ret is Null ? null : ret
            };
            return res;
        }

        protected void AddPlugin(ScriptPlugin plugin)
        {
            ((List<IPlugin>)Plugins).Add(plugin);
        }

        protected void RemovePlugin(ScriptPlugin plugin)
        {
            ((List<IPlugin>)Plugins).Remove(plugin);
        }

        public bool AllowReflection { get; set; } = true;

        public override IScriptContext CreateScriptContext(IDependencyContainer container)
        {
            var engine = new ScriptEngine();
            engine.EnableExposedClrTypes = true;

            var ctx = new JavaScriptContext(container, engine);
            RegisterContext(ctx);
            return ctx;
        }
    }
}