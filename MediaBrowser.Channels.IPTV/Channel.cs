using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Channels.IPTV
{
    class Channel : IChannel, IHasCacheKey
    {
        private readonly ILogger _logger;
        private readonly IUserManager _user;

        public Channel(IUserManager userManager, ILogManager logManager)
        {
			_user = userManager;
			_logger = logManager.GetLogger(GetType().Name);
        }

        public string DataVersion
        {
            get
            {
                // Increment as needed to invalidate all caches
                return "1";
            }
        }

        public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            _logger.Debug("cat ID : " + query.FolderId);

            return await GetChannelItemsInternal(query.UserId, cancellationToken).ConfigureAwait(false);
        }


        private async Task<ChannelItemResult> GetChannelItemsInternal(string userId, CancellationToken cancellationToken)
        {
            var user = string.IsNullOrEmpty(userId) ? null : _user.GetUserById(userId);
            var items = new List<ChannelItemInfo>();

            foreach (var s in Plugin.Instance.Configuration.Bookmarks)
            {
                // Until we have user configuration in the UI, we have to disable this.
                //if (!string.Equals(s.UserId, userId, StringComparison.OrdinalIgnoreCase))
                //{
                //    continue;
                //}
                if (
                    (user.Policy.MaxParentalRating.HasValue && user.Policy.MaxParentalRating >= s.ParentalRating)
                    || (!user.Policy.MaxParentalRating.HasValue)
                )
                {
                    var item = new ChannelItemInfo
					{
						Name = s.Name,
						ImageUrl = s.Image,
						Id = s.Name,
						Type = ChannelItemType.Media,
						ContentType = ChannelMediaContentType.Clip,
						MediaType = ChannelMediaType.Video,

						MediaSources = new List<ChannelMediaInfo>
						{
							new ChannelMediaInfo
							{
								Path = s.Path,
								Protocol = s.Protocol
							}  
						}
					};

					items.Add(item);
				}
			}

            return new ChannelItemResult
            {
                Items = items.ToList()
            };
        }

        public IEnumerable<ImageType> GetSupportedChannelImages()
        {
            return new List<ImageType>
            {
                ImageType.Thumb,
                ImageType.Backdrop
            };
        }

        public string HomePageUrl
        {
            get { return ""; }
        }

        public string Name
        {
            get { return "Video Bookmarks"; }
        }

        public InternalChannelFeatures GetChannelFeatures()
        {
            return new InternalChannelFeatures
            {
                ContentTypes = new List<ChannelMediaContentType>
                {
                    ChannelMediaContentType.Clip
                },

                MediaTypes = new List<ChannelMediaType>
                {
                    ChannelMediaType.Video
                },

                SupportsContentDownloading = true
            };
        }

        public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
        {
            switch (type)
            {
                case ImageType.Thumb:
                case ImageType.Backdrop:
                    {
                        var path = GetType().Namespace + ".Images." + type.ToString().ToLower() + ".png";

                        return Task.FromResult(new DynamicImageResponse
                        {
                            Format = ImageFormat.Png,
                            HasImage = true,
                            Stream = GetType().Assembly.GetManifestResourceStream(path)
                        });
                    }
                default:
                    throw new ArgumentException("Unsupported image type: " + type);
            }
        }

        public bool IsEnabledFor(string userId)
        {
            return true;
        }

        public ChannelParentalRating ParentalRating
        {
            get { return ChannelParentalRating.GeneralAudience; }
        }

        public string GetCacheKey(string userId)
        {
            return Guid.NewGuid().ToString("N");
        }

        public string Description
        {
            get { return string.Empty; }
        }

    }
}
