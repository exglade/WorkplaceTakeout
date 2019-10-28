using Facebook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WorkplaceTakeout.CustomTakeout
{
    public class Extractor
    {
        private readonly string[] _postFields = new[]
        {
            "id",
            "created_time",
            "formatting",
            "from",
            "icon",
            "link",
            "message",
            "name",
            "object_id",
            "permalink_url",
            "picture",
            "place",
            //"poll", // Invalid
            "properties",
            "status_type",
            "story",
            "to",
            "type",
            "updated_time",
            "with_tags"
        };

        private readonly string[] _eventFields = new[]
        {
            "id",
            "attending_count",
            "cover",
            "declined_count",
            "description",
            "end_time",
            "event_times",
            "guest_list_enabled",
            "interested_count",
            "is_canceled",
            "maybe_count",
            "name",
            "owner",
            "parent_group",
            "place",
            "start_time",
            "timezone",
            "type",
            "updated_time",
        };

        private readonly string[] _groupFields = new[]
        {
            "id",
            "cover",
            "description",
            "icon",
            "is_workplace_default",
            "is_community",
            "name",
            "owner",
            "privacy",
            "updated_time",
            "archived",
            "post_requires_admin_approval",
            "purpose",
            "post_permissions",
            "join_setting"
        };

        private readonly string[] _memberFields = new[]
        {
            "id",
            "first_name",
            "last_name",
            "email",
            "title",
            "organization",
            "division",
            "department",
            "primary_phone",
            "primary_address",
            "picture",
            "link",
            "locale",
            "name",
            "name_format",
            "updated_time",
            "account_invite_time",
            "account_claim_time",
            "external_id",
            "start_date",
            "about",
            "cost_center",
            "claim_link",
            "access_code",
            "work_locale"
        };

        private readonly string[] _groupFileFields = new[]
        {
            "download_link",
            //"from", // deprecated
            "group",
            "id",
            "message",
            "updated_time"
        };

        private readonly string[] _docFields = new[]
        {
            "id",
            "can_delete",
            "can_edit",
            "created_time",
            "from",
            "icon",
            "link",
            "message",
            "revision",
            "subject",
            "updated_time"
        };

        private readonly string[] _albumFields = new[]
        {
            "id",
            "can_upload",
            "count",
            "cover_photo",
            "created_time",
            "description",
            "event",
            "from",
            "link",
            "location",
            "name",
            "place",
            "privacy",
            "type",
            "updated_time",
        };

        private readonly string[] _albumPhotoFields = new[]
        {
            "source",
            "url",
            "message",
            "place",
            "no_story"
        };

        private readonly string[] _commentFields = new[]
        {
            "id",
            "attachment",
            "can_comment",
            "can_remove",
            "can_hide",
            "can_like",
            "can_reply_privately",
            "comment_count",
            "created_time",
            "from",
            "like_count",
            "message",
            "message_tags",
            "object",
            "parent",
            "private_reply_conversation",
            "user_likes",
        };

        private readonly string[] _storyAttachmentFields = new[]
        {
            "description",
            "description_tags",
            "media",
            "media_type",
            "target",
            "title",
            "type",
            "unshimmed_url",
            "url"
        };

        private readonly string _fakeUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.120 Safari/537.36";

        private readonly string _outputFolderPath;
        private readonly FacebookClient _client;

        public Extractor(FacebookClient client, string outputFolderPath)
        {
            _outputFolderPath = outputFolderPath;
            _client = client;
        }

        public async Task RunAsync()
        {
            // Goals:
            // - Retrieve all members' basic information
            // - Retrieve all groups' basic information
            // - (Group) Retrieve group's albums, docs, feed, files information (others optional, not important)
            // - (Group) Download albums, docs, feed's details, files
            // - Retrieve all events' basic information (optional, not actively used)

            // Fetch all Members

            GetAllMembers();

            // Fetch all /community/groups

            await GetAllGroupsAsync();

            // Fetch all /community/events

            await GetAllEventsAsync();
        }

        private async Task GetAllEventsAsync()
        {
            Console.Write($"GET /community/events...");

            var eventFieldsStr = string.Join(",", _eventFields);
            var eventsResponses = GetAndSaveAllPaginatedData(
                path: $"/community/events?fields={eventFieldsStr}",
                filePathWithIndex: Path.Combine(_outputFolderPath, @"community\events_{0}.json"));

            Console.WriteLine("DONE");

            var eventsDefinition = new
            {
                Data = new[] {
                    new {
                        Id = "",
                        Name = "",
                        Cover = new { Source = "" }
                    }
                }
            };
            var postFieldsStr = string.Join(",", _postFields);

            foreach (var response in eventsResponses)
            {
                var events = Helper.DeserializeAnonymousType(response, eventsDefinition);
                if (events?.Data?.Length > 0)
                {
                    foreach (var @event in events.Data)
                    {
                        var relativeEventFilePath = $"community\\events\\{@event.Id}";
                        Console.Write($"GET Event {@event.Id}:{@event.Name}...");

                        // Save event cover image.

                        var eventFolderPath = Path.Combine(_outputFolderPath, relativeEventFilePath);
                        var coverUrl = @event.Cover?.Source;
                        if (!string.IsNullOrEmpty(coverUrl))
                            await DownloadFile(coverUrl, eventFolderPath, "cover");

                        // Get feed

                        var feedPath = $"/{@event.Id}/feed?fields={postFieldsStr}";
                        GetAndSaveAllPaginatedData(feedPath, Path.Combine(eventFolderPath, "feed_{0}.json"));

                        Console.WriteLine("DONE");
                    }
                }
            }
        }

        private async Task GetAllGroupsAsync()
        {
            Console.Write($"GET /community/groups...");

            var groupFieldsStr = string.Join(",", _groupFields);
            var groupsResponses = GetAndSaveAllPaginatedData(
                path: $"/community/groups?fields={groupFieldsStr}",
                filePathWithIndex: Path.Combine(_outputFolderPath, @"community\groups_{0}.json"));

            Console.WriteLine("DONE");

            var groupsDefinition = new
            {
                Data = new[]
                {
                    new {
                        Id = "",
                        Name = "",
                        Cover = new { Source = "" },
                        Icon = ""
                    }
                }
            };
            var docFieldsStr = string.Join(",", _docFields);

            foreach (var response in groupsResponses)
            {
                var groups = Helper.DeserializeAnonymousType(response, groupsDefinition);
                if (groups?.Data?.Length > 0)
                {
                    foreach (var group in groups.Data)
                    {
                        var relativeGroupFilePath = $"community\\groups\\{group.Id}";
                        Console.Write($"GET Group {group.Id}:{group.Name}...");

                        var groupFolderPath = Path.Combine(_outputFolderPath, relativeGroupFilePath);

                        // Save group cover image.

                        var coverUrl = group.Cover?.Source;
                        if (!string.IsNullOrEmpty(coverUrl))
                            await DownloadFile(coverUrl, groupFolderPath, "cover");

                        // Save group icon image.

                        var iconUrl = group.Cover?.Source;
                        if (!string.IsNullOrEmpty(iconUrl))
                            await DownloadFile(iconUrl, groupFolderPath, "icon");

                        /*
                         * Edges: (when member is deactivated in company, they are gone from group as member)
                         * /admins - no use at this point
                         * /albums : /{album-id}/photos
                         * /docs
                         * /events - event id is enough, /community/events has all
                         * /feed
                         * /files
                         * /member_requests - no use at this point
                         * /members - no use at this point
                         * /moderators - no use at this point
                         * /pinned_posts - should exists in /feed too
                         * /groups - skip, not used
                         */

                        // Get albums

                        GetGroupAlbumPhotos(group.Id, groupFolderPath);

                        // Get docs

                        GetAndSaveAllPaginatedData(
                            path: $"/{group.Id}/docs?fields={docFieldsStr}",
                            filePathWithIndex: Path.Combine(groupFolderPath, "docs_{0}.json"));

                        // Get events

                        GetAndSaveAllPaginatedData(
                            path: $"/{group.Id}/events",
                            filePathWithIndex: Path.Combine(groupFolderPath, "events_{0}.json"));

                        // Get feed

                        await GetGroupFeed(group.Id, groupFolderPath);

                        // Get files

                        await GetGroupFilesAsync(group.Id, groupFolderPath);

                        Console.WriteLine("DONE");
                    }
                }
            }
        }

        private async Task GetGroupFeed(string groupId, string groupFolderPath)
        {
            var feedDefinition = new
            {
                Data = new[]
                {
                    new {
                        Id = "",
                        Picture = ""
                    }
                }
            };
            var postFieldsStr = string.Join(",", _postFields);
            var commentFieldsStr = string.Join(",", _commentFields);
            var storyAttachmentFieldsStr = string.Join(",", _storyAttachmentFields);

            var feedResponses = GetAndSaveAllPaginatedData(
                path: $"/{groupId}/feed?fields={postFieldsStr}",
                filePathWithIndex: Path.Combine(groupFolderPath, "feed_{0}.json"));

            /*
             * Edges:
             * /attachments
             * /comments
             * /likes - no use at this point
             * /reactions - no use at this point
             * /seen - no use at this point
             */

            foreach (var feedResponse in feedResponses)
            {
                var posts = Helper.DeserializeAnonymousType(feedResponse, feedDefinition);
                if (posts?.Data?.Length > 0)
                {
                    foreach (var post in posts.Data)
                    {
                        var postFolderPath = Path.Combine(groupFolderPath, $"posts\\{post.Id}");

                        // Get post's picture

                        if (!string.IsNullOrEmpty(post.Picture))
                            await DownloadFile(post.Picture, postFolderPath, "picture", doFakeUserAgent: true);

                        // Get attachments

                        GetAndSaveAllPaginatedData(
                            path: $"/{post.Id}/attachments?fields={storyAttachmentFieldsStr}",
                            filePathWithIndex: Path.Combine(postFolderPath, "attachments_{0}.json"));

                        // Get comments

                        GetAndSaveAllPaginatedData(
                            path: $"/{post.Id}/comments?fields={commentFieldsStr}",
                            filePathWithIndex: Path.Combine(postFolderPath, "comments_{0}.json"));
                    }
                }
            }
        }

        private void GetGroupAlbumPhotos(string groupId, string groupFolderPath)
        {
            var albumsDefinition = new
            {
                Data = new[]
                {
                    new
                    {
                        Id = ""
                    }
                }
            };
            var albumFieldsStr = string.Join(",", _albumFields);
            var albumPhotoFieldsStr = string.Join(",", _albumPhotoFields);

            var albumsResponses = GetAndSaveAllPaginatedData(
                path: $"/{groupId}/albums?fields={albumFieldsStr}",
                filePathWithIndex: Path.Combine(groupFolderPath, "albums_{0}.json"));
            foreach (var albumsResponse in albumsResponses)
            {
                var albums = Helper.DeserializeAnonymousType(albumsResponse, albumsDefinition);
                if (albums?.Data?.Length > 0)
                {
                    foreach (var album in albums.Data)
                    {
                        GetAndSaveAllPaginatedData(
                            path: $"/{album.Id}/photos?fields={albumPhotoFieldsStr}",
                            filePathWithIndex: Path.Combine(groupFolderPath, $"albums\\{album.Id}", "photos_{0}.json"));

                        // TODO: Save photos?
                    }
                }
            }
        }

        private async Task GetGroupFilesAsync(string groupId, string groupFolderPath)
        {
            var filesDefinition = new
            {
                Data = new[]
                {
                    new {
                        Id = "",
                        DownloadLink = ""
                    }
                }
            };
            var groupFileFieldsStr = string.Join(",", _groupFileFields);

            var filesResponses = GetAndSaveAllPaginatedData(
                            path: $"/{groupId}/files?fields={groupFileFieldsStr}",
                            filePathWithIndex: Path.Combine(groupFolderPath, "files_{0}.json"));
            foreach (var filesResponse in filesResponses)
            {
                var files = Helper.DeserializeAnonymousType(filesResponse, filesDefinition);
                if (files?.Data?.Length > 0)
                {
                    foreach (var file in files.Data)
                    {
                        if (!string.IsNullOrEmpty(file.DownloadLink))
                            await DownloadFile(file.DownloadLink, Path.Combine(groupFolderPath, "files"), $"{file.Id}", doFakeUserAgent: true);
                    }
                }
            }
        }

        private void GetAllMembers()
        {
            var memberFieldsStr = string.Join(",", _memberFields);

            Console.Write($"GET /community/former_members...");

            GetAndSaveAllPaginatedData(
                path: $"/community/former_members?fields={memberFieldsStr}",
                filePathWithIndex: Path.Combine(_outputFolderPath, @"community\former_members_{0}.json"));

            Console.WriteLine("DONE");

            Console.Write($"GET /community/members...");

            GetAndSaveAllPaginatedData(
                path: $"/community/members?fields={memberFieldsStr}",
                filePathWithIndex: Path.Combine(_outputFolderPath, @"community\members_{0}.json"));

            Console.WriteLine("DONE");
        }

        private List<object> GetAndSaveAllPaginatedData(string path, string filePathWithIndex)
        {
            var paginationDefinition = new { Paging = new { Previous = "", Next = "" } };

            var allResults = new List<object>();
            for (int i = 0; !string.IsNullOrEmpty(path); i++)
            {
                // Fetch data.

                var result = _client.Get(path);
                allResults.Add(result);

                // Save data as JSON to file.

                var filepath = string.Format(filePathWithIndex, i);
                Helper.WriteObjectAsJsonToFile(filepath, result);

                // Get next page.

                var node = Helper.DeserializeAnonymousType(result, paginationDefinition);
                path = node?.Paging?.Next;
            }

            return allResults;
        }

        private async Task DownloadFile(string url, string folderPath, string newFileNameWithoutExtension, bool doFakeUserAgent = false)
        {
            using (var httpClient = new HttpClient())
            {
                // Fake user agent when server expect latest browser.

                if(doFakeUserAgent)
                {
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_fakeUserAgent);
                }

                // Get HTTP response.

                using (var response = await httpClient.GetAsync(url))
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();

                        var filename = response.Content.Headers.ContentDisposition?.FileName;
                        if (string.IsNullOrEmpty(filename))
                            filename = Helper.GetFileNameFromUrl(url);

                        var stream = await response.Content.ReadAsStreamAsync();

                        var newFilename = string.IsNullOrEmpty(newFileNameWithoutExtension)
                            ? filename
                            : newFileNameWithoutExtension + Path.GetExtension(filename);
                        var destinationFile = Path.Combine(folderPath, newFilename);

                        Helper.WriteToFile(destinationFile, stream);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR Downloading {url}\n{ex.Message}");
                    }
                }
            }
            
        }
    }
}