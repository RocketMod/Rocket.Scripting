using Rocket.API.DependencyInjection;
using Rocket.API.Plugins;

namespace Rocket.Scripting.CSharp.Properties
{
    public class DependencyRegistrator : IDependencyRegistrator
    {
        public void Register(IDependencyContainer container, IDependencyResolver resolver)
        {
            container.RegisterSingletonType<IPluginManager, CSharpScriptingProvider>("csharp_plugins");
            var instance = (CSharpScriptingProvider) container.Resolve<IPluginManager>("csharp_plugins");
            container.RegisterSingletonInstance<IScriptingProvider>(instance, "csharp");
        }
    }
}