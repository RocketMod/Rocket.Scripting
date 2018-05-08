using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rocket.API;
using Rocket.API.Chat;
using Rocket.API.Configuration;
using Rocket.API.DependencyInjection;
using Rocket.API.Economy;
using Rocket.API.Eventing;
using Rocket.API.Logging;
using Rocket.API.Permissions;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.API.Scheduler;
using Rocket.Core.Configuration;
using Rocket.Core.Logging;

namespace Rocket.Scripting
{
    public abstract class ScriptingProvider : IPluginManager, IConfigurationContext, IScriptingProvider
    {
        public IDependencyContainer Container { get; }

        protected ScriptingProvider(IDependencyContainer container)
        {
            Container = container.CreateChildContainer();
        }

        /// <summary>
        ///     List of all scripting contexts
        /// </summary>
        public ICollection<IScriptContext> Contexts { get; } = new List<IScriptContext>();

        /// <summary>
        /// File types associated with the scripting language (e.g. ".js", ".javascript")
        /// </summary>
        public abstract List<string> FileTypes { get; }

        /// <summary>
        /// Full name of the scripting language (e.g. "JavaScript")
        /// </summary>
        public abstract string ScriptName { get; }

        /// <inheritdoc />
        public abstract IEnumerable<IPlugin> Plugins { get; }

        /// <inheritdoc />
        public IEnumerator<IPlugin> GetEnumerator()
        {
            return Plugins.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public IPlugin GetPlugin(string name)
        {
            return Plugins.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public bool PluginExists(string name)
        {
            return GetPlugin(name) != null;
        }

        /// <inheritdoc />
        public void Init()
        {
            BaseLogger.SkipTypeFromLogging(typeof(GlobalMembersHelper));

            if (!Directory.Exists(WorkingDirectory))
                Directory.CreateDirectory(WorkingDirectory);

            OnInit();
        }

        protected abstract void OnInit();

        /// <inheritdoc />
        public void ExecuteSoftDependCode(string pluginName, Action<IPlugin> action)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// <p>Searches for the "Plugin.[filetype]" (e.g. Plugin.js) and runs it</p>
        /// <p>Will generate a new plugin instance if context is null</p>
        /// </summary>
        /// <param name="path">The path to the directory containing the script files</param>
        /// <param name="context">The script context. Can be null if there is none.</param>
        public virtual ScriptResult LoadPluginFromDirectory(string path, ref IScriptContext context)
        {
            var files = Directory.GetFiles(path);
            if (files.Length == 0)
            {
                return new ScriptResult(ScriptExecutionResult.FileNotFound);
            }

            ScriptPluginMeta meta = GetPluginMeta(path);
            string targetFile = null;
            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string ext = Path.GetExtension(file);

                if (fileName.Equals("plugin", StringComparison.OrdinalIgnoreCase) &&
                    FileTypes.Any(c => c.Equals(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    targetFile = file;
                }
            }

            if (targetFile == null)
                return new ScriptResult(ScriptExecutionResult.FileNotFound);

            return ExecuteFile(targetFile, meta.EntryPoint, Container, ref context, meta, true);
        }

        /// <summary>
        /// Get information about the plugin in the given directory.
        /// </summary>
        /// <param name="path">The plugin directory.</param>
        /// <returns></returns>
        public virtual ScriptPluginMeta GetPluginMeta(string path)
        {
            IConfiguration jsonConfiguration = Container.Resolve<IConfiguration>("json");
            var metaFile = Path.Combine(path, "plugin.json");
            if (!File.Exists(metaFile))
                throw new FileNotFoundException(null, metaFile);

            var dir = Path.GetDirectoryName(metaFile);
            var name = Path.GetFileNameWithoutExtension(metaFile);

            var configContext = new ConfigurationContext(dir, name);
            jsonConfiguration.Load(configContext);
            return jsonConfiguration.Get<ScriptPluginMeta>();
        }

        /// <summary>
        /// Registers the context to the scripting engine.
        /// </summary>
        /// <param name="context">The script context.</param>
        public virtual void RegisterContext(IScriptContext context)
        {
            context.SetGlobalVariables();
            Contexts.Add(context);
        }

        public virtual void UnregisterContext(IScriptContext context)
        {
            Contexts.Remove(context);
        }

        /// <summary>
        /// Executes the given script file
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="entryPoint">The entry function to call.</param>
        /// <param name="context">The script context. Can be null if there is none.</param>
        /// <param name="createPluginInstanceOnNull">Create a new plugin instance when <paramref name="context"/> is null?</param>
        /// <param name="container">The dependency container.</param>
        /// <param name="meta">Metadata for the plugin if <paramref name="createPluginInstanceOnNull"/> is true and <paramref name="context"/> is null. Can be null if <paramref name="createPluginInstanceOnNull"/> is false.</param>
        /// <returns>The result of the script execution.</returns>
        public abstract ScriptResult ExecuteFile(string path, string entryPoint, IDependencyContainer container,
            ref IScriptContext context, ScriptPluginMeta meta, bool createPluginInstanceOnNull = false);

        /// <summary>
        /// Executes the given script file
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="entryPoint">The entry function to call.</param>
        /// <param name="context">The script context. Can be null if there is none.</param>
        /// <param name="container">The dependency container.</param>
        /// <returns>The result of the script execution.</returns>
        public virtual ScriptResult ExecuteFile(string path, string entryPoint, IDependencyContainer container, ref IScriptContext context) =>
            ExecuteFile(path, entryPoint, container, ref context, null, false);

        public abstract IScriptContext CreateScriptContext(IDependencyContainer container);

        /// <inheritdoc />
        public string WorkingDirectory
        {
            get
            {
                var baseDir = Container.Resolve<IRuntime>().WorkingDirectory;
                return Path.Combine(Path.Combine(baseDir, "Scripting"), ScriptName);
            }
        }

        /// <inheritdoc />
        public string ConfigurationName => ScriptName;
    }
}
