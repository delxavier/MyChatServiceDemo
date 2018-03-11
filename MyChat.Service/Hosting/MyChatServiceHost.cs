// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyChatServiceHost.cs">
//    Copyright (c) 2018. All Rights reserved.
// </copyright>
// <summary>
//    The custom host class for chat service.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MyChat.Service.Hosting
{
    using System;
    using System.ServiceModel;
    using MyChat.Service.Logging;

    /// <summary>
    /// The custom host class for chat service.
    /// </summary>
    internal sealed class MyChatServiceHost : ServiceHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MyChatServiceHost"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> instance.</param>
        /// <param name="baseAddresses">The base addresses.</param>
        public MyChatServiceHost(ILogger logger, params Uri[] baseAddresses)
            : base(serviceType: typeof(MyChatService), baseAddresses: baseAddresses)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(paramName: nameof(logger));
            }

            var instanceProvider = new MyChatServiceInstanceProvider(logger: logger);
            foreach (var cd in this.ImplementedContracts.Values)
            {
                cd.Behaviors.Add(item: instanceProvider);
            }
        }
    }
}
