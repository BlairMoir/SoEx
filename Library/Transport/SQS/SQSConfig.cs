namespace SoEx.Transport.SQS
{
    public class SQSConfig
    {
        /// <summary>
        /// The URL of the SQS queue.
        /// </summary>
        /// <remarks>
        /// Queue URLs and names are case-sensitive.
        /// </remarks>
        public required string QueueUrl { get; init; }

        /// <summary>
        /// Gets or sets the service URL for Amazon SQS operations.
        /// </summary>
        /// <remarks>
        /// Use this property to override the default AWS SQS endpoint.
        /// Commonly used for local development with SQS emulators (LocalStack, ElasticMQ)
        /// or to specify custom VPC endpoints.
        /// If not set, the SDK automatically determines the endpoint based on the Region.
        /// </remarks>
        public string? ServiceURL { get; init; }

        /// <summary>
        /// Gets or sets the maximum number of messages to retrieve in a single receive operation.
        /// </summary>
        /// <remarks>
        /// Valid values are 1-10. Amazon SQS never returns more messages than this value,
        /// but may return fewer. This affects throughput - higher values can improve performance
        /// for high-volume queues but may increase processing latency for individual messages.
        /// Default is 10 (maximum allowed by AWS SQS).
        /// </remarks>
        public int MaxNumberOfMessages { get; init; } = 10;

        /// <summary>
        /// Gets or sets the duration (in seconds) for which the receive call waits for a message to arrive.
        /// </summary>
        /// <remarks>
        /// This enables long polling, which reduces empty receives and lowers costs.
        /// Valid values are 0-20 seconds. A value of 0 uses short polling (returns immediately).
        /// Long polling (1-20 seconds) is recommended for most use cases as it:
        /// - Reduces the number of empty responses
        /// - Eliminates false empty responses
        /// - Reduces Amazon SQS usage costs
        /// Default is 20 seconds (maximum, recommended for best efficiency).
        /// </remarks>
        public int WaitTimeSeconds { get; init; } = 20;
    }
}
