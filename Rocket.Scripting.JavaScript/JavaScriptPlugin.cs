using Rocket.API.DependencyInjection;
using Rocket.Core.DependencyInjection;

namespace Rocket.Scripting.JavaScript
{
    [DontAutoRegister]
    public class JavaScriptPlugin : ScriptPlugin
    {
        public JavaScriptPlugin(ScriptPluginMeta pluginMeta, IDependencyContainer container, ScriptingProvider provider, JavaScriptContext context) : base(pluginMeta, container, provider, context)
        {
        }

        protected override bool OnActivate()
        {
            return (bool) 
                ((JavaScriptContext) ScriptContext).ScriptEngine.CallGlobalFunction(PluginMeta.EntryPoint, Container);
        }

        protected override bool OnDeactivate()
        {
            return true;
        }
    }
}