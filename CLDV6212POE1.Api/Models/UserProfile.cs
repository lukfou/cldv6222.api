using Azure;
using Azure.Data.Tables;

namespace CLDV6212POE1.Api.Models;

public class UserProfile : ITableEntity
{
    public string PartitionKey { get; set; } = "";

    public string RowKey { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public string FullName { get; set; } = "";

    public DateTimeOffset CreatedDate { get; set; }

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }
}