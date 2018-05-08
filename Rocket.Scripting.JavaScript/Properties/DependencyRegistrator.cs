using Rocket.API.DependencyInjection;
using Rocket.API.Plugins;

namespace Rocket.Scripting.JavaScript.Properties
{
    public class DependencyRegistrator : IDependencyRegistrator
    {
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterSingletonType<IPluginManager, JavaScriptScriptingProvider>("javascript_plugins");
            var instance = (JavaScriptScriptingProvider) container.Resolve<IPluginManager>("javascript_plugins");
            container.RegisterSingletonInstance<IScriptingProvider>(instance, "javascript_scripts");
        }
    }
}