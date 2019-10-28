using Facebook;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace WorkplaceTakeout
{
    /// <summary>
    /// A simple demo to fetch data from all Workplace edges.
    /// </summary>
    public static class SimpleWorkplaceGraphApiDemo
    {
        // Assign all parameters below before calling RunAllEdges.

        #region Parameters

        // Output folder path to write JSON result to.
        // It's recommended that you choose an empty folder for this to isolate the output content.

        public static string OutputFolderPath { get; set; }

        // Nodes
        // You can find these in URL in Workplace.

        public static string GroupId { get; set; }
        public static string PostId { get; set; }
        public static string MemberId { get; set; }
        public static string SkillId { get; set; }
        public static string EventId { get; set; }

        #endregion Parameters

        /// <summary>
        /// Get all edges with all node IDs filled into the string.
        /// </summary>
        /// <returns>A computed <see cref="Edges"/> object.</returns>
        public static Edges GetAllEdges() => new Edges();

        /// <summary>
        /// Run all edges.
        /// </summary>
        /// <param name="client">Facebook client to use for making request.</param>
        public static void RunAllEdges(FacebookClient client)
        {
            var edgesObj = GetAllEdges();

            // Compile edges into flat list.

            var edges = new[] {
                edgesObj.CommunityEdges,
                edgesObj.GroupEdges,
                edgesObj.PostEdges,
                edgesObj.CompanyEdges,
                edgesObj.MemberEdges,
                edgesObj.SkillEdges,
                edgesObj.EventEdges
            }.SelectMany(x => x);

            foreach (var edge in edges)
            {
                try
                {
                    Console.Write($"GET {edge}...");

                    // Request data from Facebok.

                    var result = client.Get(edge);
                    var output = JsonConvert.SerializeObject(result);

                    // Write output into file.

                    var subedges = edge.Split("/");
                    var filepath = OutputFolderPath;
                    for (int i = 0; i < subedges.Length; i++)
                    {
                        filepath = Path.Combine(filepath, subedges[i]);
                    }
                    Directory.CreateDirectory(filepath);
                    filepath = Path.Combine(filepath, "output.json");

                    File.WriteAllText(filepath, output, System.Text.Encoding.UTF8);
                    Console.WriteLine("DONE");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR\n{ex.Message}");
                }
            }
        }

        public class Edges
        {
            // Graph API
            // https://developers.facebook.com/docs/workplace/reference/graph-api

            public string[] CommunityEdges { get; set; }
            public string[] GroupEdges { get; set; }
            public string[] PostEdges { get; set; }
            public string[] CompanyEdges { get; set; }
            public string[] MemberEdges { get; set; }
            public string[] SkillEdges { get; set; }
            public string[] EventEdges { get; set; }

            public Edges()
            {
                CommunityEdges = new[]
                {
                    // Community's edges
                    // https://developers.facebook.com/docs/workplace/reference/graph-api/community

                    "/community/admins",
                    "/community/former_members",
                    "/community/groups",
                    "/community/members",
                    //"/community/accounts", // need Provision User Accounts permission.
                    "/community/events",
                    "/community/reported_content",
                };

                GroupEdges = new[]
                {
                    // Group's edges
                    // https://developers.facebook.com/docs/workplace/reference/graph-api/group

                    $"/{GroupId}",
                    $"/{GroupId}/admins",
                    $"/{GroupId}/albums",
                    $"/{GroupId}/docs",
                    $"/{GroupId}/events",
                    $"/{GroupId}/feed",
                    $"/{GroupId}/files",
                    $"/{GroupId}/member_requests",
                    $"/{GroupId}/members",
                    $"/{GroupId}/moderators",
                    $"/{GroupId}/pinned_posts",
                    $"/{GroupId}/groups",
                };

                PostEdges = new[]
                {
                    // Post's edges
                    // https://developers.facebook.com/docs/workplace/reference/graph-api/post

                    $"/{PostId}",
                    $"/{GroupId}_{PostId}/attachments",
                    $"/{PostId}/comments",
                    $"/{PostId}/likes",
                    $"/{GroupId}_{PostId}/reactions",
                    $"/{GroupId}_{PostId}/seen",
                };

                CompanyEdges = new[]
                {
                    // Company's Members
                    // https://developers.facebook.com/docs/workplace/reference/graph-api/member

                    "/company/members", // Similar to /community/members
                };

                MemberEdges = new[]
                {
                    // Member's edges
                    // https://developers.facebook.com/docs/workplace/reference/graph-api/member

                    $"/{MemberId}",
                    $"/{MemberId}/events",
                    $"/{MemberId}/feed",
                    $"/{MemberId}/conversations",
                    $"/{MemberId}/managers",
                    $"/{MemberId}/reports",
                    $"/{MemberId}/photos",
                    $"/{MemberId}/groups",
                    $"/{MemberId}/phones",
                    $"/{MemberId}/skills",
                    //$"/{memberId}/logout", // POST only
                };

                SkillEdges = new[]
                {
                    // Skill's edges
                    // https://developers.facebook.com/docs/workplace/reference/graph-api/skill

                    $"/{SkillId}",
                };

                EventEdges = new[]
                {
                    // Event's edges
                    // https://developers.facebook.com/docs/workplace/reference/graph-api/event

                    $"/{EventId}",
                    $"/{EventId}/admins",
                    //$"/{eventId}/attending", // non-existing field
                    //$"/{eventId}/interested", // non-existing field
                    //$"/{eventId}/declined", // non-existing field
                    //$"/{eventId}/maybe", // non-existing field
                    //$"/{eventId}/photos", // non-existing field
                    //$"/{eventId}/picture", // Unknown facebook response
                    $"/{EventId}/feed",
                };
            }
        }
    }
}