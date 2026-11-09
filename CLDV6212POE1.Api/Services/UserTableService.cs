using Azure;
using Azure.Data.Tables;
using CLDV6212POE1.Api.Models;

namespace CLDV6212POE1.Api.Services;

public class UserTableService
{
    private readonly TableClient _tableClient;

    public UserTableService(IConfiguration configuration)
    {
        string connectionString =
            configuration["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException(
                "Azure Storage connection string is missing.");

        string tableName =
            configuration["AzureStorage:UsersTableName"]
            ?? "Users";

        _tableClient = new TableClient(
            connectionString,
            tableName);
    }

    public async Task InitialiseAsync()
    {
        await _tableClient.CreateIfNotExistsAsync();
    }

    public async Task<UserProfile?> GetUserByEmailAsync(
        string email)
    {
        string normalisedEmail = email.Trim().ToLowerInvariant();

        string filter = TableClient.CreateQueryFilter(
            $"RowKey eq {normalisedEmail}");

        await foreach (
            UserProfile user in
            _tableClient.QueryAsync<UserProfile>(
                filter: filter))
        {
            return user;
        }

        return null;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        UserProfile? user =
            await GetUserByEmailAsync(email);

        return user != null;
    }

    public async Task CreateUserAsync(UserProfile user)
    {
        try
        {
            await _tableClient.AddEntityAsync(user);
        }
        catch (RequestFailedException ex)
            when (ex.Status == 409)
        {
            throw new InvalidOperationException(
                "A user with this email address already exists.",
                ex);
        }
    }
}