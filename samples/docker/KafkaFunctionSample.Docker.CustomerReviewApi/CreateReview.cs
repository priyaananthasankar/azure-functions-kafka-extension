using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using KafkaFunctionSample.Docker.Common;
using Microsoft.Azure.WebJobs.Extensions.Kafka;

namespace KafkaFunctionSample.Docker.CustomerReviewApi
{
    public static class CreateReview
    {
        [FunctionName("CreateReview")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reviews")] HttpRequest req,
            [Kafka("Brokers", "customerreviews", KeyType = typeof(string), ValueType = typeof(CustomerReview))] IAsyncCollector<KafkaEventData> reviews,
            ILogger log)
        {
            log.LogInformation("Received request to create review");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var customerReview = JsonConvert.DeserializeObject<CustomerReview>(requestBody);

            // TODO: add validation
            var kafkaEvent = new KafkaEventData()
            {
                Key = customerReview.ProductID,
                Value = customerReview,
            };

            await reviews.AddAsync(kafkaEvent);
            await reviews.FlushAsync();

            log.LogInformation("Review created: {review}", JsonConvert.SerializeObject(customerReview));

            return new OkResult();
        }
    }
}
