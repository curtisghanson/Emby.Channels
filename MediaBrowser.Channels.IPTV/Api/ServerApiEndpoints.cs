﻿using MediaBrowser.Channels.IPTV.Configuration;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.MediaInfo;
using ServiceStack;
using System;
using System.Linq;

namespace MediaBrowser.Channels.IPTV.Api
{
    [Route("/Iptv/Bookmarks", "POST", Summary = "Bookmarks a video")]
    public class VideoSend : IReturnVoid
    {
        [ApiMember(Name = "Name", Description = "Name", IsRequired = false, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string Name { get; set; }

        [ApiMember(Name = "Path", Description = "Path", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string Path { get; set; }

		[ApiMember(Name = "UserId", Description = "UserId", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
		public string UserId { get; set; }

        [ApiMember(Name = "Protocol", Description = "Protocol", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public MediaProtocol Protocol { get; set; }

        [ApiMember(Name = "ParentalRating", Description = "ParentalRating", IsRequired = true, DataType = "int", ParameterType = "path", Verb = "POST")]
        public int ParentalRating { get; set; }

        /*
        [ApiMember(Name = "Tags", Description = "Tags", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string Tags { get; set; }
        */

        [ApiMember(Name = "ImagePath", Description = "ImagePath", IsRequired = false, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string ImagePath { get; set; }
    }

    class ServerApiEndpoints : IRestfulService
    {
        public void Post(VideoSend request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Name cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(request.Path))
            {
                throw new ArgumentException("Path cannot be empty.");
            }
			if (string.IsNullOrWhiteSpace(request.UserId))
			{
				throw new ArgumentException("UserId cannot be empty.");
			}

            var list = Plugin.Instance.Configuration.Bookmarks.ToList();

            list.Add(new Bookmark
            {
				UserId = request.UserId,
                Name = request.Name,
                Image = request.ImagePath,
                Path = request.Path,
                ParentalRating = request.ParentalRating,
                /* Tags = request.Tags, */
                Protocol = request.Protocol
            });

            Plugin.Instance.Configuration.Bookmarks = list.ToArray();

            Plugin.Instance.SaveConfiguration();
        }
    }
}
