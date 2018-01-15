using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Api.External.Models.V4.Common;
using Api.External.Models.V4.Lemmy;
using ApiClient.Helper;
using ApiClient.Models;
using NUnit.Framework;
using GlobalSettings = ApiClient.Helper.GlobalSettings;

namespace TestV4 {
    [TestFixture]
    public class Lemmy {
        [SetUp]
        public void Init() {
            // LOGIN
            _context = LemmyLogin().Result;
            Assert.IsFalse(string.IsNullOrWhiteSpace(_context.Token));

            // GET USER ORGANIZATION ID
            User.GetOrganizationId(_context).Wait();
            Assert.IsFalse(string.IsNullOrWhiteSpace(_context.OrganizationId));
        }

        private static Context _context;

        private enum NotificationType {
            Information,
            ActionRequired
        }

        private async Task<Context> LemmyLogin() {
            var context = new Context();
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Create encoded credentials 
                var credentials = GlobalSettings.UserName_Co + ":" + GlobalSettings.Password_Co;
                var bCredentials = Encoding.GetEncoding("ISO-8859-1").GetBytes(credentials);
                var base64Credentials = Convert.ToBase64String(bCredentials);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Set Authorization header and encoded credentials
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + base64Credentials);

                // HTTP POST
                var response = await client.PostAsync(string.Format("lemmy/{0}/{1}/auth",
                    GlobalSettings.LemmyToken, GlobalSettings.LemmyVersion), new StringContent(""));

                if (!response.IsSuccessStatusCode) {
                    return null;
                }

                context.Token = await response.Content.ReadAsStringAsync();
            }
            return context;
        }

        /// <summary>
        ///     Posts a notification
        /// </summary>
        private async Task<bool> PostNotification(NotificationMessage message, string type) {
            bool result;
            // Create a client and connect to LK instance
            using (var client = new HttpClient()) {
                // Configure request
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Configure request body
                var jsonString = JsonUtils<NotificationMessage>.Serialize(message);
                var stringContent = new StringContent(jsonString, Encoding.UTF8);
                stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // HTTP POST
                var response = await client.PostAsync(
                    string.Format("lemmy/{0}/{1}/notification/{2}", new Guid(GlobalSettings.LemmyToken),
                        GlobalSettings.LemmyVersion, type), stringContent);
                result = response.IsSuccessStatusCode;
            }
            return result;
        }


        private NotificationMessage GetNotification(NotificationType type) {
            var installationKey = new Guid(GlobalSettings.LemmyToken);

            switch (type) {
                case NotificationType.Information:
                    return new NotificationMessage {
                        Subject = "Information subject",
                        Body = "Information body"
                    };
                case NotificationType.ActionRequired:
                    return new NotificationMessage {
                        Subject = "ActionRequired subject",
                        Body = "ActionRequired body"
                    };
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        ///     Gets a list of lemmy settings
        /// </summary>
        private async Task<List<Settings>> GetLemmySettings() {
            List<Settings> result;

            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                // HTTP GET
                //lemmy/settings/{lemmyToken:guid}/{version}
                var response = await client
                    .GetAsync(string.Format("lemmy/{0}/{1}/settings",
                        new Guid(GlobalSettings.LemmyToken), GlobalSettings.LemmyVersion));
                result = response.IsSuccessStatusCode
                    ? JsonUtils<List<Settings>>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;
            }
            return result;
        }

        /// <summary>
        ///     Loops through a list of LemmySetting and performs basic checking
        /// </summary>
        private void ChecksForLemmySettings(List<Settings> settings) {
            Assert.Greater(settings.Count, 0);
            foreach (var setting in settings) ChecksForLemmySetting(setting);
        }

        /// <summary>
        ///     Basic checking of LemmySetting
        /// </summary>
        private void ChecksForLemmySetting(Settings lemmySetting) {
            Assert.IsNotNullOrEmpty(lemmySetting.Key, "Settings has no key.");
            Assert.IsNotNullOrEmpty(lemmySetting.DataType, "Settings has no DataType.");
            Assert.IsNotNullOrEmpty(lemmySetting.Value, "Settings has no Value.");
        }

        private async Task<Event> ReplyToCase(string caseNumber, string areaType, string recipientId, Reply reply) {
            Event result;
            // Create a client and connect to LK instance
            using (var client = new HttpClient()) {
                // Configure request
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Configure request body
                var jsonString = JsonUtils<Reply>.Serialize(reply);
                var stringContent = new StringContent(jsonString, Encoding.UTF8);
                stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // HTTP POST
                var response =
                    await
                        client.PostAsync(
                            string.Format(
                                "organization/{0}/cableownercase/{1}/{2}/caserecipient/{3}/reply",
                                _context.OrganizationId, caseNumber, areaType, recipientId),
                            stringContent);

                result = JsonUtils<Event>.Deserialize(
                    response.Content.ReadAsStringAsync().Result,
                    response.IsSuccessStatusCode);
            }

            return result;
        }

        [Test]
        public void AnsweringCase_Test() {
            // get cases which I can confirm
            var cases = Organization.GetCableOwnerCases_Filter(null, 5, null, null,
                new[] {Constants.CABLE_INQUIRY_PURPOSE, Constants.PROJECT_INQUIRY_PURPOSE}, "toconfirm", null,
                _context).Result;

            var firstCase = cases.First();
            var caseRecipient = firstCase.Recipients.First();
            var recipientId = caseRecipient.Recipient.Id;
            var areaType = caseRecipient.AreaType;

            var replyEvent = new Reply {
                Comment = "NO_INTEREST",
                Action = new List<string> {"NO INTEREST"}
            };

            var taskGetRecipient = ReplyToCase(firstCase.Id, areaType, recipientId, replyEvent);
            Assert.IsNotNull(taskGetRecipient);
            Assert.IsNotNull(taskGetRecipient.Result);

            var eventModel = taskGetRecipient.Result;
            Assert.IsNotNullOrEmpty(eventModel.Comment, "The event can't be empty");
            Assert.That(eventModel.Measure.Count, Is.EqualTo(replyEvent.Action.Count),
                "The actions are not the same as the once sent");
        }

        [Test]
        public void GetLemmySettings_Test() {
            var task = GetLemmySettings();
            Assert.IsNotNull(task);
            Assert.IsNotNull(task.Result);

            ChecksForLemmySettings(task.Result);
        }

        [Test]
        public void PostNotification_Test() {
            // Create information notification
            var notificationInformation = GetNotification(NotificationType.Information);

            var taskPostNotificationInformation =
                PostNotification(notificationInformation, NotificationType.Information.ToString());
            Assert.IsNotNull(taskPostNotificationInformation);
            Assert.IsNotNull(taskPostNotificationInformation.Result);
            Assert.IsTrue(taskPostNotificationInformation.Result);

            // Create AtionRequired notification
            var notificationActionRequired = GetNotification(NotificationType.ActionRequired);

            var taskPostNotificationActionRequired =
                PostNotification(notificationActionRequired, NotificationType.ActionRequired.ToString());
            Assert.IsNotNull(taskPostNotificationActionRequired);
            Assert.IsNotNull(taskPostNotificationActionRequired.Result);
            Assert.IsTrue(taskPostNotificationActionRequired.Result);
        }

        [Test]
        public void TestLogin() {
            var lemmyContext = LemmyLogin().Result;
            Assert.IsNotNull(lemmyContext, "The context after lemmy logging in is null.");
            Assert.That(string.IsNullOrWhiteSpace(lemmyContext.Token), Is.False,
                "The token has not been set for lemmy login.");
        }
    }
}
