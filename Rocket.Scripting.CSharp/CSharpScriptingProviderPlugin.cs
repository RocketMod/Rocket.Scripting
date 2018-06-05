using Rocket.API.DependencyInjection;
using Rocket.API.Plugins;
using Rocket.Core.Plugins;

namespace Rocket.Scripting.CSharp
{
    public class CSharpScriptingProviderPlugin : Plugin
    {
        public CSharpScriptingProviderPlugin(IDependencyContainer container) : base("C#_ScriptProvider", container)
        {
        }

        protected override void OnLoad(bool isFromReload)
        {
            base.OnLoad(isFromReload);
            Container.Resolve<IScriptingProvider>("csharp").LoadPlugins();
        }

        protected override void OnUnload()
        {
            //likely not supported
            Container.Resolve<IScriptingProvider>("csharp").UnloadPlugins();
        }
    }
}