using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Rocket.API.DependencyInjection;
using Rocket.API.Plugins;

namespace Rocket.Scripting.JavaScript
{
    public class JavaScriptScriptingProvider : ScriptingProvider
    {
        public JavaScriptScriptingProvider(IDependencyContainer container) : base(container)
        {
        }

        public override string[] FileTypes => new[] { "js" };
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
                ExecuteFile(entryFile, Container, ref context, meta, true);
            }
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
                    var plugin = (JavaScriptPlugin)GetPlugin(meta.Name);
                    if (plugin == null)
                    {
                        plugin = new JavaScriptPlugin(meta, context.Container, this, (JavaScriptContext)context);
                        AddPlugin(plugin);
                    }

                    plugin.Load(false);
                    return new ScriptResult(ScriptExecutionResult.Success);
                }
            }

            var engine = ((JavaScriptContext)context).ScriptEngine;
            if (engine == null)
            {
                return new ScriptResult(ScriptExecutionResult.FailedMisc);
            }

            ExecuteFromWhileWithImports(engine, path);

            if (entryPoint != null)
            {
                engine.Invoke(entryPoint, arguments);
            }

            var ret = engine.GetCompletionValue();

            return new ScriptResult(ScriptExecutionResult.Success)
            {
                HasReturn = ret.Type != Types.None,
                Return = ret == JsValue.Null ? null : ret
            };
        }

        private void ExecuteFromWhileWithImports(Engine engine, string filePath)
        {
            var currentDir = Path.GetDirectoryName(filePath);

            string sourceCode = File.ReadAllText(filePath);

            Regex regex = new Regex("import \"(.*)\\.js\"");
            var matches = regex.Matches(sourceCode);

            foreach (Match match in matches)
            {
                string expr = match.Value;
                sourceCode = sourceCode.Replace(expr, "");
                sourceCode = sourceCode.Replace(expr + Environment.NewLine, "");
                sourceCode = sourceCode.Replace(expr + ";", "");
                sourceCode = sourceCode.Replace(expr + ";" + Environment.NewLine, "");

                var fileName = match.Groups[0].Value;
                ExecuteFromWhileWithImports(engine, Path.Combine(currentDir, fileName + ".js"));
            }

            engine.Execute(sourceCode);
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
            var engine = new Engine((options) => options.AllowClr(AppDomain.CurrentDomain.GetAssemblies()));

            var ctx = new JavaScriptContext(container, engine);
            RegisterContext(ctx);
            return ctx;
        }

        public override string ServiceName => "JavaScript";
    }
}