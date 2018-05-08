using System;
using Jurassic;
using Rocket.API.DependencyInjection;

namespace Rocket.Scripting.JavaScript
{
    public class JavaScriptContext : IScriptContext
    {
        public JavaScriptContext(IDependencyContainer container, ScriptEngine scriptEngine)
        {
            Container = container;
            ScriptEngine = scriptEngine;
        }

        public ScriptEngine ScriptEngine { get; }

        public IDependencyContainer Container { get; }

        public void ExposeType(string name, Type type)
        {
        }

        public void SetGlobalVariable(string name, object value)
        {
            ScriptEngine.SetGlobalValue(name, value);
        }

        public void SetGlobalFunction(string name, Delegate function)
        {
            ScriptEngine.SetGlobalFunction(name, function);
        }

        public void UnregisterGlobalVariable(string name)
        {
            ScriptEngine.SetGlobalValue(name, null);
        }

        public void UnregisterGlobalFunction(string name)
        {
            ScriptEngine.SetGlobalFunction(name, null);
        }

        public ScriptResult Eval(string expression)
        {
            try
            {
                var ret = ScriptEngine.Evaluate(expression);
                return new ScriptResult(ScriptExecutionResult.Success)
                {
                    HasReturn = !(ret is Undefined),
                    Return = ret is Null ? null : ret
                };
            }
            catch (JavaScriptException e)
            {
                throw new WrappedJavaScriptException(e);
            }
        }
    }
}