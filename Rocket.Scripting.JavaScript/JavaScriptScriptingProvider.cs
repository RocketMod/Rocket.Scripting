using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;
using Rocket.API.Plugins;
using Rocket.Core.Logging;

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
        public override void LoadPlugins()
        {
            if (!Directory.Exists(WorkingDirectory))
                Directory.CreateDirectory(WorkingDirectory);

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
            foreach (var plugin in Plugins)
            {
                plugin.Unload();
            }

            ((List<IPlugin>)Plugins).Clear();
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

                    ((JavaScriptContext)context).Plugin = plugin;
                    context.SetGlobalVariables();

                    ExecuteFromFileWithImports(((JavaScriptContext)context).ScriptEngine, path);
                    plugin.Load(false);
                    return new ScriptResult(ScriptExecutionResult.Success);
                }
            }

            context.SetGlobalVariables();
            var engine = ((JavaScriptContext)context).ScriptEngine;
            if (engine == null)
            {
                return new ScriptResult(ScriptExecutionResult.FailedMisc);
            }

            ExecuteFromFileWithImports(engine, path);

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

        private void ExecuteFromFileWithImports(Engine engine, string filePath)
        {
            var currentDir = Path.GetDirectoryName(filePath);

            string sourceCode = File.ReadAllText(filePath);

            Regex regex = new Regex("import ?\\((?<2>[\'\"])(?<1>.*).js\\2\\)|import ?(?<2>[\'\"])(?<1>.*).js\\2[^)]");

            var matches = regex.Matches(sourceCode);

            foreach (Match match in matches)
            {
                string expr = match.Value;
                sourceCode = sourceCode.Replace(expr, "");
                sourceCode = sourceCode.Replace(expr + Environment.NewLine, "");
                sourceCode = sourceCode.Replace(expr + ";", "");
                sourceCode = sourceCode.Replace(expr + ";" + Environment.NewLine, "");

                var fileName = match.Groups[0].Value;
                ExecuteFromFileWithImports(engine, Path.Combine(currentDir, fileName + ".js"));
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
            var logger = container.Resolve<ILogger>();
            List<Assembly> assemblies = new List<Assembly>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    assembly.GetTypes();
                    assemblies.Add(assembly);
                    /* only add assemblies we can get the types of to fix an issue with jint */
                }
                catch
                {
                }
            }

            var engine = new Engine((options) => options.AllowClr(assemblies.ToArray()));

            var ctx = new JavaScriptContext(container, engine);
            RegisterContext(ctx);
            return ctx;
        }

        public override string ServiceName => "JavaScript";
    }
}