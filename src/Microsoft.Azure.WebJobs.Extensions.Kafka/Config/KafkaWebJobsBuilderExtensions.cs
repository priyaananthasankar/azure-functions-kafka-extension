// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Kafka
{
    public static class KafkaWebJobsBuilderExtensions
    {
        /// <summary>
        /// Adds the Kafka extensions to the provider <see cref="IWebJobsBuilder"/>
        /// </summary>
        public static IWebJobsBuilder AddKafka(this IWebJobsBuilder builder) => AddKafka(builder, o => { });

        /// <summary>
        /// Adds the Kafka extensions to the provider <see cref="IWebJobsBuilder"/>
        /// </summary>
        public static IWebJobsBuilder AddKafka(this IWebJobsBuilder builder, Action<KafkaOptions> configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddExtension<KafkaExtensionConfigProvider>()
                .BindOptions<KafkaOptions>();

            builder.Services.Configure<KafkaOptions>(options =>
            {
                configure(options);
            });

            builder.Services.AddSingleton<IKafkaTopicFactory, KafkaTopicFactory>();
            builder.Services.AddSingleton<IKafkaProducerProvider, KafkaProducerProvider>();

            var librdKafkaPath = Environment.GetEnvironmentVariable("PATH_TO_LIBRDKAFKA");
            if (!string.IsNullOrWhiteSpace(librdKafkaPath))
            {
                try
                {
                    Console.WriteLine($"Will load librdkafka from {librdKafkaPath}");
                    Confluent.Kafka.Library.Load(librdKafkaPath);
                    Console.WriteLine($"librdkafka loaded from {librdKafkaPath}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to load librdkafka from {librdKafkaPath}");
                    Console.Error.WriteLine(ex.ToString());
                }
            }

            return builder;
        }
    }
}