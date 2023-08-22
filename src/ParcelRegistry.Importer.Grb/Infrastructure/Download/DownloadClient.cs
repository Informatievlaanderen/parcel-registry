namespace ParcelRegistry.Importer.Grb.Infrastructure.Download
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading.Tasks;
    using IdentityModel;
    using IdentityModel.Client;
    using Microsoft.Extensions.Options;

    public sealed class DownloadClient
    {
        private const string GrbGeneralProductId = "1";
        private const string GrbProductId = "6551";
        private const string Scopes = "download_catalogus_v2 download_orders_v2 vo_info";

        private readonly DownloadClientOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        private AccessToken? _accessToken;

        public DownloadClient(
            IOptions<DownloadClientOptions> downloadOptions,
            IHttpClientFactory httpClientFactory,
            JsonSerializerOptions? jsonSerializerOptions = null)
        {
            _options = downloadOptions.Value;
            _httpClientFactory = httpClientFactory;
            _jsonSerializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions();
        }

        public async Task<DateTime> GetMaxEndDate()
        {
            using var client = _httpClientFactory.CreateClient(nameof(DownloadClient));
            await AddAcmIdmAuthorization(client);
            var downloadEntity = await client.GetFromJsonAsync<EntityResponse>($"v2/products/{GrbGeneralProductId}", _jsonSerializerOptions);

            return downloadEntity.DifferenceFileMaxEndDate.Value;
        }

        public async Task<int> Order(DateTime fromDate, DateTime endDate)
        {
            if (fromDate.Date >= endDate.Date)
            {
                throw new OrderInvalidDateRangeException($"{nameof(endDate)} must be greater than {nameof(fromDate)}.");
            }

            using var client = _httpClientFactory.CreateClient(nameof(DownloadClient));
            await AddAcmIdmAuthorization(client);

            var order = new OrderRequest
            {
                ProductId = 6551,
                Format = "GML",
                TemporalCrop = new TemporalCrop
                {
                    From = fromDate,
                    To = endDate
                }
            };

            var response = await client.PostAsJsonAsync("v2/orders", order, _jsonSerializerOptions);
            response.EnsureSuccessStatusCode();

            var orderResponse = await response.Content.ReadFromJsonAsync<OrderResponse>();
            if (orderResponse.Members.Length > 1)
                throw new InvalidOperationException("Can't find the correct order, multiple orders found.");

            return orderResponse!.Members[0].OrderId;
        }

        public async Task<OrderStatus> GetOrderStatus(int orderId)
        {
            using var client = _httpClientFactory.CreateClient(nameof(DownloadClient));
            await AddAcmIdmAuthorization(client);

            var response = await client.GetFromJsonAsync<OrderStatusResponse>($"v2/orders/{orderId}/status", _jsonSerializerOptions);

            return response.Status;
        }

        public async Task<ZipArchive> GetOrderArchive(int orderId)
        {
            if (await GetOrderStatus(orderId) != OrderStatus.Completed)
                throw new InvalidOperationException("Can't get order archive, order has not been completed.");

            using var client = _httpClientFactory.CreateClient(nameof(DownloadClient));
            await AddAcmIdmAuthorization(client);

            var orderDetailResponse = await client.GetFromJsonAsync<OrderDetailResponse>($"v2/orders/{orderId}", _jsonSerializerOptions);

            if (orderDetailResponse!.Downloads.Length > 1)
                throw new InvalidOperationException("Can't get order archive, multiple downloads found.");

            var download = orderDetailResponse.Downloads[0];

            var stream = await client.GetStreamAsync($"v2/orders/{orderId}/download/{download.FileId}");
            return new ZipArchive(stream, ZipArchiveMode.Read, false);
        }

        private async Task AddAcmIdmAuthorization(HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                await GetAccessToken(Scopes));
        }

        private async Task<string> GetAccessToken(string requiredScopes)
        {
            if (_accessToken is not null && !_accessToken.IsExpired)
            {
                return _accessToken.Token;
            }

            var tokenClient = new TokenClient(
                _httpClientFactory.CreateClient(),
                new TokenClientOptions
                {
                    Address = _options.TokenEndpoint,
                    ClientId = _options.ClientId,
                    ClientSecret = _options.ClientSecret,
                    Parameters = new Parameters(new[] { new KeyValuePair<string, string>("scope", requiredScopes) }),
                });

            var response = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);

            _accessToken = new AccessToken(response.AccessToken!, response.ExpiresIn);

            return _accessToken.Token;
        }


        private sealed class AccessToken
        {
            private readonly DateTime _expiresAt;

            public string Token { get; }

            public bool IsExpired => _expiresAt <= DateTime.Now;

            public AccessToken(string token, int expiresIn)
            {
                _expiresAt = DateTime.Now.AddSeconds(expiresIn);
                Token = token;
            }
        }
    }
}
