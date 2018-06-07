using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Eventing;
using Rocket.API.User;
using Rocket.Core.Commands.Events;
using Rocket.Core.Player.Events;
using Rocket.Core.Plugins;
using Rocket.Core.User;
using Rocket.Core.User.Events;

namespace Rocket.Scripting
{
    class ScriptingPlugin : Plugin, IEventListener<PreCommandExecutionEvent>, IEventListener<UserChatEvent>
    {
        public ScriptingPlugin(IDependencyContainer container) : base("ScriptingPlugin", container)
        {
        }

        protected override void OnLoad(bool isFromReload)
        {
            EventManager.AddEventListener(this, this);
        }

        public IScriptContext StartSession(IDependencyContainer container, IUser user, string providerName)
        {
            var context = GetScriptContext(user);
            if (context != null)
                return context;

            var provider = GetScriptingProvider(container, providerName);
            if (provider != null)
            {
                context = provider.CreateScriptContext(container);
                context.SetGlobalVariables();
                context.SetGlobalVariable("me", user);
                ScriptContexts.Add(user, context);
                return context;
            }

            return null;
        }

        public IScriptingProvider GetScriptingProvider(IDependencyContainer container, string providerName)
        {
            var providers = container.ResolveAll<IScriptingProvider>();
            foreach (var provider in providers)
            {
                if (provider.ScriptName.Equals(providerName, StringComparison.OrdinalIgnoreCase)
                    || provider.FileTypes.Any(c => c.Equals(providerName, StringComparison.OrdinalIgnoreCase)))
                {
                    return provider;
                }
            }

            return null;
        }

        public IScriptContext GetScriptContext(IUser user)
        {
            foreach (var scriptContext in ScriptContexts)
            {
                if (scriptContext.Key.Id.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                    return scriptContext.Value;
            }

            return null;
        }

        public bool StopSession(IUser user)
        {
            return ScriptContexts.Keys
                .ToList()
                .Where(c => c.Id.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                .All(c => ScriptContexts.Remove(c));
        }

        public Dictionary<IUser, IScriptContext> ScriptContexts { get; } = new Dictionary<IUser, IScriptContext>();

        public void HandleEvent(IEventEmitter emitter, PreCommandExecutionEvent @event)
        {
            if (!(@event.User is IConsole console))
                return;

            var session = GetScriptContext(console);
            if (session == null)
                return;

            @event.IsCancelled = true;

            var cmd = @event.CommandLine;
            var result = session.Eval(cmd);

            if (result.HasReturn)
                console.SendMessage("> " + result.Return, ConsoleColor.Gray);
        }

        public void HandleEvent(IEventEmitter emitter, UserChatEvent @event)
        {
            var session = GetScriptContext(@event.User);
            if (session == null)
                return;

            @event.IsCancelled = true;

            var cmd = @event.Message;
            var result = session.Eval(cmd);

            if (result.HasReturn)
                @event.User.SendMessage("> " + result.Return, ConsoleColor.Gray);
        }
    }
}