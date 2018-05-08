using System;

namespace Rocket.Scripting
{
    /// <summary>
    /// The result of a script execution
    /// </summary>
    public class ScriptResult
    {
        public ScriptResult(ScriptExecutionResult result)
        {
            ExecutionResult = result;
        }

        /// <summary>
        /// See <see cref="ScriptExecutionResult"/>.
        /// </summary>
        public ScriptExecutionResult ExecutionResult { get; }

        /// <summary>
        /// Return value of the script
        /// </summary>
        public object Return { get; set; }

        /// <summary>
        /// True if the script returned a value
        /// </summary>
        public bool HasReturn { get; set; }

        /// <summary>
        /// Exception thrown by the script
        /// </summary>
        public Exception Exception { get; set; }
    }
}