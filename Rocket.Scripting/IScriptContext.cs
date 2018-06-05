using System;
using Rocket.API.DependencyInjection;
using Rocket.API.Plugins;

namespace Rocket.Scripting
{
    public interface IScriptContext
    {
        IDependencyContainer Container { get; }
        IPlugin Plugin { get; }

        void ExposeType(string name, Type type);
        void SetGlobalVariable(string name, object value);
        void SetGlobalFunction(string name, Delegate function);

        ScriptResult Eval(string expression);
    }
}