namespace ParcelRegistry.Importer.Grb.Infrastructure.Download
{
    using System;
    using System.IO.Compression;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public interface IDownloadFacade
    {
        public Task<ZipArchive> Download(DateTimeOffset fromDate, DateTimeOffset toDate);
        Task<DateTime> GetMaxDate();
    }

    public sealed class DownloadFacade : IDownloadFacade
    {
        private readonly DownloadClient _downloadClient;
        private readonly ILogger<DownloadFacade> _logger;

        public DownloadFacade(DownloadClient downloadClient, ILoggerFactory loggerFactory)
        {
            _downloadClient = downloadClient;
            _logger = loggerFactory.CreateLogger<DownloadFacade>();
        }

        public async Task<ZipArchive> Download(DateTimeOffset fromDate, DateTimeOffset toDate)
        {
            var orderResponse = await _downloadClient.Order(fromDate.DateTime, toDate.DateTime);

            var status = OrderStatus.Received;
            while (status is OrderStatus.Received or OrderStatus.Processing)
            {
                _logger.LogInformation("Waiting for order {orderId} to be processed...", orderResponse);
                status = await _downloadClient.GetOrderStatus(orderResponse);

                if (status == OrderStatus.Unknown)
                    throw new InvalidOperationException("Order status is unknown.");

                await Task.Delay(5000);
            }

            return await _downloadClient.GetOrderArchive(orderResponse);
        }

        public async Task<DateTime> GetMaxDate()
        {
            return await _downloadClient.GetMaxEndDate();
        }
    }
}
