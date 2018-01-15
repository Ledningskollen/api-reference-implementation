using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Api.External.Models.V4.Common;
using Api.External.Models.V4.Recipient;
using ApiClient.Helper;
using ApiClient.Models;
using NUnit.Framework;
using GlobalSettings = ApiClient.Helper.GlobalSettings;

namespace TestV4 {
    [TestFixture]
    public class Areas {
        [SetUp]
        public void Init() {
            // LOGIN
            _context = Authentication.Login(GlobalSettings.UserName_Co,
                GlobalSettings.Password_Co).Result;
            Assert.IsFalse(string.IsNullOrWhiteSpace(_context.Token));

            // GET USER ORGANIZATION ID
            var organizationId = User.GetOrganizationId(_context).Result;
            Assert.IsFalse(string.IsNullOrWhiteSpace(_context.OrganizationId));
        }

        [TearDown]
        public void Dispose() {
            Authentication.Logout(_context).Wait();
            Assert.IsTrue(string.IsNullOrWhiteSpace(_context.Token));
        }

        // Sample SWEREF99TM GeoJSON FeatureCollection
        private string _featureCollection =
                @"{""features"":[{""type"":""Feature"",""geometry"":{""type"":""Polygon"",""coordinates"":[[[6301470.1824371638,321765.78432757215],[6301465.8308711154,321790.4632852918],[6301441.199078504,321786.12003563583],[6301445.5506445458,321761.4410779092],[6301470.1824371638,321765.78432757215]]]}}],""type"":""FeatureCollection""}"
            ;

        private readonly string _crsFeatureCollection =
                @"{""type"":""FeatureCollection"",""features"":[{""type"":""Feature"",""properties"":{""crs"":4326},""geometry"":{""type"":""Polygon"",""coordinates"":[[[11.9644,57.7076],[11.9655,57.7076],[11.9655,57.708],[11.9644,57.708],[11.9644,57.7076]]]}}]}"
            ;

        private readonly string _crsFeatureCollection2 =
                @"{""type"":""FeatureCollection"",""features"":[{""type"":""Feature"",""properties"":{""crs"":4326},""geometry"":{""type"":""Polygon"",""coordinates"":[[[11.9645,57.7071],[11.9656,57.7071],[11.9656,57.7075],[11.9645,57.7075],[11.9645,57.7071]]]}}]}"
            ;

        private static Context _context;

        private static async Task<KeyValuePair<string, byte[]>>
            GetAreaGeometries(string organizationId, string areaId, int coordinateSystem, string dataFormat) {
            byte[] bytes;
            var fileName = string.Empty;
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                // HTTP GET
                var response = await client.GetAsync(
                    string.Format("organization/{0}/area/{1}/geometries/{2}/{3}", organizationId, areaId,
                        coordinateSystem, dataFormat));
                bytes = response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;

                if (response.IsSuccessStatusCode) {
                    fileName = Organization.GetFileNameFromContentDisposition(response);
                }
            }
            return new KeyValuePair<string, byte[]>(fileName, bytes);
        }

        /// <summary>
        ///     Method will get 1 or all Areas
        /// </summary>
        private static async Task<List<Area>> GetArea_JSON(string aoiId = null) {
            List<Area> result;
            using (var client = new HttpClient()) {
                //Set timeout to 60 seconds
                client.Timeout = TimeSpan.FromSeconds(60);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                var url = string.Format(
                    "organization/{0}/area" + (!string.IsNullOrEmpty(aoiId) ? "/" + aoiId : string.Empty),
                    _context.OrganizationId);
                // HTTP GET
                var response = await client.GetAsync(url);
                result = response.IsSuccessStatusCode
                    ? JsonUtils<List<Area>>.Deserialize(response.Content.ReadAsStringAsync().Result)
                    : null;
            }
            return result;
        }

        private static async Task<List<Area>> GetAreaType_JSON(string areaType) {
            List<Area> result;
            using (var client = new HttpClient()) {
                //Set timeout to 60 seconds
                client.Timeout = TimeSpan.FromSeconds(60);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                var url = string.Format("organization/{0}/area/{1}", _context.OrganizationId, areaType);
                // HTTP GET
                var response = await client.GetAsync(url);
                result = response.IsSuccessStatusCode
                    ? JsonUtils<List<Area>>.Deserialize(response.Content.ReadAsStringAsync().Result)
                    : null;
            }
            return result;
        }

        private static async Task<Area> AddArea(Area area) {
            Area result;
            using (var client = new HttpClient()) {
                //Set timeout to 60 seconds
                client.Timeout = TimeSpan.FromSeconds(60);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                var aoiJson = JsonUtils<Area>.Serialize(area);
                // HTTP GET
                var response = await client
                    .PostAsync(
                        string.Format("organization/{0}/area", _context.OrganizationId),
                        new StringContent(aoiJson, Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode) {
                    return null;
                }
                result = JsonUtils<Area>.Deserialize(response.Content.ReadAsStringAsync().Result);
            }

            return result;
        }

        private static async Task<List<Recipient>> OrganizationRecipients_JSON() {
            List<Recipient> result;
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token",
                    _context.Token);

                // HTTP GET
                var response = await client
                    .GetAsync(string.Format("organization/{0}/recipient", _context.OrganizationId));
                result = response.IsSuccessStatusCode
                    ? JsonUtils<List<Recipient>>.Deserialize(response.Content.ReadAsStringAsync().Result)
                    : null;
            }
            return result;
        }

        private static async Task<Area> PatchArea(Area area) {
            Area result;

            using (var client = new HttpClient()) {
                //Set timeout to 60 seconds
                client.Timeout = TimeSpan.FromSeconds(60);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                var aoiJson = JsonUtils<Area>.Serialize(area);
                // HTTP GET
                var response = await client
                    .PutAsync(
                        string.Format("organization/{0}/area", _context.OrganizationId),
                        new StringContent(aoiJson, Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode) {
                    return null;
                }
                result = JsonUtils<Area>.Deserialize(response.Content.ReadAsStringAsync().Result);
            }

            return result;
        }

        private static async Task<bool> DeleteArea_JSON(string areaId) {
            bool result;
            using (var client = new HttpClient()) {
                //Set timeout to 60 seconds
                client.Timeout = TimeSpan.FromSeconds(60);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token",
                    _context.Token);

                // HTTP GET
                var response = await client.DeleteAsync(
                    string.Format("organization/{0}/area/{1}", _context.OrganizationId, areaId));
                result = response.IsSuccessStatusCode;
            }
            return result;
        }

        private static void CheckDeleteAddPatchArea(Area originalArea, Area changedArea) {
            Assert.That(originalArea.Geometry.Count, Is.GreaterThanOrEqualTo(24),
                "Should be 24 polygons in the first grid");

            var addedList = GetArea_JSON().Result;
            Assert.That(addedList.Any(x => x.Id.Equals(originalArea.Id)), Is.True,
                "The new geometry hasn't been saved.");

            Assert.That(changedArea.Geometry.Count, Is.GreaterThanOrEqualTo(26),
                "Should be 26 polygons in the second grid (24 + 2)");

            var result = DeleteArea_JSON(originalArea.Id).Result;

            Assert.That(result, Is.True, "Could not delete AOI");

            // Make sure it was deleted
            var remainderList = GetArea_JSON().Result;
            Assert.That(remainderList.Any(x => x.Id.Equals(originalArea.Id)), Is.False,
                "The remaining list includes the deleted AOI");
        }

        private static Dictionary<string, string> CreateRecipients(List<Recipient> recipientList, string areaType,
            Random r) {
            var recipients1 = new Dictionary<string, string> {
                {
                    "cable indication",
                    recipientList.GetRange(r.Next(0, recipientList.Count()), 1)
                        .First().Id
                }, {
                    "project",
                    recipientList.GetRange(r.Next(0, recipientList.Count()), 1)
                        .First().Id
                }, {
                    "planning",
                    recipientList.GetRange(r.Next(0, recipientList.Count()), 1)
                        .First().Id
                }
            };

            var caseTypeToAdd = areaType == "affected" ? "emergency" : "collaboration";
            recipients1.Add(caseTypeToAdd, recipientList.GetRange(r.Next(0, recipientList.Count), 1)
                .First().Id);
            return recipients1;
        }

        /// <summary>
        ///     Loops through the result list and does some basic testing
        /// </summary>
        private void ChecksForAreas(
            IEnumerable<Area> areaList,
            string areaType = null) {
            foreach (var area in areaList) {
                if (areaType != null) {
                    Assert.That(areaType.Equals(area.AreaType), "All areas should be of the same type.");
                }
                ChecksForAreas(area);
            }
        }

        /// <summary>
        ///     Do basic checking for an Area
        /// </summary>
        /// <param name="area"></param>
        private void ChecksForAreas(Area area) {
            Assert.IsNotNull(area.Id);
            Assert.IsNotNullOrEmpty(area.Name);
            Assert.IsNotNull(area.Geometry);
            Assert.IsNotNull(area.Recipients);
            Assert.IsNotNull(area.AreaType);

            if (area.AreaType == "affected") {
                Assert.That(area.Recipients.Count, Is.EqualTo(4),
                    "There should be 1 recipient assigned for every case type that affected areas support.");
            } else {
                Assert.That(area.Recipients.Count, Is.GreaterThanOrEqualTo(1),
                    "There should be 1 recipient assigned for every case type that control areas support.");
            }
        }

        [Test]
        public void DeleteAndAddAreasAsJson() {
            // This are the areas of interest. They will transformed into a 1km2 area for security
            const string affectedType = "affected";

            // This are the bevakninsområden. They describe the interested areas, but not the where the cables are.
            const string controlType = "control";

            var recipientList = OrganizationRecipients_JSON().Result;
            var r = new Randomizer();

            //Change recipient for "cable indication" (affected)
            var recipients2Affected = new Dictionary<string, string> {
                {
                    "cable indication",
                    recipientList.GetRange(r.Next(0, recipientList.Count()), 1)
                        .First().Id
                }
            };

            //Change recipient for "cable indication" and set recipient to "none" for project (control)
            var recipients2Control = new Dictionary<string, string> {
                {
                    "cable indication",
                    recipientList.GetRange(r.Next(0, recipientList.Count()), 1)
                        .First().Id
                }, {
                    "project",
                    ""
                }
            };

            var addedAffectedArea = AddArea(new Area {
                Geometry = _crsFeatureCollection,
                Name = "ReallyUniqueLongUniqueName123",
                Recipients = CreateRecipients(recipientList, affectedType, r),
                AreaType = affectedType
            }).Result;

            var addedControlArea = AddArea(new Area {
                Geometry = _crsFeatureCollection,
                Name = "EvenMoreUniquLongUniqueName",
                Recipients =
                    CreateRecipients(recipientList,
                        controlType,
                        r), //Should be other recipients no emergency,
                AreaType = controlType
            }).Result;

            var changedAffectedArea = PatchArea(new Area {
                Id = addedAffectedArea.Id,
                Geometry = _crsFeatureCollection2,
                Name = "ReallyUniqueLongUniqueNameThatWasChanged",
                Recipients = recipients2Affected
            }).Result;

            var changedControlArea = PatchArea(new Area {
                Id = addedControlArea.Id,
                Geometry = _crsFeatureCollection2,
                Name = "EvenMoreUniqueLongUniqueNameThatWasChanged",
                Recipients = recipients2Control
            }).Result;

            CheckDeleteAddPatchArea(addedAffectedArea, changedAffectedArea);
            CheckDeleteAddPatchArea(addedControlArea, changedControlArea);
        }

        [Test]
        public void GetAffectedAreasAsJson() {
            const string areaType = "affected";
            var affectedAreaList = GetAreaType_JSON(areaType);

            // Basic checking that the result contains one or more area of interests
            Assert.IsNotNull(affectedAreaList.Result);
            Assert.GreaterOrEqual(affectedAreaList.Result.Count, 1, "No control areas");

            ChecksForAreas(affectedAreaList.Result, areaType);
        }

        [Test]
        public void GetAreaGeometries() {
            var area = GetArea_JSON().Result.FirstOrDefault();
            Assert.IsNotNull(area, "The area  was null");

            var coordinateSystem =
                Constants.LkSupportedCoordinateSystems()
                    .Values.OrderBy(x => Guid.NewGuid())
                    .First();
            const string dataFormat = "WKT";

            var task = GetAreaGeometries(_context.OrganizationId, area.Id, coordinateSystem, dataFormat);

            Assert.IsNotNull(task, "The task was null");
            Assert.IsNotNull(task.Result, "There result is empty");

            //Save file as requested in ticket comment
            using (var sourceStream = new FileStream(task.Result.Key, FileMode.Append, FileAccess.Write)) {
                sourceStream.Write(task.Result.Value, 0, task.Result.Value.Length);
            }
        }

        [Test]
        public void GetAreasAsJson() {
            // Get all areas for the current organization
            var areaList = GetArea_JSON();

            // Basic checking that the result contains one or more areas
            Assert.IsNotNull(areaList.Result);
            Assert.GreaterOrEqual(areaList.Result.Count, 1, "No areas");

            ChecksForAreas(areaList.Result);

            // Get the first specific area for the current organization
            var specificAoi = GetArea_JSON(areaList.Result.First().Id);

            // Basic checking that the result contains one area
            Assert.IsNotNull(specificAoi.Result);
            Assert.IsTrue(specificAoi.Result.Count == 1);

            ChecksForAreas(specificAoi.Result);
        }

        [Test]
        public void GetControlAreasAsJson() {
            const string areaType = "control";
            var controlAreaList = GetAreaType_JSON(areaType);

            // Basic checking that the result contains one or more control area
            Assert.IsNotNull(controlAreaList.Result);
            Assert.GreaterOrEqual(controlAreaList.Result.Count, 1, "No control areas");

            ChecksForAreas(controlAreaList.Result, areaType);
        }
    }
}
