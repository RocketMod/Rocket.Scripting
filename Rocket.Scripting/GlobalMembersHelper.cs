using System;
using System.Drawing;
using Rocket.API;
using Rocket.API.Economy;
using Rocket.API.Eventing;
using Rocket.API.Logging;
using Rocket.API.Permissions;
using Rocket.API.Player;
using Rocket.API.Plugins;
using Rocket.API.Scheduler;
using Rocket.API.User;

namespace Rocket.Scripting
{
    public static class GlobalMembersHelper
    {
        public static void SetGlobalVariables(this IScriptContext context)
        {
            context.SetGlobalVariable("container", context.Container);
            context.SetGlobalVariable("plugin", context.Plugin);

            TryRegisterService<ILogger>(context, "logger");
            TryRegisterService<ITaskScheduler>(context, "scheduler");
            TryRegisterService<IPlayerManager>(context, "playermanager");
            TryRegisterService<IPluginManager>(context, "plugins");
            TryRegisterService<IEconomyProvider>(context, "economy");
            TryRegisterService<IUserManager>(context, "usermanager");
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

            var host = context.Container.Resolve<IHost>();
            context.SetGlobalFunction("print", new Action<string>(message =>
            {
                ConsoleColor bak = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(context.Plugin?.Name ?? "Script");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("] ");

                Console.WriteLine(message);

                Console.ForegroundColor = bak;
            }));

            context.SetGlobalFunction("print_color", new Action<string, int>((message, c) =>
            {
                ConsoleColor bak = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(context.Plugin?.Name ?? "Script");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("] ");

                Console.ForegroundColor = (ConsoleColor) c;
                Console.WriteLine(message);

                Console.ForegroundColor = bak;
            }));

            context.SetGlobalFunction("log_level", new Action<string, int>((message, logLevel) =>
            {
                context.Container.Resolve<ILogger>().Log(message, (LogLevel)logLevel);
            }));
        }

        public static void TryRegisterService<TService>(this IScriptContext context, string name)
        {
            if (context.Container.TryResolve(null, out TService service))
                context.SetGlobalVariable(name, service);
        }
    }
}