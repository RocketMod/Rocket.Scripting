using System;
using Jurassic;
using Rocket.API.Commands;
using Rocket.Core.User;

namespace Rocket.Scripting.JavaScript
{
    public class WrappedJavaScriptException : Exception, ICommandFriendlyException
    {
        public WrappedJavaScriptException(JavaScriptException ex) : base(ex.Message)
        {

        }

        public void SendErrorMessage(ICommandContext context)
        {
            context.User.SendMessage("> " + Message, ConsoleColor.Red);
        }
    }
}