namespace Rocket.Scripting
{
    public enum ScriptExecutionResult
    {
        /// <summary>
        /// The script has been executed with no errors.
        /// </summary>
        Success,
        /// <summary>
        /// The script has thrown exceptions on load. The <see cref="ScriptResult.Exception"/> property should not be null.
        /// </summary>
        FailedException,
        /// <summary>
        /// The script failed to execute, but no exceptions occured.
        /// </summary>
        FailedMisc,
        /// <summary>
        /// The script has failed to load (e.g. missing file permissions).
        /// </summary>
        LoadFailed,
        /// <summary>
        /// The script file was not found.
        /// </summary>
        FileNotFound,
        /// <summary>
        /// The entrypoint of the script was not found.
        /// </summary>
        EntrypointNotFound
    }
}