﻿namespace NServiceBus.Transport.AzureServiceBus.CommandLine
{
    using System;
    using System.Threading.Tasks;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Management;

    static class Endpoint
    {
        public static async Task Create(ManagementClient client, CommandArgument name, CommandOption topicName, CommandOption subscriptionName, CommandOption<int> size, CommandOption partitioning)
        {
            try
            {
                await Queue.Create(client, name, size, partitioning);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                Console.WriteLine("Queue already exists, skipping creation");
            }

            try
            {
                await Topic.Create(client, topicName, size, partitioning);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                Console.WriteLine("Topic already exists, skipping creation");
            }

            try
            {
                await Subscription.Create(client, name, topicName, subscriptionName);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                Console.WriteLine("Subscription already exists, skipping creation");
            }
        }

        public static async Task Subscribe(ManagementClient client, CommandArgument name, CommandOption topicName, CommandOption subscriptionName, CommandArgument eventType, CommandOption ruleName)
        {
            try
            {
                await Rule.Create(client, name, topicName, subscriptionName, eventType, ruleName);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                Console.WriteLine($"Rule already exists, skipping creation. Verify SQL filter matches '[NServiceBus.EnclosedMessageTypes] LIKE '%{eventType.Value}%'.");
            }
        }

        public static async Task Unsubscribe(ManagementClient client, CommandArgument name, CommandOption topicName, CommandOption subscriptionName, CommandArgument eventType, CommandOption ruleName)
        {
            try
            {
                await Rule.Delete(client, name, topicName, subscriptionName, eventType, ruleName);
            }
            catch (MessagingEntityNotFoundException)
            {
                Console.WriteLine("Rule does not exist, skipping deletion");
            }
        }
    }
}