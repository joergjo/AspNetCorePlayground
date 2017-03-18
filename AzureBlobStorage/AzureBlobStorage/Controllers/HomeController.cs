using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureBlobStorage.Controllers
{
    public class HomeController : Controller
    {
        private readonly CloudBlobContainer _container;
        private readonly ILogger _logger;

        public HomeController(CloudBlobContainer container, ILogger<HomeController> logger)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ViewResult Index() => View();

        public ViewResult About() => View();

        public ViewResult Upload()
        {
            ViewBag.ExpiryDate = DateTime.UtcNow.AddMonths(1);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Upload(IFormFile inputFile, DateTime expiryDate)
        {
            if (inputFile == null || inputFile.Length == 0)
            {
                ModelState.AddModelError(nameof(inputFile), "Please specify a file to upload");
            }
            if (expiryDate < DateTime.UtcNow)
            {
                ModelState.AddModelError(nameof(expiryDate), "Please specify a future date");
            }

            if (ModelState.IsValid)
            {
                _logger.LogDebug("Received file '{0}'.", inputFile.FileName);

                var blob = default(CloudBlockBlob);
                using (var inputStream = inputFile.OpenReadStream())
                {
                    blob = await UploadToBlobStorageAsync(inputStream, inputFile.FileName, inputFile.ContentType);
                    _logger.LogDebug("Wrote uploaded file to block blob '{0}'.", blob.Uri);
                }

                string sasToken = CreateSharedAccessSignature(blob, expiryDate);
                TempData["SharedAccessUri"] = blob.Uri + sasToken;
                _logger.LogTrace("Created SAS token '{0}'.", sasToken);

                return RedirectToAction(nameof(Upload));
            }
            else
            {
                ViewBag.ExpiryDate = expiryDate;
                return View();
            }
        }

        public IActionResult Error() => View();

        private async Task<CloudBlockBlob> UploadToBlobStorageAsync(Stream inputStream, string name, string contentType)
        {
            var blob = _container.GetBlockBlobReference(name);
            blob.Properties.ContentType = contentType;
            await blob.UploadFromStreamAsync(inputStream);
            return blob;
        }

        private string CreateSharedAccessSignature(CloudBlockBlob blob, DateTime expiryDate)
        {
            var policy = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = expiryDate,
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write
            };
            return blob.GetSharedAccessSignature(policy);
        }
    }
}
