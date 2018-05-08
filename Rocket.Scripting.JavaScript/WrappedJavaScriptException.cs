using System;
using Jurassic;
using Rocket.API.Commands;

namespace Rocket.Scripting.JavaScript
{
    public class WrappedJavaScriptException : Exception, ICommandFriendlyException
    {
        public WrappedJavaScriptException(JavaScriptException ex) : base(ex.Message)
        {

        }

        public void SendErrorMessage(ICommandContext context)
        {
            context.Caller.SendMessage("> " + Message, ConsoleColor.Red);
        }
    }
}