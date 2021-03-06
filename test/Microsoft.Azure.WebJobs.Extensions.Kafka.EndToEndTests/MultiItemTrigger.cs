﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Kafka.EndToEndTests
{
    internal static class MultiItemTrigger
    {
        public static void Trigger(
            [KafkaTrigger("LocalBroker", Constants.StringTopicWithOnePartitionName, ConsumerGroup = nameof(MultiItemTrigger), ValueType = typeof(string))] KafkaEventData[] kafkaEvents,
            ILogger log)
        {
            foreach (var kafkaEvent in kafkaEvents)
            {
                log.LogInformation(kafkaEvent.Value.ToString());
            }
        }
    }

    internal static class MultiItemByteTrigger
    {
        public static void Trigger(
            [KafkaTrigger("LocalBroker", Constants.StringTopicWithOnePartitionName, ConsumerGroup = nameof(MultiItemTrigger))] byte[][] kafkaEvents,
            ILogger log)
        {
            foreach (var kafkaEvent in kafkaEvents)
            {
                log.LogInformation($@"Byte data received. Length: {kafkaEvent.Length}, Content: ""{Encoding.UTF8.GetString(kafkaEvent)}""");
            }
        }
    }

    internal static class SingleItemByteTrigger
    {
        public static void Trigger(
            [KafkaTrigger("LocalBroker", Constants.StringTopicWithOnePartitionName, ConsumerGroup = nameof(SingleItemByteTrigger))] byte[] kafkaEvent,
            ILogger log)
        {
            log.LogInformation($@"Byte data received. Length: {kafkaEvent.Length}, Content: ""{Encoding.UTF8.GetString(kafkaEvent)}""");
        }
    }
}
