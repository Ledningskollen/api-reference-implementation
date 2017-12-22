using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Api.External.Models.V4.Case;
using Api.External.Models.V4.Common;
using Api.External.Models.V4.Recipient;
using ApiClient.Helper;
using ApiClient.Models;
using NUnit.Framework;
using GlobalSettings = ApiClient.Helper.GlobalSettings;

namespace TestV4 {

    [TestFixture]
    public class Organization {
        private static string CaseNumber { get; set; }
        private static string RecipientId { get; set; }

        private static Context _context;

        [SetUp]
        public void Init() {
            // LOGIN
            _context = Authentication.Login(GlobalSettings.UserName_Co, GlobalSettings.Password_Co).Result;
            Assert.IsFalse(string.IsNullOrWhiteSpace(_context.Token));

            // GET USER ORGANIZATION ID
            User.GetOrganizationId(_context).Wait();
            Assert.IsFalse(string.IsNullOrWhiteSpace(_context.OrganizationId));


            // GET THE FIRST CASE IN HISTORY WITH ANY RECIPIENTS
            var cases = GetCableOwnerCases_JSON(EnumUtils.ToDescription(Constants.Case_Status.HISTORICAL), 10).Result;
            CaseNumber = cases.First(x => x.Recipients.Any()).Id;

            //Get the first recipient in organization 
            RecipientId = OrganizationRecipients_JSON().Result.First().Id;

            Assert.IsFalse(string.IsNullOrWhiteSpace(CaseNumber));
        }

        [TearDown]
        public void Dispose() {
            Authentication.Logout(_context).Wait();

            Assert.IsTrue(string.IsNullOrWhiteSpace(_context.Token));
        }

        private async Task<CableOwnerCase> CableOwnerCaseByNumber_JSON(
            string caseNumber = null) {
            if (string.IsNullOrWhiteSpace(caseNumber)) {
                caseNumber = CaseNumber;
            }

            CableOwnerCase result;
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                // HTTP GET
                var response = await client
                    .GetAsync(
                        string.Format("organization/{0}/cableownercase/{1}",
                            _context.OrganizationId,
                            caseNumber));
                result = response.IsSuccessStatusCode
                    ? JsonUtils<CableOwnerCase>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;
            }

            return result;
        }

        private static async Task<XmlDocument> CableOwnerCaseByNumber_XML() {

            var result = new XmlDocument();
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);
                client.DefaultRequestHeaders.Add("Accept", "text/xml");

                // HTTP GET
                var response = await client
                    .GetAsync(
                        string.Format("organization/{0}/cableownercase/{1}",
                            _context.OrganizationId,
                            CaseNumber));
                if (!response.IsSuccessStatusCode) {
                    return result;
                }
                var xml = response.Content.ReadAsStringAsync().Result;
                result.LoadXml(xml);
            }
            return result;
        }

        private static async Task<List<CableOwnerCase>> GetCableOwnerCases_JSON(
            string status, int limit = 100, Context context = null) {
            List<CableOwnerCase> result;
            if (context == null) {
                context = _context;
            }
            using (var client = new HttpClient()) {
                //Set timeout to 60 seconds
                client.Timeout = TimeSpan.FromSeconds(60);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", context.Token);

                // HTTP GET
                var response = await client
                    .GetAsync(
                        string.Format(
                            "organization/{0}/cableownercases/{1}/{2}",
                            context.OrganizationId,
                            status,
                            limit));


                result = JsonUtils<List<CableOwnerCase>>.Deserialize(
                    response.Content.ReadAsStringAsync().Result,
                    response.IsSuccessStatusCode);
            }
            return result;
        }

        private static async Task<List<CableOwnerCase>>
            GetCableOwnerCases_Before_JSON(
            string status, string caseNumber, int limit = 100) {
            List<CableOwnerCase> result;
            using (var client = new HttpClient()) {
                //Set timeout to 60 seconds
                client.Timeout = TimeSpan.FromSeconds(60);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token",
                    _context.Token);

                // HTTP GET
                var response = await client
                    .GetAsync(
                        string.Format(
                            "organization/{0}/cableownercases/{1}/before/{2}/{3}",
                            _context.OrganizationId,
                            status,
                            caseNumber,
                            limit));
                result = response.IsSuccessStatusCode
                    ? JsonUtils<List<CableOwnerCase>>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;
            }
            return result;
        }

        private static async Task<List<CableOwnerCase>>
            GetCableOwnerCases_After_JSON(
            string status, string caseNumber, int limit = 100) {
            List<CableOwnerCase> result;
            using (var client = new HttpClient()) {
                //Set timeout to 60 seconds
                client.Timeout = TimeSpan.FromSeconds(60);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token",
                    _context.Token);

                // HTTP GET
                var response = await client
                    .GetAsync(
                        string.Format(
                            "organization/{0}/cableownercases/{1}/after/{2}/{3}",
                            _context.OrganizationId,
                            status,
                            caseNumber,
                            limit));
                result = response.IsSuccessStatusCode
                    ? JsonUtils<List<CableOwnerCase>>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;
            }
            return result;
        }

        /// <summary>
        /// Posts a filter case request
        /// </summary>
        private async Task<List<CableOwnerCase>> PostCableOwnerCases_Filter(CaseFilters filters) {

            List<CableOwnerCase> result;
            // Create a client and connect to LK instance
            using (var client = new HttpClient()) {
                // Configure request
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // Configure request body
                var jsonString = JsonUtils<CaseFilters>.Serialize(filters);
                var stringContent = new StringContent(jsonString, Encoding.UTF8);
                stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // HTTP POST
                var response = await client.PostAsync(
                            string.Format("organization/{0}/cableownercasefilter", _context.OrganizationId),
                            stringContent);

                result = JsonUtils<List<CableOwnerCase>>.Deserialize(
                    response.Content.ReadAsStringAsync().Result,
                    response.IsSuccessStatusCode);

            }
            return result;
        }

        public static async Task<List<CableOwnerCase>>
            GetCableOwnerCases_Filter(
            string caseNumber = null, int? limit = null,
            DateTime? fromDate = null,
            DateTime? toDate = null, string[] types = null,
            string status = null, Guid[] recipientList = null,
            Context context = null) {

            if (context == null) {
                context = _context;
            }

            List<CableOwnerCase> result;
            using (var client = new HttpClient()) {
                //Set timeout to 60 seconds
                client.Timeout = TimeSpan.FromSeconds(60);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token",
                    context.Token);

                // HTTP GET
                var queryString = "";

                if (!string.IsNullOrEmpty(caseNumber)) {
                    if (string.IsNullOrEmpty(queryString)) {
                        queryString = "?";
                    } else {
                        queryString += "&";
                    }
                    queryString += "caseNumber=" + caseNumber;
                }

                if (limit.HasValue) {
                    if (string.IsNullOrEmpty(queryString)) {
                        queryString = "?";
                    } else {
                        queryString += "&";
                    }
                    queryString += "limit=" + limit;
                }

                if (fromDate.HasValue) {
                    if (string.IsNullOrEmpty(queryString)) {
                        queryString = "?";
                    } else {
                        queryString += "&";
                    }
                    queryString += "from=" +
                                   fromDate.Value.ToShortDateString();
                }

                if (toDate.HasValue) {
                    if (string.IsNullOrEmpty(queryString)) {
                        queryString = "?";
                    } else {
                        queryString += "&";
                    }
                    queryString += "to=" + toDate.Value.ToShortDateString();
                }

                if (types != null && types.Any()) {
                    if (string.IsNullOrEmpty(queryString)) {
                        queryString = "?";
                    } else {
                        queryString += "&";
                    }
                    queryString += "types=" +
                                   types.Aggregate((x, y) => x + "&types=" + y);
                }

                if (!string.IsNullOrEmpty(status)) {
                    if (string.IsNullOrEmpty(queryString)) {
                        queryString = "?";
                    } else {
                        queryString += "&";
                    }
                    queryString += "status=" + status;
                }

                if (recipientList != null && recipientList.Any()) {
                    if (string.IsNullOrEmpty(queryString)) {
                        queryString = "?";
                    } else {
                        queryString += "&";
                    }
                    string result1 = null;
                    var first = true;
                    foreach (var guid in recipientList) {
                        if (first) {
                            first = false;
                            result1 = "recipientList=" + guid;
                            continue;
                        }
                        result1 += "&recipientList=" + guid;
                    }
                    queryString += result1;
                }

                var response = await client
                    .GetAsync(string.Format(
                        "organization/{0}/cableownercasefilter",
                        context.OrganizationId) + queryString);

                result = response.IsSuccessStatusCode
                    ? JsonUtils<List<CableOwnerCase>>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;
            }

            return result;
        }

        private static async Task<KeyValuePair<string, byte[]>> GetCableOwnerCasePdf(
            string organizationId, string caseId) {
            var fileName = string.Empty;

            byte[] bytes;
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                // HTTP GET
                var response = await client
                    .GetAsync(
                        string.Format(
                            "organization/{0}/cableownercase/{1}/pdf",
                            organizationId,
                            caseId));
                bytes = response.IsSuccessStatusCode
                    ? await response.Content.ReadAsByteArrayAsync()
                    : null;

                var msg = response.Content.ReadAsByteArrayAsync();

                if (response.IsSuccessStatusCode) {
                    fileName = GetFileNameFromContentDisposition(response);
                }
            }

            return new KeyValuePair<string, byte[]>(fileName, bytes);
        }

        public static string GetFileNameFromContentDisposition(HttpResponseMessage response) {
            var fileName = string.Empty;
            try {
                var contentDisposition =
                    response.Content.Headers.First(
                        h => h.Key.Equals("Content-Disposition", StringComparison.OrdinalIgnoreCase));
                var fileNameObj =
                    contentDisposition.Value.First(v => v.Contains("filename"));
                var index = fileNameObj.LastIndexOf("filename=", StringComparison.OrdinalIgnoreCase);
                if (index > -1) {
                    fileName = fileNameObj.Substring(index + "filename=".Length);
                }
            } catch (Exception e) {
                Console.WriteLine("Problems getting the file name. {0}", e);
            }

            return fileName;
        }

        private static async Task<KeyValuePair<string, byte[]>> GetCableOwnerCaseGeometries(
            string organizationId, string caseNumber, int coordinateSystem,
            string dataFormat) {

            byte[] bytes;
            var fileName = string.Empty;
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);
                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);
                // HTTP GET
                var url = string.Format(
                    "organization/{0}/cableownercase/{1}/geometries/{2}/{3}",
                    organizationId,
                    caseNumber,
                    coordinateSystem,
                    dataFormat);
                var response = await client.GetAsync(url);
                bytes = response.IsSuccessStatusCode
                    ? await response.Content.ReadAsByteArrayAsync()
                    : null;

                if (response.IsSuccessStatusCode) {
                    fileName = GetFileNameFromContentDisposition(response);
                }
            }
            return new KeyValuePair<string, byte[]>(fileName, bytes);
        }

        private static async Task<KeyValuePair<string, byte[]>> GetAreaGeometries(
            string organizationId, string areaId,
            int coordinateSystem, string dataFormat) {

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
                var response = await client
                    .GetAsync(
                        string.Format(
                            "organization/{0}/area/{1}/geometries/{2}/{3}",
                            organizationId,
                            areaId,
                            coordinateSystem,
                            dataFormat));
                bytes = response.IsSuccessStatusCode
                    ? await response.Content.ReadAsByteArrayAsync()
                    : null;

                if (response.IsSuccessStatusCode) {
                    fileName = GetFileNameFromContentDisposition(response);
                }
            }
            return new KeyValuePair<string, byte[]>(fileName, bytes);
        }

        private static async Task<Recipient> OrganizationRecipient_JSON(
            string organizationId, string recipientId) {
            Recipient result = null;
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                // HTTP GET
                var response = await client
                    .GetAsync(string.Format("organization/{0}/recipient/{1}",
                        organizationId,
                        recipientId));
                var recipients = response.IsSuccessStatusCode
                    ? JsonUtils<List<Recipient>>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;

                if (recipients != null)
                    result = recipients.FirstOrDefault();
            }


            return result;
        }

        private static async Task<XmlDocument> OrganizationRecipients_XML() {
            var result = new XmlDocument();
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);
                client.DefaultRequestHeaders.Add("Accept", "text/xml");

                // HTTP GET
                var response = await client
                    .GetAsync(string.Format("organization/{0}/recipient",
                        _context.OrganizationId));
                if (!response.IsSuccessStatusCode) {
                    return result;
                }
                var xml = response.Content.ReadAsStringAsync().Result;
                result.LoadXml(xml);
            }
            return result;
        }

        /// <summary>
        /// Method will get 1 or all Areas
        /// </summary>
        private static async Task<List<Area>> GetArea_JSON(string areaId = null) {
            List<Area> result;
            using (var client = new HttpClient()) {
                //Set timeout to 60 seconds
                client.Timeout = TimeSpan.FromSeconds(60);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token",
                    _context.Token);

                var url =
                    string.Format(
                        "organization/{0}/area" +
                        (!string.IsNullOrEmpty(areaId)
                            ? "/" + areaId
                            : string.Empty),
                        _context.OrganizationId);
                // HTTP GET
                var response = await client
                    .GetAsync(url);
                result = response.IsSuccessStatusCode
                    ? JsonUtils<List<Area>>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;
            }
            return result;
        }

        /// <summary>
        /// Posts a reply on the case
        /// </summary>
        private async Task<Event> PostReply(string caseNumber, string areaType, string recipientId, Reply reply) {
            if (string.IsNullOrWhiteSpace(caseNumber)) {
                caseNumber = CaseNumber;
            }

            Event result;
            // Create a client and connect to LK instance
            using (var client = new HttpClient()) {
                // Configure request
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // Configure request body
                var jsonString = JsonUtils<Reply>.Serialize(reply);
                var stringContent = new StringContent(jsonString, Encoding.UTF8);
                //var stringContent = new StringContent(jsonString, Encoding.GetEncoding("ISO-8859-1"));
                stringContent.Headers.ContentType =
                    new MediaTypeHeaderValue("application/json");

                // HTTP POST
                var response =
                    await
                        client.PostAsync(
                            string.Format(
                                "organization/{0}/cableownercase/{1}/{2}/caserecipient/{3}/reply",
                                _context.OrganizationId,
                                caseNumber,
                                areaType,
                                recipientId),
                            stringContent);

                result = JsonUtils<Event>.Deserialize(
                    response.Content.ReadAsStringAsync().Result,
                    response.IsSuccessStatusCode);

            }
            return result;
        }

        /// <summary>
        /// Marks a case as fetched
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <param name="areaType"></param>
        /// <param name="recipientId"></param>
        /// <returns></returns>
        private async Task<bool> TakeCase(string caseNumber, string areaType, string recipientId) {
            if (string.IsNullOrWhiteSpace(caseNumber)) {
                throw new Exception("Case number is required");
            }
            bool result;

            using (var client = new HttpClient()) {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                //HTTP POST 
                var response = 
                    await client.PostAsync(
                        string.Format(
                        "organization/{0}/cableownercase/{1}/{2}/caserecipient/{3}/take",
                        _context.OrganizationId, caseNumber, areaType, recipientId),
                        new StringContent(""));
                result = response.IsSuccessStatusCode;
            }
            return result; 
        }

        private async Task<KeyValuePair<string, byte[]>> GetAttachment(string caseNumber,
            string fileName, bool zip) {
            var _fileName = "no-name.file";
            if (string.IsNullOrWhiteSpace(caseNumber)) {
                throw new Exception("Case number is required");
            }
            if (string.IsNullOrWhiteSpace(fileName)) {
                throw new Exception("File name is required");
            }

            List<int> result;
            byte[] bytes = new byte[1];

            using (var client = new HttpClient()) {
                // Configure request
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                // HTTP GET
                var response = await client
                    .GetAsync(
                        string.Format(
                            "inquiry/{0}/attachment/{1}?zip={2}",
                            caseNumber,
                            fileName,
                            zip));

                if (response.IsSuccessStatusCode) {
                    bytes = await response.Content.ReadAsByteArrayAsync();
                    _fileName = GetFileNameFromContentDisposition(response);
                } else {
                    // if failed => this will thrown an exception
                    // this code is to force the exception to be thrown
                    result = JsonUtils<List<int>>.Deserialize(
                        response.Content.ReadAsStringAsync().Result,
                        response.IsSuccessStatusCode);
                }

            }
            return new KeyValuePair<string, byte[]>(_fileName, bytes);
        }

        private static async Task<List<Measures>> GetMeasures(string areaType, string caseType) {
            List<Measures> result;

            using (var client = new HttpClient()) {
                //Set timeout to 60 seconds
                client.Timeout = TimeSpan.FromSeconds(60);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                // HTTP GET
                var response = await client
                    .GetAsync(
                        string.Format(
                            "organization/{0}/measures/{1}/{2}",
                            _context.OrganizationId,
                            areaType, caseType));


                result = JsonUtils<List<Measures>>.Deserialize(
                    response.Content.ReadAsStringAsync().Result,
                    response.IsSuccessStatusCode);
            }
            return result;
        }

        [Test]
        public void GetCableOwnerCaseAsJson() {
            var task = CableOwnerCaseByNumber_JSON();
            Assert.IsNotNull(task);
            Assert.IsNotNull(task.Result);

            ChecksForCableOwnerCase(task.Result);
        }

        [Test]
        public void GetCableOwnerCaseAsXml() {
            var task = CableOwnerCaseByNumber_XML();
            Assert.IsNotNull(task);
            Assert.IsNotNull(task.Result);
        }

        [Test]
        public void GetCableOwnerCasesAsJson() {

            // STATUSES
            // ToConfirm = cases that have inquiries whom should be handled (viewed) by this organization
            // Confirmed = cases which have been already confirmed by this organization
            // Canceled = cases which have been canceled by the inquirer (case creator)
            // Historical = all cases regardless of their statuses. Useful when archiving cases is needed

            var statuses = new[]
            {"toconfirm", "confirmed", "canceled", "historical"};
            foreach (var s in statuses) {
                // Get the first ten latest cases of each status
                var cases = GetCableOwnerCases_JSON(s, 10);
                Assert.IsNotNull(cases);
                Assert.IsNotNull(cases.Result);

                // Do basic checks of the cases
                ChecksForCableOwnerCase(
                    cases.Result.Where(x => x.Recipients.Any()));

                // Get the nine cases before the last case
                var casesBefore = GetCableOwnerCases_Before_JSON(s,
                    cases.Result.First().Id,
                    9);
                Assert.IsNotNull(casesBefore);
                Assert.IsNotNull(casesBefore.Result);

                // Get the nine cases after the first case
                var casesAfter = GetCableOwnerCases_After_JSON(s,
                    cases.Result.Last().Id,
                    9);
                Assert.IsNotNull(casesAfter);
                Assert.IsNotNull(casesAfter.Result);

                // Compare that the before list and the after list are the same
                ChecksForCableOwnerCase(
                    casesBefore.Result.Take(casesBefore.Result.Count - 1).ToList(),
                    casesAfter.Result.Skip(1).Take(8).ToList());

                // Compare that nine out of ten cases are the same with original list
                ChecksForCableOwnerCase(casesBefore.Result, cases.Result, true);

                // Compare that nine out of ten cases are the same with original list
                ChecksForCableOwnerCase(casesAfter.Result, cases.Result, true);
            }
        }

        /// <summary>
        /// Test to reassign a recipient from an unanswered case to a different
        /// one. For this to work you need to have at least two recipients.
        /// </summary>
        [Test]
        public void ReassignRecipient_Test() {

            // getting a case which I have not given an answered
            var cases = GetCableOwnerCases_JSON(EnumUtils.ToDescription(Constants.Case_Status.TO_CONFIRM), 2).Result;
            var caze = cases.First();

            // extract its recipient
            var recipientFrom = caze.Recipients.FirstOrDefault();
            Assert.IsNotNull(recipientFrom, "The FROM recipient is null");

            // randomly get a second recipient
            var allRecipients = OrganizationRecipients_JSON().Result;
            if (allRecipients.Count < 2) {
                Assert.Fail("You only have one recipient, you need at least two to run this test.");
            }
            var recipientsWithout =
                allRecipients.Where(r => r.Id != recipientFrom.Recipient.Id);
            var recipientTo = recipientsWithout.LastOrDefault();
            Assert.IsNotNull(recipientTo, "The TO recipient is null");

            Assert.AreNotEqual(recipientFrom.Recipient.Id, recipientTo.Id);

            // Make the API call with the client to reassign recipient
            var reassignRecipient = new ReassignRecipient() {
                CurrentRecipientId = recipientFrom.Recipient.Id,
                NewRecipientId = recipientTo.Id
            };

            ReassignRecipient(caze.Id, recipientFrom.AreaType, reassignRecipient).Wait();

            var caseAfterSwitchRecipient =
                CableOwnerCaseByNumber_JSON(caze.Id).Result;
            Assert.AreEqual(caze.Id,
                caseAfterSwitchRecipient.Id,
                "Case id is different (" + caze.Id + ", " +
                caseAfterSwitchRecipient.Id + ")");
            Assert.AreEqual(caze.Type,
                caseAfterSwitchRecipient.Type,
                "The case types are not the same");
            Assert.True(
                caseAfterSwitchRecipient.Recipients.Any(
                    cr => cr.Recipient.Id == recipientTo.Id),
                "The new recipient was not assigned");
        }

        [Test]
        public void ReassignRecipientForAllCaseTypes_Test() {

            var caseTypes = new[] {
                Constants.PROJECT_INQUIRY_PURPOSE,
                Constants.COLLABORATION_INQUIRY_PURPOSE,
                Constants.PLANNING_INQUIRY_PURPOSE,
                Constants.CABLE_INQUIRY_PURPOSE,
            };

            foreach (var caseType in caseTypes) {

                //Get case which requires manual review 
                var caseList = GetCableOwnerCases_Filter(
                    types: new[] {caseType},
                    limit: 1,
                    status: EnumUtils.ToDescription(Constants.Case_Status.TO_CONFIRM));

                //Pick the first case which requires manual review
                var firstCase = caseList.Result.First();

                // extract its recipient
                var recipientFrom = firstCase.Recipients.FirstOrDefault(x => x.Status == "requires review");
                Assert.IsNotNull(recipientFrom, "The FROM recipient is null");

                // randomly get a second recipient
                var allRecipients = OrganizationRecipients_JSON().Result;
                if (allRecipients.Count < 2) {
                    Assert.Fail("You only have one recipient, you need at least two to run this test.");
                }
                var recipientsWithout =
                    allRecipients.Where(r => r.Id != recipientFrom.Recipient.Id);
                var recipientTo = recipientsWithout.LastOrDefault();
                Assert.IsNotNull(recipientTo, "The TO recipient is null");

                Assert.AreNotEqual(recipientFrom.Recipient.Id, recipientTo.Id);

                // Make the API call with the client to reassign recipient
                var reassignRecipient = new ReassignRecipient() {
                    CurrentRecipientId = recipientFrom.Recipient.Id,
                    NewRecipientId = recipientTo.Id
                };

                ReassignRecipient(firstCase.Id, recipientFrom.AreaType, reassignRecipient).Wait();

                var caseAfterSwitchRecipient =
                    CableOwnerCaseByNumber_JSON(firstCase.Id).Result;
                Assert.AreEqual(firstCase.Id, caseAfterSwitchRecipient.Id,
                                "Case id is different (" + firstCase.Id + ", " +
                                caseAfterSwitchRecipient.Id + ")");
                Assert.AreEqual(firstCase.Type, caseAfterSwitchRecipient.Type,
                                "The case types are not the same");
                Assert.True(caseAfterSwitchRecipient.Recipients.Any(cr => cr.Recipient.Id == recipientTo.Id),
                    "The new recipient was not assigned");

                Console.WriteLine("Case type {0} passed the test", caseType);
            }
        }

        private async Task ReassignRecipient(string caseNumber, string areaType, ReassignRecipient reassignRecipient) {

            // Create a client and connect to LK instance
            using (var client = new HttpClient()) {
                // Configure request
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                var jsonString =
                    JsonUtils<ReassignRecipient>.Serialize(reassignRecipient);
                var stringContent = new StringContent(jsonString);
                stringContent.Headers.ContentType =
                    new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(
                    string.Format(
                        "organization/{0}/cableownercase/{1}/{2}/reassign",
                        _context.OrganizationId,
                        caseNumber, areaType),
                    stringContent);

                Assert.True(response.IsSuccessStatusCode);
            }
        }

        private async Task<List<Recipient>> OrganizationRecipients_JSON() {

            List<Recipient> result;
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                // HTTP GET
                var response = await client
                    .GetAsync(string.Format("organization/{0}/recipient",
                        _context.OrganizationId));
                result = response.IsSuccessStatusCode
                    ? JsonUtils<List<Recipient>>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;
            }

            return result;
        }

        [Test]
        public void GetCaseByFilter() {
            var caseList =
                GetCableOwnerCases_Filter(
                    types: new[] {"cable indication", "emergency"},
                    limit: 10,
                    caseNumber: CaseNumber)
                    .Result;

            ChecksForCableOwnerCase(caseList);
        }

        [Test]
        public void PostCaseByFilter() {
            var filtes = new CaseFilters() { 
                caseTypes = new[] {Constants.CABLE_INQUIRY_PURPOSE, Constants.EMERGENCY_INQUIRY_PURPOSE},
                limit = 50,
            };
            var caseList = PostCableOwnerCases_Filter(filtes).Result;
            ChecksForCableOwnerCase(caseList);

            var bigSearchTotalResults = caseList.Count;

            filtes.from = "2017-08-01";
            filtes.to = "2018-08-01";

            caseList = PostCableOwnerCases_Filter(filtes).Result;
            ChecksForCableOwnerCase(caseList);

            var secondSearchTotalResults = caseList.Count;

            Assert.That(bigSearchTotalResults, Is.GreaterThanOrEqualTo(secondSearchTotalResults));

            filtes.geoData = @"{""type"":""FeatureCollection"",""features"":[{""type"":""Feature"",""properties"":{""crs"":4326},""geometry"":{""type"":""Polygon"",""coordinates"":[[[11.962287425994873,57.70461728172189],[11.96271926164627,57.70461728172189],[11.96271926164627,57.70482220900525],[11.962287425994873,57.70482220900525],[11.962287425994873,57.70461728172189]]]}}]}";

            filtes.caseTypes = new[] {Constants.CABLE_INQUIRY_PURPOSE, Constants.PROJECT_INQUIRY_PURPOSE};

            caseList = PostCableOwnerCases_Filter(filtes).Result;
            ChecksForCableOwnerCase(caseList);

            var thirdSearchTotalResults = caseList.Count;

            Assert.That(bigSearchTotalResults, Is.GreaterThanOrEqualTo(thirdSearchTotalResults));
            Assert.That(secondSearchTotalResults, Is.GreaterThanOrEqualTo(thirdSearchTotalResults));


            filtes.caseTypes = new[] {Constants.PROJECT_INQUIRY_PURPOSE};

            caseList = PostCableOwnerCases_Filter(filtes).Result;
            ChecksForCableOwnerCase(caseList);

            var fourthSearchTotalResults = caseList.Count;
            Assert.That(thirdSearchTotalResults, Is.GreaterThanOrEqualTo(fourthSearchTotalResults));
        }

        [Test]
        public void GetCaseByStatusFilter() {
            //Filter on cases recipient status (which should not be done)
            var caseList =
                GetCableOwnerCases_Filter(
                    status: "under_review",
                    limit: 10,
                    caseNumber: CaseNumber)
                    .Result;

            ChecksForCableOwnerCase(caseList);
        }

        [Test]
        public void GetCaseByFilterExtended() {

            //Get organization recipients
            var organizationRecipients = OrganizationRecipients_JSON().Result;

            //Get two random recipients id
            var recipientList = new Guid[2];
            var rnd = new Random(DateTime.Now.Millisecond);
            var num = rnd.Next(organizationRecipients.Count);
            recipientList[0] = new Guid(organizationRecipients[num].Id);
            organizationRecipients.RemoveAt(num);
            if (organizationRecipients.Count > 0) {
                num = rnd.Next(organizationRecipients.Count);
                recipientList[1] = new Guid(organizationRecipients[num].Id);
            }

            var caseList =
                GetCableOwnerCases_Filter(limit: 10,
                    caseNumber: CaseNumber,
                    status: EnumUtils.ToDescription(Constants.Case_Status.TO_CONFIRM),
                    recipientList: recipientList)
                    .Result;

            ChecksForCableOwnerCase(caseList);
        }

        [Test]
        public void GetCasePdf() {
            var task =
                GetCableOwnerCasePdf(_context.OrganizationId, CaseNumber);
            Assert.IsNotNull(task, "The task was null");
            Assert.IsNotNull(task.Result.Value, "There result is empty");

            //Is valid pdf document
            //There is no easy way to check if this is valid pdf document so we will do basic check. 
            //If this is PDF document, in first line it should have PDF version specification.
            //http://stackoverflow.com/questions/3108201/detect-if-pdf-file-is-correct-header-pdf

            var fileBytes = task.Result.Value;
            var result =
                Encoding.UTF8.GetString(
                    fileBytes.Take(7).ToArray());
            Assert.IsTrue(result == "%PDF-1.");

            //Save file as requested in ticket comment
            var fileName = string.IsNullOrWhiteSpace(task.Result.Key)
                               ? CaseNumber + ".pdf"
                               : task.Result.Key;
            using ( var sourceStream = new FileStream(fileName,
                                            FileMode.Append, FileAccess.Write)) {
                sourceStream.Write(fileBytes, 0, fileBytes.Length);
            }
        }

        [Test]
        public void GetCaseGeometries() {

            var coordinateSystem =
                Constants.LkSupportedCoordinateSystems()
                    .Values.OrderBy(x => Guid.NewGuid())
                    .First();
            const string dataFormat = "WKT";

            var task = GetCableOwnerCaseGeometries(_context.OrganizationId,
                CaseNumber,
                coordinateSystem,
                dataFormat);
            
            Assert.IsNotNull(task, "The task was null");
            Assert.IsNotNull(task.Result, "There result is empty");

            //Save file as requested in ticket comment
            using (var sourceStream = new FileStream(task.Result.Key,
                                            FileMode.Append, FileAccess.Write)) {
                sourceStream.Write(task.Result.Value, 0, task.Result.Value.Length);
            }
        }

        [Test]
        public void GetGeometriesAsGisFile() {
            var area = GetArea_JSON().Result.FirstOrDefault();
            Assert.IsNotNull(area, "The area of interest was null");

            var coordinateSystem =
                Constants.LkSupportedCoordinateSystems()
                    .Values.OrderBy(x => Guid.NewGuid())
                    .First();
            const string dataFormat = "WKT";

            var task = GetAreaGeometries(_context.OrganizationId,
                area.Id,
                coordinateSystem,
                dataFormat);

            Assert.IsNotNull(task, "The task was null");
            Assert.IsNotNull(task.Result, "There result is empty");

            //Save file as requested in ticket comment
            using (
                var sourceStream =
                    new FileStream(task.Result.Key,
                        FileMode.Append,
                        FileAccess.Write)) {
                sourceStream.Write(task.Result.Value, 0, task.Result.Value.Length);
            }
        }

        [Test]
        public void GetOrganizationRecipientAsJson() {
            //Get all recipients
            var taskGetAllRecipients =
                OrganizationRecipients_JSON();
            Assert.IsNotNull(taskGetAllRecipients);
            Assert.IsNotNull(taskGetAllRecipients.Result);

            //Checks that the all recipients information are valid
            ChecksForRecipient(taskGetAllRecipients.Result);

            //Get first one
            var firstRecipient = taskGetAllRecipients.Result.FirstOrDefault();
            Assert.IsNotNull(firstRecipient);

            //Get recipient by first recipient id
            var taskGetRecipient =
                OrganizationRecipient_JSON(_context.OrganizationId,
                    firstRecipient.Id);
            Assert.IsNotNull(taskGetRecipient);
            Assert.IsNotNull(taskGetRecipient.Result);

            //Compare two recipients(they should be the same)
            var secondCallRecipient = taskGetRecipient.Result;

            Assert.That(firstRecipient.Id, Is.EqualTo(secondCallRecipient.Id));
            Assert.That(firstRecipient.Name,
                Is.EqualTo(secondCallRecipient.Name));
            Assert.That(firstRecipient.ContactInfo.Address,
                Is.EqualTo(secondCallRecipient.ContactInfo.Address));
            Assert.That(firstRecipient.ContactInfo.PhoneNumber,
                Is.EqualTo(secondCallRecipient.ContactInfo.PhoneNumber));
            Assert.That(firstRecipient.ContactInfo.Email,
                Is.EqualTo(secondCallRecipient.ContactInfo.Email));
            Assert.That(firstRecipient.ContactInfo.FaxNumber,
                Is.EqualTo(secondCallRecipient.ContactInfo.FaxNumber));

            //Checks that the recipient information is valid
            ChecksForRecipient(secondCallRecipient);
        }

        [Test]
        public void GetOrganizationRecipientsAsJson() {
            var task = OrganizationRecipients_JSON();
            Assert.IsNotNull(task);
            Assert.IsNotNull(task.Result);
        }

        [Test]
        public void GetOrganizationRecipientsAsXml() {
            var task = OrganizationRecipients_XML();
            Assert.IsNotNull(task);
            Assert.IsNotNull(task.Result);
        }

        [Test]
        public void PostReply_Test() {

            var caseList = GetCableOwnerCases_JSON(EnumUtils.ToDescription(Constants.Case_Status.TO_CONFIRM), 10);

            var firstCase = caseList.Result.First();

            var involvedRecipient = firstCase.Recipients.First();
            var recipientId = involvedRecipient.Recipient.Id;

            // Create reply
            var reply = CreateReplyObject(firstCase, involvedRecipient);

            var taskGetRecipient = PostReply(firstCase.Id, involvedRecipient.AreaType, recipientId, reply);
            Assert.IsNotNull(taskGetRecipient);
            Assert.IsNotNull(taskGetRecipient.Result);

            ChecksForEvent(taskGetRecipient.Result, reply);
        }

        [Test]
        public void PostReplyToAllCaseTypes_Test() {
            var caseTypes = new[] {
                Constants.COLLABORATION_INQUIRY_PURPOSE,
                Constants.PLANNING_INQUIRY_PURPOSE,
                Constants.PROJECT_INQUIRY_PURPOSE,
                Constants.CABLE_INQUIRY_PURPOSE,
            };

            foreach (var caseType in caseTypes) {

                var caseList = GetCableOwnerCases_Filter(
                    types: new[] {caseType},
                    limit: 1,
                    status: EnumUtils.ToDescription(Constants.Case_Status.TO_CONFIRM));
                var firstCase = caseList.Result.First();

                var involvedRecipient = firstCase.Recipients.First();
                var recipientId = involvedRecipient.Recipient.Id;

                // Create reply
                var reply = CreateReplyObject(firstCase, involvedRecipient);

                var taskGetRecipient = PostReply(firstCase.Id, involvedRecipient.AreaType, recipientId, reply);
                Assert.IsNotNull(taskGetRecipient);
                Assert.IsNotNull(taskGetRecipient.Result);

                ChecksForEvent(taskGetRecipient.Result, reply);
                Console.WriteLine("Case type {0} passed the test", caseType);
            }
        }

        private Reply CreateReplyObject(CableOwnerCase coCase, CableOwnerCaseRecipient caseRecipient) {

            Assert.IsNotNullOrEmpty(coCase.Type, "Case has no type.");
            Assert.IsTrue(
                Constants.MeasuresPerCaseType.ContainsKey(coCase.Type),
                "Case type is not valid.");

            var possibleActions = GetMeasures(caseRecipient.AreaType, coCase.Type).Result;

            //Get random action for case type 
            var rnd = new Random(DateTime.Now.Millisecond);
            var action = possibleActions[rnd.Next(possibleActions.Count)];

            return new Reply {
                Comment =
                    "Testing 1 2 3 - Estas son las mañanitas que cantaba el rey Davíd!" +
                    "A los mångos que se la cräian y ahåra tä le cantamös a tí! ",
                Action = new List<string> {action.Measure}
            };
        }

        [Test]
        public void TestCase_Measurements() {
            var caseTypes = new[] {
                Constants.PurposeEnum.PLANNING,
                Constants.PurposeEnum.PROJECT,
                Constants.PurposeEnum.CABLE_INDICATION,
                Constants.PurposeEnum.COLLABORATE,
                //Constants.PurposeEnum.EMERGENCY,
            };
            var areaTypes = new[] {
                Constants.AreaType.CONTROL,
                Constants.AreaType.AFFECTED,
            };
            foreach (var areaType in areaTypes) {
                foreach (var caseType in caseTypes) {
                    Console.WriteLine("Testing caset type: {0}, area type: {1}", caseType, areaType);
                    var res = GetMeasures(EnumUtils.ToDescription(areaType), EnumUtils.ToDescription(caseType)).Result;
                    Assert.NotNull(res, "areatype {0} with casetype {1} return null", areaType, caseType);
                }
            }
        }


        [Test]
        public void TakeCase_Test() {
            //Get case which requires manual review 
            var caseList = GetCableOwnerCases_JSON(EnumUtils.ToDescription(Constants.Case_Status.TO_CONFIRM), 10);

            //Pick the first case which requires manual review
            var firstCase = caseList.Result.First();
            
            //Pick the first recipient with status manual review
            var firstCaseRecipient = firstCase.Recipients.First(r => r.Status == "requires review");

            //Mark the case as fetched/taken
            var taskTakeCase = TakeCase(firstCase.Id, firstCaseRecipient.AreaType, firstCaseRecipient.Recipient.Id);

            //We should have gotten a result
            Assert.IsNotNull(taskTakeCase);
            Assert.IsNotNull(taskTakeCase.Result);

            //Result should be true
            Assert.IsTrue(taskTakeCase.Result);

            //New status should be under review
            var takenCase = CableOwnerCaseByNumber_JSON(firstCase.Id);
            var takenCaseRecipient = takenCase.Result.Recipients.First(r => r.Recipient.Id == firstCaseRecipient.Recipient.Id);
            Assert.That(takenCaseRecipient.Status, 
                    Is.EqualTo("under review"), 
                    "Recipient case status should be under review");
            
            //Validate that the case is no longer in the toconfirm list
            var toConfirmCaseIds = GetCableOwnerCases_JSON(EnumUtils.ToDescription(Constants.Case_Status.TO_CONFIRM), 10)
                                        .Result.Select(c => c.Id).ToList();

            Assert.That(toConfirmCaseIds.Contains(firstCase.Id), 
                Is.False, "The taken case should not be in the toconfirm list");
        }

        [Test]
        public void TakeCasePerCaseType_Test() {

            var caseTypes = new[] {
                Constants.PLANNING_INQUIRY_PURPOSE,
                Constants.PROJECT_INQUIRY_PURPOSE,
                Constants.CABLE_INQUIRY_PURPOSE,
                Constants.COLLABORATION_INQUIRY_PURPOSE,
            };

            foreach (var caseType in caseTypes) {

                //Get case which requires manual review 
                var caseList = GetCableOwnerCases_Filter(
                    types: new[] {caseType},
                    limit: 5,
                    status: EnumUtils.ToDescription(Constants.Case_Status.TO_CONFIRM));

                //Pick the first case which requires manual review
                var firstCase = caseList.Result.First(c => c.Recipients.Any(r => r.Status == "requires review"));

                //Pick the first recipient with status manual review
                var firstCaseRecipient = firstCase.Recipients.First(r => r.Status == "requires review");

                //Mark the case as fetched/taken
                var taskTakeCase = TakeCase(firstCase.Id, firstCaseRecipient.AreaType, firstCaseRecipient.Recipient.Id);

                //We should have gotten a result
                Assert.IsNotNull(taskTakeCase);
                Assert.IsNotNull(taskTakeCase.Result);

                //Result should be true
                Assert.IsTrue(taskTakeCase.Result);

                //New status should be under review
                var takenCase = CableOwnerCaseByNumber_JSON(firstCase.Id);
                var takenCaseRecipient = takenCase.Result.Recipients.First(r => r.Recipient.Id == firstCaseRecipient.Recipient.Id);
                Assert.That(takenCaseRecipient.Status, Is.EqualTo("under review"),
                            "Recipient case status should be under review. Case was " + firstCase.Id);

                //Validate that the case is no longer in the toconfirm list
                var toConfirmCases =
                    GetCableOwnerCases_JSON(EnumUtils.ToDescription(Constants.Case_Status.TO_CONFIRM), 10)
                        .Result.ToList();

                // assert that the case should not be listed.
                // but if it is, that would mean that case has assigned more than one recipient
                // then check that none of the recipients which have the status "requires review"
                //  is the same recipient which was "taken" before.
                Assert.That(toConfirmCases.Select(cc => cc.Id).Contains(firstCase.Id)
                            && toConfirmCases.Any(ci => (ci.Id == firstCase.Id) &&
                                                        ci.Recipients.Where(cr=> cr.Status.Equals("requires review"))
                                                                     .Any(cr => cr.Recipient.Id == firstCaseRecipient.Recipient.Id
                                                                                && cr.AreaType == firstCaseRecipient.AreaType)),
                            Is.False, "The taken case should not be in the toconfirm list. Case was " + firstCase.Id + 
                                      " and recipient id is: " + firstCaseRecipient.Recipient.Id);

                Console.WriteLine("Case type {0} passed the test", caseType);
            }
        }




        [Test]
        public void GetAttachments() {

            var caseList =
                GetCableOwnerCases_Filter(
                    types: new[] {Constants.PLANNING_INQUIRY_PURPOSE},
                    fromDate: new DateTime(2015, 01, 01),
                    limit: 100).Result;

            var caseWithAttachment =
                caseList.FirstOrDefault(c => c.PlanningInquiry.Attachments.Any());
            if (caseWithAttachment != null) {
                var attachmentName =
                    caseWithAttachment.PlanningInquiry.Attachments.First();

                var taskFile = GetAttachment(caseWithAttachment.Id,
                    HttpUtility.UrlPathEncode(attachmentName),
                    false);
                var taskZip = GetAttachment(caseWithAttachment.Id,
                    HttpUtility.UrlPathEncode(attachmentName),
                    true);

                Assert.IsNotNull(taskFile, "The task was null");
                Assert.IsNotNull(taskFile.Result, "There result is empty");

                Assert.IsNotNull(taskZip, "The task was null");
                Assert.IsNotNull(taskZip.Result, "There result is empty");

                Assert.That(taskFile.Result.Value.Length,
                    Is.GreaterThan(1), "The result is not an attachment.");
                Assert.That(taskZip.Result.Value.Length,
                    Is.GreaterThan(1), "The result is not an attachment.");
                //Save file as requested in ticket comment
                using (var sourceStream = new FileStream(taskFile.Result.Key,
                                                            FileMode.Append, FileAccess.Write)) {
                    sourceStream.Write(taskFile.Result.Value, 0, taskFile.Result.Value.Length);
                }

                using (var sourceStream = new FileStream(taskZip.Result.Key, 
                                                            FileMode.Append, FileAccess.Write)) {
                    sourceStream.Write(taskZip.Result.Value, 0, taskZip.Result.Value.Length);
                }

            } else {
                Assert.Fail(
                    "I can't do a test without knowing which case has an attachment.");
                // I can't do a test without knowing which case has an attachment.
                // so, do nothing
            }
        }
        
        [Test]
        public void GetCasePdfUsingWebRequest() {
            byte[] bytes;
            //var caseId = "20141208-00288";
            try {
                using (var client = new WebClient()) {
 
                    IWebProxy defaultProxy = WebRequest.DefaultWebProxy;
                    if (defaultProxy != null) {
                        defaultProxy.Credentials = CredentialCache.DefaultCredentials;
                        client.Proxy = defaultProxy;
                    }
                    
                    client.BaseAddress = GlobalSettings.Path;
                    client.Headers.Clear();
                    client.Headers.Add("X-Auth-Token", _context.Token);
                    var stream = client.OpenRead(string.Format("organization/{0}/cableownercase/{1}/pdf", _context.OrganizationId, CaseNumber));
                    
                    using (var ms = new MemoryStream()) {
                        var buffer = new byte[16 * 1024];
                        int read;
                        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0) {
                            ms.Write(buffer, 0, read);
                        }

                        //Save file as requested in ticket comment
                        using (var sourceStream = new FileStream(CaseNumber + ".pdf",
                                    FileMode.Append, FileAccess.Write)) {
                            ms.Position = 0;
                            ms.CopyTo(sourceStream);
                        }
                    }
                    
                }

            } catch (Exception ex) {
                Assert.Fail("Error " + ex.Message + " " + ex.StackTrace);
            }
            //return bytes;
        }

        [Test]
        public void GetAllAttachments() {

            var caseList =
                GetCableOwnerCases_Filter(
                    types: new[] {Constants.PLANNING_INQUIRY_PURPOSE},
                    limit: 100).Result;

            var caseWithAttachment =
                caseList.FirstOrDefault(c => c.PlanningInquiry.Attachments.Any());
            if (caseWithAttachment != null) {
                var taskZip = GetAttachment(caseWithAttachment.Id,
                    HttpUtility.UrlPathEncode("all"),
                    false);

                Assert.IsNotNull(taskZip, "The task was null");
                Assert.IsNotNull(taskZip.Result, "There result is empty");

                Assert.That(taskZip.Result.Value.Length,
                    Is.GreaterThan(1),
                    "The result is not an attachment.");
                //Save file as requested in ticket comment

                using (
                    var sourceStream = new FileStream(taskZip.Result.Key, 
                                                        FileMode.Append, FileAccess.Write)) {
                    sourceStream.Write(taskZip.Result.Value, 0, taskZip.Result.Value.Length);
                }

            } else {
                Assert.Fail(
                    "I can't do a test without knowing which case has an attachment.");
                // I can't do a test without knowing which case has an attachment.
                // so, do nothing
            }
        }

        /// <summary>
        /// Loops through the first list and tests if the current case exits in the second list
        /// </summary>
        private void ChecksForCableOwnerCase(
            IEnumerable<CableOwnerCase> leftList,
            IEnumerable<CableOwnerCase> rightList, bool skipFirst = false) {

            if (skipFirst) {
                leftList = leftList.Skip(1).ToList();
            }

            foreach (var c in leftList) {
                Assert.That(rightList.Count(x => x.Id.Equals(c.Id)),
                    Is.EqualTo(1),
                    "The second list does not contain the case " + c.Id +
                    " which is " +
                    "in the first list.");
            }
        }

        /// <summary>
        /// Loops through a list of cases and performs basic checking
        /// </summary>
        private void ChecksForCableOwnerCase(
            IEnumerable<CableOwnerCase> caseList) {
            foreach (var c in caseList) {
                ChecksForCableOwnerCase(c);
            }
        }

        /// <summary>
        /// Basic checking of case
        /// </summary>
        private void ChecksForCableOwnerCase(CableOwnerCase coCase) {
            Assert.IsNotNullOrEmpty(coCase.Id, "Case has no Id. " + coCase.Id);
            if (!coCase.Type.Equals(Constants.EMERGENCY_INQUIRY_PURPOSE)) {
                Assert.IsNotNullOrEmpty(coCase.Name, "Case has no Name. " + coCase.Id);
            } else {
                // An emeregency does not have to have a name.
                // Only through the API is that we allow a name to be set.
            }
            Assert.IsNotNull(coCase.Recipients, "No recipients for this case. " + coCase.Id);
            Assert.GreaterOrEqual(coCase.Recipients.Count,
                1, "No recipients for this case. " + coCase.Id);
        }


        /// <summary>
        /// Loops through a list of recipients and performs basic checking
        /// </summary>
        private void ChecksForRecipient(List<Recipient> recipientList) {
            Assert.Greater(recipientList.Count, 0);
            foreach (var r in recipientList) {
                ChecksForRecipient(r);
            }
        }

        /// <summary>
        /// Basic checking of recipient
        /// </summary>
        private void ChecksForRecipient(Recipient recipient) {
            Assert.IsNotNull(recipient.Id, "The recipient id is null.");
            Assert.IsTrue(recipient.Id != default(Guid).ToString(),
                "The recipient id has default value.");
            Assert.IsNotNullOrEmpty(recipient.Name,
                "The recipient does not have name.");
            Assert.IsNotNull(recipient.ContactInfo,
                "The recipient does not have contact information.");
        }

        /// <summary>
        /// Basic checking of Event
        /// </summary>
        private void ChecksForEvent(Event eventModel, Reply reply) {
            Assert.IsNotNullOrEmpty(eventModel.Comment,
                "The event comment can not be empty.");
            Assert.IsNotNull(eventModel.Time,
                "The event does not have time of creation.");
            Assert.GreaterOrEqual(eventModel.Measure.Count,
                1,
                "The event does not have measures.");
            Assert.AreEqual(reply.Action,
                eventModel.Measure,
                "The event action is not valid.");
        }
    }
}
