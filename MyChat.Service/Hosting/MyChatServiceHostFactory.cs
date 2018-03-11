//-----------------------------------------------------------------------
// <copyright file="MyChatServiceHostFactory.cs">
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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

        /// <summary> The list of object to dispose. </summary>
        private readonly SynchronizedCollection<IDisposable> objectsToDispose = new SynchronizedCollection<IDisposable>();

        /// <summary> Disposed field. </summary>
        private volatile bool isDisposed;

        /// <summary>
        /// Creates a ServiceHostBase with a specific base address using custom initiation data.
        /// </summary>
        /// <param name="constructorString">The initialization data that is passed to the <see cref="ServiceHostBase"/> instance being constructed by the factory.</param>
        /// <param name="baseAddresses">An <see cref="Array"/> of type <see cref="Uri"/> that contains the base addresses of the host.</param>
        /// <returns>The <see cref="ServiceHostBase"/> object with the specified base addresses and initialized with the custom initiation data.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposition is done during web service unload")]
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            var service = new MyChatServiceHost(logger: this.logger, hostFactory: this, baseAddresses: baseAddresses);
            service.Opening += (source, eventArgs) => this.logger.Write("Service opening...");
            service.Opened += (source, eventArgs) => this.logger.Write("Service opened...");
            service.Closing += (source, eventArgs) => this.CleanUp();
            service.Closed += (source, eventArgs) => this.logger.Write("Service closed...");
            return service;
        }

        /// <summary>
        /// Objects created in the service should be registered here in case of unhandled exception in service code
        /// because stop or shutdown code won't be called in that case.
        /// </summary>
        /// <param name="object">The object to register for disposition.</param>
        /// <remarks>
        /// The contract allows multiple calls to disposition code without error.
        /// This is required since normal service termination will call disposition in stop or shutdown events, 
        /// and again when leaving the main function as process exits.
        /// </remarks>
        public void RegisterToDisposition(IDisposable @object)
        {
            this.objectsToDispose.Add(@object);
        }

        /// <summary>
        /// Objects created in the service should be removed from global disposition.
        /// </summary>
        /// <param name="object">The object to remove from disposition.</param>
        public void RemoveToDisposition(IDisposable @object)
        {
            this.objectsToDispose.Remove(@object);
        }

        /// <summary>
        /// Cleans factory instance.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Don't make fail general disposition code on on object disposition")]
        private void CleanUp()
        {
            if (!this.isDisposed)
            {
                this.logger.Format("Disposing {0}", this.GetType().Name);

                List<IDisposable> copyOfObjectsToDispose;
                lock (this.objectsToDispose.SyncRoot)
                {
                    copyOfObjectsToDispose = new List<IDisposable>(this.objectsToDispose);
                }

                copyOfObjectsToDispose.Reverse();
                foreach (IDisposable @object in copyOfObjectsToDispose)
                {
                    try
                    {
                        if (@object != null)
                        {
                            @object.Dispose();
                            this.logger.Format(SeverityLevel.Debug, "Object instance {0} has been disposed", @object.GetType().Name);
                        }
                    }
                    catch (Exception exception)
                    {
                        this.logger.WriteException(exception, SeverityLevel.Error, "Unexpected error while disposing object on exit");
                    }
                }
                
                this.isDisposed = true;
            }
        }
    }
}
