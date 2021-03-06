﻿using Rocket.API.DependencyInjection;

namespace Rocket.Scripting
{
    public interface IScriptingProvider
    {
        string[] FileTypes { get; }
        
        string ScriptName { get; }

        void RegisterContext(IScriptContext context);

        void UnregisterContext(IScriptContext context);

        bool SupportsRepl { get; }

        ScriptResult ExecuteFile(
            string path, 
            IDependencyContainer container,
            ref IScriptContext context, 
            ScriptPluginMeta meta, 
            bool createPluginInstanceOnNull = false,
            string entryPoint = null,
            params object[] arguments
        );

        ScriptResult ExecuteFile(
            string path, 
            IDependencyContainer container, 
            ref IScriptContext context,
            string entryPoint = null,
            params object[] arguments);

        IScriptContext CreateScriptContext(IDependencyContainer container);
        void LoadPlugins();

        void UnloadPlugins();
    }
}