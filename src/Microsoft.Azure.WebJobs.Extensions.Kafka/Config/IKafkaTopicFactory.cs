// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Kafka
{
    /// <summary>
    /// Kafka topic factory interface
    /// </summary>
    public interface IKafkaTopicFactory
    {
        /// <summary>
        /// Creates a topic if it does not exists
        /// </summary>
        Task<bool> CreateIfNotExistsAsync(string brokerList, string topic, int partitionCount, short replicationFactor);
    }
}
