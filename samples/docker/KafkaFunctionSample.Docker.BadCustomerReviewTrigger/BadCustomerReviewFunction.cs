using System;
using System.Threading.Tasks;
using KafkaFunctionSample.Docker.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Kafka;
using Microsoft.Extensions.Logging;

namespace KafkaFunctionDockerSample
{
    public static class BadCustomerReviewFunction
    {
        [FunctionName(nameof(BadCustomerReviewTrigger))]
        public static async Task BadCustomerReviewTrigger(
            [KafkaTrigger("Brokers", "badcustomerreviews", ConsumerGroup=nameof(BadCustomerReviewTrigger), KeyType = typeof(string), ValueType = typeof(BadCustomerReview))] KafkaEventData[] events,
            [CosmosDB(
                databaseName: "%CosmosDbName%",
                collectionName: "BadReviews",
                ConnectionStringSetting = "CosmosDBConnection")] IAsyncCollector<BadCustomerReview> badReviews,
            ILogger logger
        )
        {
            foreach (var anEvent in events)
            {
                var review = (BadCustomerReview)anEvent.Value;
                await badReviews.AddAsync(review);

                logger.LogInformation("Review for {product} with {score} sent to cosmos", review.ProductID, review.Score);
            }
        }
    }
}