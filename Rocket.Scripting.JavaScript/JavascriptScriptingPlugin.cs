using Rocket.API.DependencyInjection;
using Rocket.API.Plugins;
using Rocket.Core.Plugins;

namespace Rocket.Scripting.JavaScript
{
    public class JavaScriptScriptingPlugin : Plugin
    {
        public JavaScriptScriptingPlugin(IDependencyContainer container) : base("JavaScriptPlugin", container)
        {
        }

        protected override void OnActivate(bool isFromReload)
        {
            base.OnActivate(isFromReload);
            if (!isFromReload)
            {
                Container.Resolve<IPluginManager>("javascript_plugins").Init();
            }
        }
    }
}