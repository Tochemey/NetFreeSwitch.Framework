using System;

namespace Networking.Common.Logging {
    /// <summary>
    ///     Factory which produces loggers when requested.
    /// </summary>
    public interface ILogProvider {
        /// <summary>
        ///     Create a new logger.
        /// </summary>
        /// <param name="typeThatWantToLog">Type which want's to write to the log file</param>
        /// <returns>Logger to use (must not be <c>null</c>).</returns>
        ILogger GetLogger(Type typeThatWantToLog);
    }
}