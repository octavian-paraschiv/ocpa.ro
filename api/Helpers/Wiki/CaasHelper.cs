using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ocpa.ro.api.Helpers.Wiki
{
    public interface ICaasHelper
    {
        Task<byte[]> GetNewCat(string resourcePath, string queryString);
    }

    public class CaasHelper : BaseHelper, ICaasHelper, IDisposable
    {
        private const string CatCacheKey = "cat";
        private const string DefaultCatImagePath = "Helpers/Wiki/loadcat.gif";

        private readonly HttpClient _client;
        private readonly IDistributedCache _cache;
        private readonly ManualResetEventSlim _terminateBackgroundProcess = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _backgroundProcessRunning = new ManualResetEventSlim(false);

        private string _resourcePath;
        private string _queryString;
        private bool disposedValue;
        private readonly SemaphoreSlim _paramsSemaphore = new SemaphoreSlim(1);

        public CaasHelper(IWebHostEnvironment hostingEnvironment, ILogger logger,
            IHttpClientFactory factory, IDistributedCache cache)
            : base(hostingEnvironment, logger)
        {
            _client = factory.CreateClient("caas");
            _cache = cache;
        }

        public async Task<byte[]> GetNewCat(string resourcePath, string queryString)
        {
            byte[] data = null;

            try
            {
                data = await _cache.GetAsync(CatCacheKey);

                if (!(data?.Length > 0))
                    data = await File.ReadAllBytesAsync(DefaultCatImagePath);

                await _paramsSemaphore.WaitAsync();

                _resourcePath = resourcePath;
                _queryString = queryString;

                if (!_backgroundProcessRunning.Wait(0))
                    StartBackgroundRecacheProcess();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            finally
            {
                _paramsSemaphore.Release();
            }

            return data;
        }

        private void StartBackgroundRecacheProcess()
        {
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                _backgroundProcessRunning.Set();

                while (!_terminateBackgroundProcess.Wait(0))
                {
                    try
                    {
                        string resourcePath = string.Empty;
                        string queryString = string.Empty;

                        await _paramsSemaphore.WaitAsync();
                        resourcePath = _resourcePath;
                        queryString = _queryString;
                        _paramsSemaphore.Release();

                        var data = await _client.GetByteArrayAsync($"{resourcePath}{queryString}");
                        if (data?.Length > 0)
                            await _cache.SetAsync(CatCacheKey, data);
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }

                    Thread.Sleep(30000);
                }
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    _terminateBackgroundProcess.Set();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

