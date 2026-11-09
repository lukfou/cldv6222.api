using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLDV6212POE1.Api.Controllers;

[ApiController]
[Route("api")]
[AllowAnonymous]
public class BackendProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public BackendProxyController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("menu")]
    public Task ProxyGetMenu()
        => ForwardAsync(HttpMethod.Get, "api/menu");

    [HttpPost("menu")]
    public Task ProxyCreateMenuItem()
        => ForwardAsync(HttpMethod.Post, "api/menu");

    [HttpGet("menu/category/{category}")]
    public Task ProxyGetMenuByCategory(string category)
        => ForwardAsync(
            HttpMethod.Get,
            $"api/menu/category/{Uri.EscapeDataString(category)}");

    [HttpPut("menu/{category}/{id}")]
    public Task ProxyUpdateMenuItem(
        string category,
        string id)
        => ForwardAsync(
            HttpMethod.Put,
            $"api/menu/{Uri.EscapeDataString(category)}/{Uri.EscapeDataString(id)}");

    [HttpDelete("menu/{category}/{id}")]
    public Task ProxyDeleteMenuItem(
        string category,
        string id)
        => ForwardAsync(
            HttpMethod.Delete,
            $"api/menu/{Uri.EscapeDataString(category)}/{Uri.EscapeDataString(id)}");

    [HttpPost("documents/upload")]
    public Task ProxyUploadDocument()
        => ForwardAsync(
            HttpMethod.Post,
            "api/documents/upload");

    [HttpGet("documents")]
    public Task ProxyListDocuments()
        => ForwardAsync(
            HttpMethod.Get,
            "api/documents");

    [HttpGet("documents/download/{fileName}")]
    public Task ProxyDownloadDocument(string fileName)
        => ForwardAsync(
            HttpMethod.Get,
            $"api/documents/download/{Uri.EscapeDataString(fileName)}");

    [HttpPost("orders/queue")]
    public Task ProxyQueueOrder()
        => ForwardAsync(
            HttpMethod.Post,
            "api/orders/queue");

    [HttpPost("orders/status")]
    public Task ProxyUpdateOrderStatus()
        => ForwardAsync(
            HttpMethod.Post,
            "api/orders/status");

    private async Task ForwardAsync(
        HttpMethod method,
        string backendPath)
    {
        string baseUrl =
            _configuration["Backend:FunctionsBaseUrl"]
            ?? throw new InvalidOperationException(
                "Backend Functions URL is missing.");

        if (!Uri.TryCreate(
                baseUrl,
                UriKind.Absolute,
                out Uri? baseUri))
        {
            throw new InvalidOperationException(
                "Backend Functions URL is invalid.");
        }

        if (baseUri.Host.Equals(
                Request.Host.Host,
                StringComparison.OrdinalIgnoreCase) &&
            baseUri.Port == Request.Host.Port)
        {
            throw new InvalidOperationException(
                "Backend Functions URL must point to the Functions app, not this API.");
        }

        Uri destination = new Uri(
            baseUri,
            backendPath + Request.QueryString);

        using var message = new HttpRequestMessage(
            method,
            destination);

        bool hasBody =
            Request.ContentLength is > 0 ||
            Request.Headers.ContainsKey("Transfer-Encoding");

        if (hasBody)
        {
            // Buffer the body so the backend receives a fixed Content-Length
            // instead of a request streamed with transfer-encoding: chunked.
            using var requestBodyStream = new MemoryStream();
            await Request.Body.CopyToAsync(
                requestBodyStream,
                HttpContext.RequestAborted);

            byte[] requestBody = requestBodyStream.ToArray();

            message.Content = new ByteArrayContent(requestBody);

            if (!string.IsNullOrWhiteSpace(Request.ContentType))
            {
                message.Content.Headers.ContentType =
                    MediaTypeHeaderValue.Parse(Request.ContentType);
            }
        }

        foreach (var header in Request.Headers)
        {
            if (header.Key.Equals(
                    "Host",
                    StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals(
                    "Content-Type",
                    StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals(
                    "Content-Length",
                    StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals(
                    "Transfer-Encoding",
                    StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals(
                    "Connection",
                    StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals(
                    "Accept-Encoding",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!message.Headers.TryAddWithoutValidation(
                    header.Key,
                    header.Value.ToArray()))
            {
                message.Content?.Headers
                    .TryAddWithoutValidation(
                        header.Key,
                        header.Value.ToArray());
            }
        }

        var client = _httpClientFactory.CreateClient();

        HttpResponseMessage httpResponseMessage = await client.SendAsync(
                message,
                HttpCompletionOption.ResponseContentRead,
                HttpContext.RequestAborted);
        using HttpResponseMessage backendResponse =
            httpResponseMessage;

        Response.StatusCode =
            (int)backendResponse.StatusCode;

        foreach (var header in backendResponse.Headers)
        {
            if (!IsHopByHopHeader(header.Key))
            {
                Response.Headers[header.Key] =
                    header.Value.ToArray();
            }
        }

        foreach (var header in backendResponse.Content.Headers)
        {
            if (!IsHopByHopHeader(header.Key) &&
                !header.Key.Equals(
                    "Content-Length",
                    StringComparison.OrdinalIgnoreCase))
            {
                Response.Headers[header.Key] =
                    header.Value.ToArray();
            }
        }

        Response.Headers.Remove("transfer-encoding");

        await backendResponse.Content.CopyToAsync(
            Response.Body,
            HttpContext.RequestAborted);
    }

    private static bool IsHopByHopHeader(string headerName)
        => headerName.Equals("Connection", StringComparison.OrdinalIgnoreCase) ||
           headerName.Equals("Keep-Alive", StringComparison.OrdinalIgnoreCase) ||
           headerName.Equals("Proxy-Authenticate", StringComparison.OrdinalIgnoreCase) ||
           headerName.Equals("Proxy-Authorization", StringComparison.OrdinalIgnoreCase) ||
           headerName.Equals("TE", StringComparison.OrdinalIgnoreCase) ||
           headerName.Equals("Trailer", StringComparison.OrdinalIgnoreCase) ||
           headerName.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase) ||
           headerName.Equals("Upgrade", StringComparison.OrdinalIgnoreCase);
}