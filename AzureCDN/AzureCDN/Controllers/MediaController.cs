using AzureCDN.Common;
using AzureCDN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureCDN.Controllers
{
    public class MediaController : Controller
    {
        private readonly CloudBlobClient _client;
        private readonly IOptions<MediaSource> _options;

        public MediaController(CloudBlobClient client, IOptions<MediaSource> options)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<IActionResult> Index(bool skipImages = false)
        {
            var blobs = await ListBlobsInMediaContainer();
            var mediaItems = blobs.Select(x => 
                new MediaItem { Uri = x.Uri, ContentType = x.Properties.ContentType });
            var viewModel = CreateViewModel(mediaItems, skipImages);

            return View(viewModel);
        }
        // GET: /<controller>/
        public async Task<IActionResult> Cached(bool skipImages = false)
        {
            var blobs = await ListBlobsInMediaContainer();
            var mediaItems = blobs.Select(x => 
                new MediaItem { Uri = new Uri(_options.Value.CdnEndpoint, x.Name), ContentType = x.Properties.ContentType });
            var viewModel = CreateViewModel(mediaItems, skipImages);

            return View(nameof(Index), viewModel);
        }

        private MediaItemViewModel CreateViewModel(IEnumerable<MediaItem> mediaItems, bool skipImages)
        {
            if (skipImages)
            {
                mediaItems = mediaItems.Where(x => x.ContentType.StartsWith("video"));
            }

            return new MediaItemViewModel(mediaItems.ToList());
        }

        private async Task<List<ICloudBlob>> ListBlobsInMediaContainer(string prefix = "")
        {
            var container = _client.GetContainerReference(_options.Value.ContainerName);
            var blobs = new List<ICloudBlob>();
            var continuationToken = default(BlobContinuationToken);
            var resultSegment = default(BlobResultSegment);

            do
            {
                resultSegment = await container.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.Metadata, 16, continuationToken, null, null);
                blobs.AddRange(resultSegment.Results.OfType<ICloudBlob>());
                continuationToken = resultSegment.ContinuationToken;
            }
            while (continuationToken != null);

            return blobs;
        }
    }
}
