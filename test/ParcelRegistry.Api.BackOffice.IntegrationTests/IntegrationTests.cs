namespace ParcelRegistry.Api.BackOffice.IntegrationTests
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class IntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;

        public IntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("/v2/percelen/MM1-456/acties/adreskoppelen", "dv_ar_adres_beheer")]
        [InlineData("/v2/percelen/MM1-456/acties/adresontkoppelen", "dv_ar_adres_beheer")]
        public async Task ReturnsSuccess(string endpoint, string requiredScopes)
        {
            var client = _fixture.TestServer.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(requiredScopes));

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Theory]
        [InlineData("/v2/percelen/MM1-456/acties/adreskoppelen")]
        [InlineData("/v2/percelen/MM1-456/acties/adresontkoppelen")]
        public async Task ReturnsUnauthorized(string endpoint)
        {
            var client = _fixture.TestServer.CreateClient();

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("/v2/percelen/MM1-456/acties/adreskoppelen", "dv_ar_adres_uitzonderingen")]
        [InlineData("/v2/percelen/MM1-456/acties/adresontkoppelen", "dv_ar_adres_uitzonderingen")]
        public async Task ReturnsForbidden(string endpoint, string badscope)
        {
            var client = _fixture.TestServer.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", await _fixture.GetAccessToken(badscope));

            var response = await client.PostAsync(endpoint,
                new StringContent("{}", Encoding.UTF8, "application/json"), CancellationToken.None);
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
