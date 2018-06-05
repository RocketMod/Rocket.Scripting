using Rocket.API.DependencyInjection;
using Rocket.Core.Plugins;

namespace Rocket.Scripting.JavaScript
{
    public class JavaScriptScriptingPlugin : Plugin
    {
        public JavaScriptScriptingPlugin(IDependencyContainer container) : base("JavaScript_ScriptProvider", container)
        {
        }

        protected override void OnLoad(bool isFromReload)
        {
            base.OnLoad(isFromReload);
            Container.Resolve<IScriptingProvider>("javascript").LoadPlugins();
        }

        protected override void OnUnload()
        {
            Container.Resolve<IScriptingProvider>("javascript").UnloadPlugins();
        }
    }
}