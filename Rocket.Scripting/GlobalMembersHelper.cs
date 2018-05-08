using System;
using System.ComponentModel;
using Rocket.API.Chat;
using Rocket.API.Economy;
using Rocket.API.Eventing;
using Rocket.API.Logging;
using Rocket.API.Permissions;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.API.Scheduler;

namespace Rocket.Scripting
{
    public static class GlobalMembersHelper
    {
        public static void SetGlobalVariables(this IScriptContext context)
        {
            context.SetGlobalVariable("container", context.Container);
            TryRegisterService<ILogger>(context, "logger");
            TryRegisterService<ITaskScheduler>(context, "scheduler");
            TryRegisterService<IPlayerManager>(context, "playermanager");
            TryRegisterService<IPluginManager>(context, "plugins");
            TryRegisterService<IEconomyProvider>(context, "economy");
            TryRegisterService<IChatManager>(context, "chat");
            TryRegisterService<IEventManager>(context, "events");
            TryRegisterService<IPermissionProvider>(context, "permissions");

            foreach (var level in Enum.GetValues(typeof(LogLevel)))
                context.SetGlobalVariable("level_" + level.ToString().ToLower(), (int)level);

            foreach (var color in Enum.GetValues(typeof(ConsoleColor)))
                context.SetGlobalVariable("color_" + color.ToString().ToLower(), (int)color);

            context.SetGlobalFunction("getType", new Func<string, Type>(Type.GetType));

            context.SetGlobalFunction("log", new Action<string>(message =>
            {
                context.Container.Resolve<ILogger>().Log(message);
            }));
            context.SetGlobalFunction("logex", new Action<string, int>((message, logLevel) =>
            {
                context.Container.Resolve<ILogger>().Log(message, (LogLevel)logLevel);
            }));
            context.SetGlobalFunction("log_color", new Action<string, int, int>((message, logLevel, color) =>
            {
                context.Container.Resolve<ILogger>().Log(message, (LogLevel)logLevel, null, (ConsoleColor?)color);
            }));
        }

        public static void TryRegisterService<TService>(this IScriptContext context, string name)
        {
            if (context.Container.TryResolve(null, out TService service))
                context.SetGlobalVariable(name, service);
        }
    }
}