using System;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Rocket.API.DependencyInjection;

namespace Rocket.Scripting.JavaScript
{
    public class JavaScriptContext : IScriptContext
    {
        public JavaScriptContext(IDependencyContainer container, Engine scriptEngine)
        {
            Container = container;
            ScriptEngine = scriptEngine;
        }

        public Engine ScriptEngine { get; }

        public IDependencyContainer Container { get; }

        public void ExposeType(string name, Type type)
        {
        }

        public void SetGlobalVariable(string name, object value)
        {
            ScriptEngine.SetValue(name, value);
        }

        public void SetGlobalFunction(string name, Delegate function)
        {
            //ScriptEngine.SetGlobalFunction(name, function);
            SetGlobalVariable(name, function);
        }

        public void UnregisterGlobalVariable(string name)
        {
            ScriptEngine.SetValue(name, JsValue.Undefined);
        }

        public void UnregisterGlobalFunction(string name)
        {
            //ScriptEngine.SetGlobalFunction(name, null);
            UnregisterGlobalVariable(name);
        }

        public ScriptResult Eval(string expression)
        {
            try
            {
                ScriptEngine.Execute(expression);
                var ret = ScriptEngine.GetCompletionValue();

                return new ScriptResult(ScriptExecutionResult.Success)
                {
                    HasReturn = ret.Type != Types.None,
                    Return = ret == JsValue.Null ? null : ret
                };
            }
            catch (JavaScriptException e)
            {
                throw new FriendlyJavaScriptException(e);
            }
        }
    }
}