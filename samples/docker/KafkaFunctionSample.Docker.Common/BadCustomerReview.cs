using Avro;
using Avro.Specific;
using Newtonsoft.Json;

namespace KafkaFunctionSample.Docker.Common
{
    /// <summary>
    /// Defines a bad customer review avro record
    /// </summary>
    public class BadCustomerReview : ISpecificRecord
    {
       public const string SchemaText = @"
       {
  ""type"": ""record"",
  ""name"": ""BadCustomerReview"",
  ""namespace"": ""KafkaFunctionSample.Docker.Common"",
  ""fields"": [
    {
      ""name"": ""timestamp"",
      ""type"": ""long""
    },
    {
      ""name"": ""productid"",
      ""type"": ""string""
    },
    {
      ""name"": ""text"",
      ""type"": ""string""
    },
    {
      ""name"": ""ipaddress"",
      ""type"": ""string""
    },
    {
      ""name"": ""score"",
      ""type"": ""double""
    },
    {
      ""name"": ""country"",
      ""type"": ""string""
    }
  ]
}";
        public static Schema _SCHEMA = Schema.Parse(SchemaText);

        [JsonIgnore]
        public virtual Schema Schema => _SCHEMA;

        public long Timestamp { get; set; }

        public string ProductID { get; set; }

        public string Text { get; set; }

        public string IpAddress { get; set; }

        public double Score { get; set; }

        public string Country { get; set; }

        public virtual object Get(int fieldPos)
        {
            switch (fieldPos)
            {
                case 0: return this.Timestamp;
                case 1: return this.ProductID;
                case 2: return this.Text;
                case 3: return this.IpAddress;
                case 4: return this.Score;
                case 5: return this.Country;
                default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Get()");
            };
        }
        public virtual void Put(int fieldPos, object fieldValue)
        {
            switch (fieldPos)
            {
                case 0: this.Timestamp = (long)fieldValue; break;
                case 1: this.ProductID = (string)fieldValue; break;
                case 2: this.Text = (string)fieldValue; break;
                case 3: this.IpAddress = (string)fieldValue; break;
                case 4: this.Score = (double)fieldValue; break;
                case 5: this.Country = (string)fieldValue; break;
                default: throw new AvroRuntimeException("Bad index " + fieldPos + " in Put()");
            };
        }
        
    }
}