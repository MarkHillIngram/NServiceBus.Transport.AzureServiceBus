﻿namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Extensibility;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Management;
    using Microsoft.Azure.ServiceBus.Primitives;

    class SubscriptionManager : IManageSubscriptions
    {
        

        readonly string topicPath;
        readonly ServiceBusConnectionStringBuilder connectionStringBuilder;
        readonly ITokenProvider tokenProvider;
        readonly NamespacePermissions namespacePermissions;
        readonly Func<string, string> ruleNameSanitizer;
        readonly string subscriptionName;

        StartupCheckResult startupCheckResult;

        public SubscriptionManager(string inputQueueName, string topicPath, ServiceBusConnectionStringBuilder connectionStringBuilder, ITokenProvider tokenProvider, NamespacePermissions namespacePermissions, Func<string, string> subscriptionNameSanitizer, Func<string, string> ruleNameSanitizer)
        {
            this.topicPath = topicPath;
            this.connectionStringBuilder = connectionStringBuilder;
            this.tokenProvider = tokenProvider;
            this.namespacePermissions = namespacePermissions;
            this.ruleNameSanitizer = ruleNameSanitizer;

            subscriptionName = subscriptionNameSanitizer(inputQueueName) ;
        }

        public async Task Subscribe(Type eventType, ContextBag context)
        {
            await CheckForManagePermissions().ConfigureAwait(false);

            var ruleName = ruleNameSanitizer(eventType.FullName);
            var sqlExpression = $"[{Headers.EnclosedMessageTypes}] LIKE '%{eventType.FullName}%'";
            var rule = new RuleDescription(ruleName, new SqlFilter(sqlExpression));

            var client = new ManagementClient(connectionStringBuilder, tokenProvider);

            try
            {
                var existingRule = await client.GetRuleAsync(topicPath, subscriptionName, rule.Name).ConfigureAwait(false);

                if (existingRule.Filter.ToString() != rule.Filter.ToString())
                {
                    rule.Action = existingRule.Action;

                    await client.UpdateRuleAsync(topicPath, subscriptionName, rule).ConfigureAwait(false);
                }
            }
            catch (MessagingEntityNotFoundException)
            {
                try
                {
                    await client.CreateRuleAsync(topicPath, subscriptionName, rule).ConfigureAwait(false);
                }
                catch (MessagingEntityAlreadyExistsException)
                {
                }
            }

            await client.CloseAsync().ConfigureAwait(false);
        }

        public async Task Unsubscribe(Type eventType, ContextBag context)
        {
            await CheckForManagePermissions().ConfigureAwait(false);

            var ruleName = eventType.FullName.Length > maxNameLength ? ruleNameSanitizer(eventType.FullName) : eventType.FullName;

            var client = new ManagementClient(connectionStringBuilder, tokenProvider);

            try
            {
                await client.DeleteRuleAsync(topicPath, subscriptionName, ruleName).ConfigureAwait(false);
            }
            catch (MessagingEntityNotFoundException)
            {
            }

            await client.CloseAsync().ConfigureAwait(false);
        }

        async Task CheckForManagePermissions()
        {
            if (startupCheckResult == null)
            {
                startupCheckResult = await namespacePermissions.CanManage().ConfigureAwait(false);
            }

            if (!startupCheckResult.Succeeded)
            {
                throw new Exception(startupCheckResult.ErrorMessage);
            }
        }
    }
}