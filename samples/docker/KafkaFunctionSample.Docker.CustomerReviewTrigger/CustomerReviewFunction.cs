using System;
using System.Threading.Tasks;
using KafkaFunctionSample.Docker.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Kafka;

namespace KafkaFunctionDockerSample
{
    public static class CustomerReviewFunction
    {
        [FunctionName(nameof(CustomerReviewTrigger))]
        public static async Task CustomerReviewTrigger(
            [KafkaTrigger("Brokers", "customerreviews", ConsumerGroup=nameof(CustomerReviewTrigger), KeyType = typeof(string), ValueType = typeof(CustomerReview))] KafkaEventData[] events,
            [Kafka("Brokers", "badcustomerreviews", KeyType=typeof(string), ValueType = typeof(BadCustomerReview))] IAsyncCollector<KafkaEventData> badReviews
        )
        {
            foreach (var customerReviewEvent in events)
            {
                var customerReview = (CustomerReview)customerReviewEvent.Value;
                var score = await GetScoreAsync(customerReview.Text);
                if (score < 4.0)
                {
                    var badReview = new BadCustomerReview()
                    {
                        ProductID = customerReview.ProductID,
                        Country = await ResolveCountryFromIpAddressAsync(customerReview.IpAddress),
                        Score = score,
                        Text = customerReview.Text,
                        Timestamp = customerReview.Timestamp,
                        IpAddress = customerReview.IpAddress,
                    };

                    await badReviews.AddAsync(new KafkaEventData()
                    {
                        Key = badReview.ProductID,
                        Value = badReview,
                    });
                }
            }
        }

        private static Task<string> ResolveCountryFromIpAddressAsync(string ipAddress)
        {
            // TODO: make actual look up
            return Task.FromResult("USA");
        }

        private static Task<double> GetScoreAsync(string text)
        {
            // TODO: use sentiment analysis
            var score = 10.0;
            if (text.Contains("bad", StringComparison.InvariantCultureIgnoreCase))
            {
                score = 3.7;
            }

            return Task.FromResult(score);
        }
    }
}