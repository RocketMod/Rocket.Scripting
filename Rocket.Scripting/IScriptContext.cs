using System;
using Rocket.API.DependencyInjection;

namespace Rocket.Scripting
{
    public interface IScriptContext
    {
        IDependencyContainer Container { get; }

        void ExposeType(string name, Type type);
        void SetGlobalVariable(string name, object value);
        void SetGlobalFunction(string name, Delegate function);

        ScriptResult Eval(string expression);
    }
}