﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Avro.Specific;
using Confluent.Kafka;
using Microsoft.Azure.WebJobs.Description;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Kafka
{
    /// <summary>
    /// Trigger attribute to start execution of function when Kafka messages are received
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public class KafkaTriggerAttribute : Attribute
    {

        public KafkaTriggerAttribute(string brokerList, string topic)
        {
            this.BrokerList = brokerList;
            this.Topic = topic;
        }

        /// <summary>
        /// Gets or sets the topic
        /// </summary>
        public string Topic { get; private set; }

        /// <summary>
        /// Gets or sets the broker list
        /// </summary>
        public string BrokerList { get; private set; }

        /// <summary>
        /// Get or sets the EventHub connection string when using Kafka protocol header feature of Azure EventHubs
        /// </summary>
        public string EventHubConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the consumer group
        /// </summary>
        public string ConsumerGroup { get; set; }

        /// <summary>
        /// Gets or sets the key element type
        /// Default is Null
        /// </summary>
        public Type KeyType { get; set; }

        Type valueType;

        /// <summary>
        /// Gets or sets the Avro data type
        /// Must implement ISpecificRecord
        /// </summary>
        public Type ValueType
        { 
            get => this.valueType;
            set
            {
                if (value != null && !IsValidValueType(value))
                {
                    throw new ArgumentException($"The value of {nameof(ValueType)} must be a byte[], string or a type that implements {nameof(ISpecificRecord)} or {nameof(Google.Protobuf.IMessage)}. The type {value.Name} does not.");
                }

                this.valueType = value;
            }
        }

        /// <summary>
        /// Gets or sets the Avro schema.
        /// Should be used only if a generic record should be generated
        /// </summary>
        public string AvroSchema { get; set; }

        /// <summary>
        /// SASL mechanism to use for authentication. 
        /// Allowed values: Gssapi, Plain, ScramSha256, ScramSha512
        /// Default: Plain
        /// 
        /// sasl.mechanism in librdkafka
        /// </summary>
        public BrokerAuthenticationMode AuthenticationMode { get; set; } = BrokerAuthenticationMode.NotSet;

        /// <summary>
        /// SASL username for use with the PLAIN and SASL-SCRAM-.. mechanisms
        /// Default: ""
        /// 
        /// 'sasl.username' in librdkafka
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// SASL password for use with the PLAIN and SASL-SCRAM-.. mechanism
        /// Default: ""
        /// 
        /// sasl.password in librdkafka
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the security protocol used to communicate with brokers
        /// Default is plain text
        /// 
        /// security.protocol in librdkafka
        /// </summary>
        public BrokerProtocol Protocol { get; set; } = BrokerProtocol.NotSet;

        /// <summary>
        /// Path to client's private key (PEM) used for authentication.
        /// Default: ""
        /// ssl.key.location in librdkafka
        /// </summary>
        public string SslKeyLocation { get; set; }


        bool IsValidValueType(Type value)
        {
            return
                typeof(ISpecificRecord).IsAssignableFrom(value) ||
                typeof(Google.Protobuf.IMessage).IsAssignableFrom(value) ||
                value == typeof(byte[]) ||
                value == typeof(string);
        }
    }
}