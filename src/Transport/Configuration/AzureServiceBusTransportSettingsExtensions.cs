namespace NServiceBus
{
    using System;
    using Configuration.AdvancedExtensibility;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Primitives;
    using Transport.AzureServiceBus;

    /// <summary>
    /// Adds access to the Azure Service Bus transport config to the global Transport object.
    /// </summary>
    public static class AzureServiceBusTransportSettingsExtensions
    {
        /// <summary>
        /// Overrides the default topic name used to publish events between endpoints.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="topicName">The name of the topic used to publish events between endpoints.</param>
        public static TransportExtensions<AzureServiceBusTransport> TopicName(this TransportExtensions<AzureServiceBusTransport> transportExtensions, string topicName)
        {
            Guard.AgainstNullAndEmpty(nameof(topicName), topicName);

            transportExtensions.GetSettings().Set(SettingsKeys.TopicName, topicName);

            return transportExtensions;
        }

        /// <summary>
        /// Overrides the default maximum size used when creating queues and topics.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="maximumSizeInGB">The maximum size to use, in gigabytes.</param>
        public static TransportExtensions<AzureServiceBusTransport> EntityMaximumSize(this TransportExtensions<AzureServiceBusTransport> transportExtensions, int maximumSizeInGB)
        {
            Guard.AgainstNegativeAndZero(nameof(maximumSizeInGB), maximumSizeInGB);

            transportExtensions.GetSettings().Set(SettingsKeys.MaximumSizeInGB, maximumSizeInGB);

            return transportExtensions;
        }

        /// <summary>
        /// Enables entity partitioning when creating queues and topics.
        /// </summary>
        public static TransportExtensions<AzureServiceBusTransport> EnablePartitioning(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            transportExtensions.GetSettings().Set(SettingsKeys.EnablePartitioning, true);

            return transportExtensions;
        }

        /// <summary>
        /// Specifies the multiplier to apply to the maximum concurrency value to calculate the prefetch count.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="prefetchMultiplier">The multiplier value to use in the prefetch calculation.</param>
        public static TransportExtensions<AzureServiceBusTransport> PrefetchMultiplier(this TransportExtensions<AzureServiceBusTransport> transportExtensions, int prefetchMultiplier)
        {
            Guard.AgainstNegativeAndZero(nameof(prefetchMultiplier), prefetchMultiplier);

            transportExtensions.GetSettings().Set(SettingsKeys.PrefetchMultiplier, prefetchMultiplier);

            return transportExtensions;
        }

        /// <summary>
        /// Overrides the default prefetch count calculation with the specified value.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="prefetchCount">The prefetch count to use.</param>
        public static TransportExtensions<AzureServiceBusTransport> PrefetchCount(this TransportExtensions<AzureServiceBusTransport> transportExtensions, int prefetchCount)
        {
            Guard.AgainstNegative(nameof(prefetchCount), prefetchCount);

            transportExtensions.GetSettings().Set(SettingsKeys.PrefetchCount, prefetchCount);

            return transportExtensions;
        }

        /// <summary>
        /// Overrides the default time to wait before triggering a circuit breaker that initiates the endpoint shutdown procedure when the message pump cannot successfully receive a message.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="timeToWait">The time to wait before triggering the circuit breaker.</param>
        public static TransportExtensions<AzureServiceBusTransport> TimeToWaitBeforeTriggeringCircuitBreaker(this TransportExtensions<AzureServiceBusTransport> transportExtensions, TimeSpan timeToWait)
        {
            Guard.AgainstNegativeAndZero(nameof(timeToWait), timeToWait);

            transportExtensions.GetSettings().Set(SettingsKeys.TimeToWaitBeforeTriggeringCircuitBreaker, timeToWait);

            return transportExtensions;
        }

        /// <summary>
        /// Specifies a callback to apply to the subscription to alter it's default name from endpoint name
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="subscriptionNameSanitizer">The callback to apply.</param>
        /// <returns></returns>
        public static TransportExtensions<AzureServiceBusTransport> SubscriptionNameSanitizer(this TransportExtensions<AzureServiceBusTransport> transportExtensions, Func<string, string> subscriptionNameSanitizer)
        {
            Guard.AgainstNull(nameof(subscriptionNameSanitizer), subscriptionNameSanitizer);

            string WrappedSubscriptionNameSanitizer(string subscriptionName)
            {
	            try
	            {
		            return subscriptionNameSanitizer(subscriptionName);
	            }
	            catch (Exception exception)
	            {
		            throw new Exception("Custom subscription name sanitizer threw an exception.", exception);
	            }
            }

            transportExtensions.GetSettings().Set(SettingsKeys.SubscriptionNameSanitizer,
	            (Func<string, string>)WrappedSubscriptionNameSanitizer);

            return transportExtensions;
        }

        /// <summary>
        /// Specifies a callback to apply to a subscription rule name when a subscribed event's name is longer than 50 characters.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="ruleNameSanitizer">The callback to apply.</param>
        /// <returns></returns>
        public static TransportExtensions<AzureServiceBusTransport> RuleNameSanitizer(this TransportExtensions<AzureServiceBusTransport> transportExtensions, Func<string, string> ruleNameSanitizer)
        {
            Guard.AgainstNull(nameof(ruleNameSanitizer), ruleNameSanitizer);

            string WrappedRuleNameSanitizer(string ruleName)
            {
	            try
	            {
		            return ruleNameSanitizer(ruleName);
	            }
	            catch (Exception exception)
	            {
		            throw new Exception("Custom rule name sanitizer threw an exception.", exception);
	            }
            }

            transportExtensions.GetSettings().Set(SettingsKeys.RuleNameSanitizer, (Func<string, string>)WrappedRuleNameSanitizer);

            return transportExtensions;
        }

        /// <summary>
        /// Configures the transport to use AMQP over WebSockets.
        /// </summary>
        /// <param name="transportExtensions"></param>
        public static TransportExtensions<AzureServiceBusTransport> UseWebSockets(this TransportExtensions<AzureServiceBusTransport> transportExtensions)
        {
            transportExtensions.GetSettings().Set(SettingsKeys.TransportType, TransportType.AmqpWebSockets);

            return transportExtensions;
        }

        /// <summary>
        /// Overrides the default token provider with a custom implementation.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="tokenProvider">The token provider to be used.</param>
        public static TransportExtensions<AzureServiceBusTransport> CustomTokenProvider(this TransportExtensions<AzureServiceBusTransport> transportExtensions, ITokenProvider tokenProvider)
        {
            transportExtensions.GetSettings().Set(SettingsKeys.CustomTokenProvider, tokenProvider);

            return transportExtensions;
        }
    }
}