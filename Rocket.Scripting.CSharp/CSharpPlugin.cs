using Rocket.API.DependencyInjection;
using Rocket.Core.DependencyInjection;

namespace Rocket.Scripting.CSharp
{
    [DontAutoRegister]
    public class CSharpPlugin: ScriptPlugin
    {
        public CSharpPlugin(ScriptPluginMeta pluginMeta, IDependencyContainer container, ScriptingProvider provider, CSharpScriptContext context) : base(pluginMeta, container, provider, context)
        {
        }

        protected override bool OnActivate()
        {
            return true;
        }

        protected override bool OnDeactivate()
        {
            return true;
        }
    }
}