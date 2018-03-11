//-----------------------------------------------------------------------
// <copyright file="MyChatServiceHostFactory.cs"
//     Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
// Customize instantiation of WCF service. 
// This is required to manage log initialization and cleanup when service is hosted by a third party (WAS, IIS, ...)
// </summary>
//-----------------------------------------------------------------------

namespace MyChat.Service.Hosting
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using MyChat.Service.Logging;

    /// <summary>
    /// Customize instantiation of WCF service. 
    /// This is required to manage log initialization and cleanup when service is hosted by a third party (WAS, IIS, ...)
    /// </summary>
    public sealed class MyChatServiceHostFactory : ServiceHostFactory
    {
        /// <summary> The <see cref="ILogger"/> instance. </summary>
        private readonly ILogger logger = new OutputLogger();

        /// <summary>
        /// Creates a ServiceHostBase with a specific base address using custom initiation data.
        /// </summary>
        /// <param name="constructorString">The initialization data that is passed to the <see cref="ServiceHostBase"/> instance being constructed by the factory.</param>
        /// <param name="baseAddresses">An <see cref="Array"/> of type <see cref="Uri"/> that contains the base addresses of the host.</param>
        /// <returns>The <see cref="ServiceHostBase"/> object with the specified base addresses and initialized with the custom initiation data.</returns>
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            var service = new MyChatServiceHost(logger: this.logger, baseAddresses: baseAddresses);
            service.Opening += (source, eventArgs) => this.logger.Write("Service opening...");
            service.Opened += (source, eventArgs) => this.logger.Write("Service opened...");
            service.Closing += (source, eventArgs) => this.logger.Write("Service cleaned...");
            service.Closed += (source, eventArgs) => this.logger.Write("Service closed...");
            return service;
        }
    }
}
