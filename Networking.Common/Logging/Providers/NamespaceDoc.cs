﻿using System.Runtime.CompilerServices;

namespace Networking.Common.Logging.Providers {
    /// <summary>
    ///     Classes which are used to deliver logs to the facade.
    /// </summary>
    /// <example>
    ///     <code>
    /// // Create loggers
    /// var consoleLogger = new ConsoleLogger() { Filter = new OnlyExceptions() };
    /// var systemDebugLogger = new SystemDebugLogger() { Filter = LogLevelFilter { MinLevel = LogLevel.Info } };
    /// 
    /// // Create the provider
    /// var logProvider = new LogProvider();
    /// logProvider.Add(consoleLogger);
    /// 
    /// // can filter on namespaces.
    /// logProvider.Add(systemDebugLogger, new NamespaceFilter("MyApp.Core"));
    /// </code>
    /// </example>
    [CompilerGenerated]
    public class NamespaceDoc {}
}