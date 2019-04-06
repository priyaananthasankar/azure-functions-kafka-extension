// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Kafka
{
    /// <summary>
    /// Kafka topic factory
    /// </summary>
    public class KafkaTopicFactory : IKafkaTopicFactory
    {
        private readonly ILogger logger;

        public KafkaTopicFactory(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger(LogCategories.CreateTriggerCategory("Kafka"));
        }

        /// <summary>
        /// Creates a topic if it does not exists
        /// </summary>
        public async Task<bool> CreateIfNotExistsAsync(string brokerList, string topic, int partitionCount, short replicationFactor)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException("Topic was not provided", nameof(topic));
            }

            if (partitionCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(partitionCount), "Partition count must be greater than zero");
            }

            if (replicationFactor <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(replicationFactor), "Replication factor must be greater than zero");
            }


            var adminClient = new AdminClientBuilder(new ClientConfig()
            {
                BootstrapServers = brokerList
            }).Build();

            var topicSpecification = new TopicSpecification()
            {
                Name = topic,
                NumPartitions = partitionCount,
                ReplicationFactor = replicationFactor,
            };

            try
            {
                await adminClient.CreateTopicsAsync(new[] { topicSpecification });

                this.logger.LogInformation(
                    "{topic} created in {brokerList}. Partitions: {partitionCount}, ReplicationFactor: {replicationFactor} ",
                    topicSpecification.Name,
                    brokerList,
                    topicSpecification.NumPartitions,
                    topicSpecification.ReplicationFactor);

                return true;
            }
            catch (CreateTopicsException createTopicsException)
            {
                if (!createTopicsException.Results.All(x => x.Error.Code == ErrorCode.TopicAlreadyExists))
                {
                    throw;
                }
            }
            catch (KafkaException ex)
            {
                if (!ex.Error.Reason.Equals("No topics to create", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw;
                }
            }

            return false;
        }
    }
}
