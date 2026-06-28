using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
var app = builder.Build();

app.UseStaticFiles();

app.MapGet("/", () => Results.Redirect("/index.html"));

async Task<string> GetPayPalAccessToken(IConfiguration config, HttpClient httpClient)
{
    var clientId = config["PayPal:ClientId"];
    var clientSecret = config["PayPal:ClientSecret"];
    var baseUrl = config["PayPal:BaseUrl"];

    var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

    var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/oauth2/token");
    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
    request.Content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("grant_type", "client_credentials")
    });

    var response = await httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
    using var json = JsonDocument.Parse(content);
    return json.RootElement.GetProperty("access_token").GetString()!;
}

app.MapPost("/api/orders", async (IConfiguration config, IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient();
    var accessToken = await GetPayPalAccessToken(config, httpClient);
    var baseUrl = config["PayPal:BaseUrl"];

    var orderRequest = new
    {
        intent = "CAPTURE",
        purchase_units = new[]
        {
            new
            {
                amount = new
                {
                    currency_code = "USD",
                    value = "100.00"
                }
            }
        }
    };

    var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    request.Content = new StringContent(JsonSerializer.Serialize(orderRequest), Encoding.UTF8, "application/json");

    var response = await httpClient.SendAsync(request);
    var content = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem(content, statusCode: (int)response.StatusCode);
    }

    return Results.Content(content, "application/json");
});

app.MapPost("/api/orders/{orderId}/capture", async (string orderId, IConfiguration config, IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient();
    var accessToken = await GetPayPalAccessToken(config, httpClient);
    var baseUrl = config["PayPal:BaseUrl"];

    var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v2/checkout/orders/{orderId}/capture");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    request.Content = new StringContent("", Encoding.UTF8, "application/json");

    var response = await httpClient.SendAsync(request);
    var content = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem(content, statusCode: (int)response.StatusCode);
    }

    return Results.Content(content, "application/json");
});

app.Run();
