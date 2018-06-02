using Rocket.API.DependencyInjection;
using Rocket.API.Plugins;
using Rocket.Core.Plugins;

namespace Rocket.Scripting.CSharp
{
    public class CSharpScriptingProviderPlugin : Plugin
    {
        public CSharpScriptingProviderPlugin(IDependencyContainer container) : base("C# Scripting", container)
        {
        }

        protected override void OnLoad(bool isFromReload)
        {
            base.OnLoad(isFromReload);
            if (!isFromReload)
            {
                Container.Resolve<IPluginManager>("csharp_plugins").Init();
            }
        }
    }
}