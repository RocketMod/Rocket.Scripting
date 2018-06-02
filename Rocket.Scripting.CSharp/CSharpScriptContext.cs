using System;
using Rocket.API.DependencyInjection;

namespace Rocket.Scripting.CSharp
{
    public class CSharpScriptContext : IScriptContext
    {
        public CSharpScriptContext(IDependencyContainer container, IEvaluator evaluator)
        {
            Evaluator = evaluator;
            Container = container;
        }

        public IDependencyContainer Container { get; }
        public IEvaluator Evaluator { get; }

        public void ExposeType(string name, Type type)
        {
            Evaluator.ReferenceAssemblyByNamespace(type.Namespace);
        }

        public void SetGlobalVariable(string name, object value)
        {
            Evaluator.SetValue(name, value);
        }

        public void SetGlobalFunction(string name, Delegate function)
        {
            Evaluator.SetValue(name, function);
        }

        public ScriptResult Eval(string expression)
        {
            Evaluator.LoadCode(expression);

            //todo
            return new ScriptResult(ScriptExecutionResult.Success);
        }
    }
}