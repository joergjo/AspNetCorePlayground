using System;
using System.Collections.Generic;

namespace AzureCDN.Models
{
    public class MediaItemViewModel
    {
        public MediaItemViewModel(List<MediaItem> mediaItems)
        {
            MediaItems = mediaItems ?? throw new ArgumentNullException(nameof(mediaItems));
        }

        public List<MediaItem> MediaItems { get; private set; }
    }

    public class MediaItem
    {
        public Uri Uri { get; set; }

        public string ContentType { get; set; }
    }
}
