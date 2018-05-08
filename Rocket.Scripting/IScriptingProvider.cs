using System.Collections.Generic;
using Rocket.API.DependencyInjection;

namespace Rocket.Scripting
{
    public interface IScriptingProvider
    {
        List<string> FileTypes { get; }

        string ScriptName { get; }

        void RegisterContext(IScriptContext context);

        void UnregisterContext(IScriptContext context);

        ScriptResult ExecuteFile(string path, string entryPoint, IDependencyContainer container,
            ref IScriptContext context, ScriptPluginMeta meta, bool createPluginInstanceOnNull = false);

        ScriptResult ExecuteFile(string path, string entryPoint, IDependencyContainer container, ref IScriptContext context);

        IScriptContext CreateScriptContext(IDependencyContainer container);
    }
}