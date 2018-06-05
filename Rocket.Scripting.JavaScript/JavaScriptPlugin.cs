using Jint.Native;
using Jint.Runtime;
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

        protected override bool OnLoad(bool isFromReload)
        {
            var engine = ((JavaScriptContext) ScriptContext).ScriptEngine;
            engine.Invoke("Load", isFromReload);
            var result = engine.GetCompletionValue();
            return result.Type == Types.None || result == JsBoolean.True || result == JsValue.Undefined || result == JsValue.Null;
        }

        protected override bool OnUnload()
        {
            var engine = ((JavaScriptContext)ScriptContext).ScriptEngine;
            engine.Invoke("Unload");
            var result = engine.GetCompletionValue();
            return result.Type == Types.None || result == JsBoolean.True || result == JsValue.Undefined || result == JsValue.Null;
        }
    }
}