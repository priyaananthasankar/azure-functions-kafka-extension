// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Avro.Generic;
using Avro.Specific;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Kafka
{
    /// <summary>
    /// Provider for <see cref="IKafkaProducer"/>
    /// Those matching the broker, key type and value type are shared
    /// </summary>
    public class KafkaProducerProvider : IKafkaProducerProvider
    {
        private readonly IConfiguration config;
        private readonly INameResolver nameResolver;
        private readonly IKafkaTopicFactory kafkaTopicFactory;
        private readonly ILogger logger;
        ConcurrentDictionary<string, IKafkaProducer> producers = new ConcurrentDictionary<string, IKafkaProducer>();

        public KafkaProducerProvider(
            IConfiguration config,
            INameResolver nameResolver,
            IKafkaTopicFactory kafkaTopicFactory,
            ILoggerProvider loggerProvider)
        {
            this.config = config;
            this.nameResolver = nameResolver;
            this.kafkaTopicFactory = kafkaTopicFactory ?? throw new ArgumentNullException(nameof(kafkaTopicFactory));
            this.logger = loggerProvider.CreateLogger(LogCategories.CreateTriggerCategory("Kafka"));
        }

        public IKafkaProducer Get(KafkaAttribute attribute)
        {
            var resolvedBrokerList = this.nameResolver.ResolveWholeString(attribute.BrokerList);
            var brokerListFromConfig = this.config.GetConnectionStringOrSetting(resolvedBrokerList);
            if (!string.IsNullOrEmpty(brokerListFromConfig))
            {
                resolvedBrokerList = brokerListFromConfig;
            }

            if (string.IsNullOrWhiteSpace(resolvedBrokerList))
            {
                throw new InvalidOperationException("Broker not provided for KafkaAttribute");
            }

            if (attribute.CreateTopicIfNotExists)
            {
                this.CreateTopicIfNotExistsAsync(attribute, resolvedBrokerList)
                    .GetAwaiter()
                    .GetResult();
            }

            var keyTypeName = attribute.KeyType == null ? string.Empty : attribute.KeyType.AssemblyQualifiedName;
            var valueTypeName = attribute.ValueType == null ? string.Empty : attribute.ValueType.AssemblyQualifiedName;
            var producerKey = $"{resolvedBrokerList}:keyTypeName:valueTypeName";

            return producers.GetOrAdd(producerKey, (k) => this.Create(attribute, resolvedBrokerList));
        }

        /// <summary>
        /// Creates the topic if it does not exist
        /// </summary>
        /// <returns>True if the topic was created, otherwise False</returns>
        private async Task<bool> CreateTopicIfNotExistsAsync(KafkaAttribute attribute, string brokerList)
        {

            var topic = this.nameResolver.ResolveWholeString(attribute.Topic);
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new InvalidOperationException($"Cannot set {nameof(attribute.CreateTopicIfNotExists)} and not provide a topic name");
            }

            return await this.kafkaTopicFactory.CreateIfNotExistsAsync(brokerList, topic, attribute.TopicPartitionCount, attribute.TopicReplicationFactor);
        }

        private IKafkaProducer Create(KafkaAttribute attribute, string brokerList)
        {
            Type keyType = attribute.KeyType ?? typeof(Null);
            Type valueType = attribute.ValueType;
            string avroSchema = null;
            if (valueType == null)
            {
                if (!string.IsNullOrEmpty(attribute.AvroSchema))
                {
                    avroSchema = attribute.AvroSchema;
                    valueType = typeof(GenericRecord);
                }
                else
                {
                    valueType = typeof(string);
                }
            }
            else
            {
                if (typeof(ISpecificRecord).IsAssignableFrom(valueType))
                {
                    var specificRecord = (ISpecificRecord)Activator.CreateInstance(valueType);
                    avroSchema = specificRecord.Schema.ToString();
                }
            }

            return (IKafkaProducer)Activator.CreateInstance(
                typeof(KafkaProducer<,>).MakeGenericType(keyType, valueType),
                this.GetProducerConfig(brokerList),
                avroSchema,
                this.logger
                );
        }

        private ProducerConfig GetProducerConfig(string brokerList)
        {
            return new ProducerConfig()
            {
                BootstrapServers = brokerList,
            };
        }
    }
}