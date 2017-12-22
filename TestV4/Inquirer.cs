using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Api.External.Models.V4.Case;
using Api.External.Models.V4.Common;
using Api.External.Models.V4.Recipient;
using ApiClient.Helper;
using ApiClient.Models;
using NUnit.Framework;
using GlobalSettings = ApiClient.Helper.GlobalSettings;

namespace TestV4 {
    [TestFixture]
    public class Inquirer {

        private static Context _context;
        private static string CaseNumber { get; set; }

        private enum ResultType {
            Before,
            After
        }

        private enum StatusType {
            Confirmed,
            Open,
            Canceled,
            Closed
        }

        [SetUp]
        public void Init() {
            // LOGIN
            _context = Authentication.Login(GlobalSettings.UserName_Inq, GlobalSettings.Password_Inq).Result;
            Assert.IsFalse(string.IsNullOrWhiteSpace(_context.Token));

            // GET USER ORGANIZATION ID
            User.GetOrganizationId(_context).Wait();
            Assert.IsFalse(string.IsNullOrWhiteSpace(_context.OrganizationId));

            // GET THE FIRST CASE IN HISTORY WITH ANY RECIPIENTS
            CaseNumber = GetMyOrganizationsInquiries(StatusType.Open).Result.First().Id;
        }

        [TearDown]
        public void Dispose() {
            Authentication.Logout(_context).Wait();

            Assert.IsTrue(string.IsNullOrWhiteSpace(_context.Token));
        }

        private async Task<List<InquirerCase>> GetMyInquiries(
            StatusType? status = null, ResultType? filter = null,
            string caseNumber = "", int limit = 100) {

            List<InquirerCase> result;
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                var url = "/" + limit;
                if (!string.IsNullOrEmpty(caseNumber) && filter.HasValue) {
                    url = "/" + filter.Value.ToString().ToLower() + "/" +
                          caseNumber + "/" + limit;
                }

                if (!string.IsNullOrEmpty(caseNumber) && status.HasValue) {
                    url = "/" + status.Value.ToString().ToLower() + "/" +
                          caseNumber + "/" + limit;

                    if (filter.HasValue) {
                        url = "/" + status.Value.ToString().ToLower() + "/" +
                              filter.ToString().ToLower() + "/" +
                              caseNumber + "/" + limit;
                    }
                } else {
                    if (status.HasValue) {
                        url = string.Format("/{0}", status.Value.ToString().ToLower());
                    }
                }

                // HTTP GET
                var response = await client.GetAsync("user/inquirercase" + url);
                result = response.IsSuccessStatusCode
                    ? JsonUtils<List<InquirerCase>>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;
            }
            return result;
        }


        /// <summary>
        /// Gets the last open cases for this inquirer organization
        /// </summary>
        /// <returns></returns>
        private async Task<List<InquirerCase>> GetMyOrganizationsInquiries(StatusType status) {
            List<InquirerCase> result;

            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                // HTTP GET
                //"organization/{orgid:guid}/inquirercase/{status}/before/{casenr}/{limit:int?}", caseNumber));
                var response = await client
                    .GetAsync(string.Format("organization/{0}/inquirercase/{1}", 
                                            _context.OrganizationId, status));
                result = response.IsSuccessStatusCode
                    ? JsonUtils<List<InquirerCase>>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;
            }
            return result;
        }


        /// <summary>
        /// Check if there's any ubl contact information for the case recipient.
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <param name="recipientId"></param>
        /// <returns></returns>
        private async Task<UblInformation> GetUblInformation(string caseNumber, string recipientId) {
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                var uri =
                    string.Format(
                        "inquirercase/{0}/caserecipient/{1}/ublinformation",
                        caseNumber,
                        recipientId);

                var response = await client.GetAsync(uri);

                return response.IsSuccessStatusCode
                    ? JsonUtils<UblInformation>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;
            }
        }


        /// <summary>
        /// Gets an inquirer case by case number.
        /// </summary>
        /// <returns>InquirerCase</returns>
        public static async Task<InquirerCase> GetInquirerCaseByNumber(string caseNumber, string token) {
            InquirerCase result;

            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", token);

                // HTTP GET
                var response = await client
                    .GetAsync(string.Format("inquirercase/{0}", caseNumber));
                result = response.IsSuccessStatusCode
                    ? JsonUtils<InquirerCase>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;
            }
            return result;
        }

        /// <summary>
        /// Gets an inquirer case by case number.
        /// </summary>
        /// <returns>InquirerCase</returns>
        private async Task<byte[]> GetInquirerCaseByNumberAsPdf(
            string caseNumber = "") {
            //Use default if case number is not provided
            if (string.IsNullOrWhiteSpace(caseNumber))
                caseNumber = CaseNumber;

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
                    .GetAsync(string.Format("inquirercase/{0}/pdf", caseNumber));
                bytes = response.IsSuccessStatusCode
                    ? await response.Content.ReadAsByteArrayAsync()
                    : null;
            }
            return bytes;
        }

        /// <summary>
        /// Posts a cancellation on the case
        /// </summary>
        private async Task<bool> PostCancelCase(CancelCase cancelCase,
            string caseNumber = "") {
            bool result;

            //Use default if case number is not provided
            if (string.IsNullOrWhiteSpace(caseNumber)) {
                caseNumber = CaseNumber;
            }

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
                var jsonString = JsonUtils<CancelCase>.Serialize(cancelCase);
                var stringContent = new StringContent(jsonString);
                stringContent.Headers.ContentType =
                    new MediaTypeHeaderValue("application/json");

                // HTTP POST
                var response = await
                    client.PostAsync(
                        string.Format("inquirercase/{0}/cancel", caseNumber),
                        stringContent);

                result = response.IsSuccessStatusCode;

            }
            return result;
        }

        private CancelCase GetCancelCase(InquirerCase openCase) {
            var possibleReasons = Constants.GetCancelCaseReason(openCase.Type);

            //Get random reason for case type 
            var rnd = new Random(DateTime.Now.Millisecond);
            var reason = possibleReasons[rnd.Next(possibleReasons.Count)];

            return new CancelCase {
                Comment = "Testing",
                Reason = reason
            };
        }

        private CloseCase GetClosingCase(
            InquirerCaseRecipient recipient, string comment) {
            return new CloseCase {
                    ClosingComments = new [] { new ClosingCaseCommentToRecipient {
                    AreaType = Constants.AreaType.AFFECTED.ToString(),
                    Comment = "Här är en första kommentar!",
                    RecipientId = recipient.Recipient.Id
                }, 
                }
            };
        }


        /// <summary>
        /// Gets a list of Inquiry Case Recipinents for which the inquirer can do confirmations
        /// </summary>
        /// <param name="status">toconfirm</param>
        /// <param name="caseNumber">Case number to use(optional)</param>
        /// <returns>List of Inquiry Case Recipinents</returns>
        private async Task<List<InquirerCaseRecipient>>
            GetInquirerCaseRepliesToConfirm(string status, string caseNumber = "") {

            //Use default if case number is not provided
            if (string.IsNullOrWhiteSpace(caseNumber))
                caseNumber = CaseNumber;

            List<InquirerCaseRecipient> result;
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
                        string.Format("inquirercase/{0}/caserecipient/{1}",
                            caseNumber,
                            status));
                result = response.IsSuccessStatusCode
                    ? JsonUtils<List<InquirerCaseRecipient>>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;
            }
            return result;
        }

        /// <summary>
        /// Post method for the inquirer to confirm/accept the answer given by a representative of an organization
        /// This method should be called after the organization has given their final answer.
        /// </summary>
        private async Task<bool> PostInquirerCaseConfirmation(string caseNumber, string recipientId) {
            bool result;
            // Create a client and connect to LK instance
            using (var client = new HttpClient()) {
                // Configure request
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                // HTTP POST
                var response = await
                        client.PostAsync(
                            string.Format("inquirercase/{0}/caserecipient/{1}/confirm",
                                caseNumber,
                                recipientId),
                            new StringContent(""));

                result = response.IsSuccessStatusCode;
            }
            return result;
        }

        /// <summary>
        /// Post method for the inquirer to close the case. 
        /// This should be called once the inquirer has accepted all the answers from the involved organizations.
        /// </summary>
        private async Task<bool> PostInquirerCaseClosing(string caseNumber, CloseCase closingComments = null) {
            bool result;
            // Create a client and connect to LK instance
            using (var client = new HttpClient()) {
                // Configure request
                client.Timeout = TimeSpan.FromSeconds(3000);
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                // check parsing of json, might fail on null.
                var jsonString =
                    JsonUtils<CloseCase>.Serialize(
                        closingComments);

                var stringContent = new StringContent(jsonString);
                stringContent.Headers.ContentType =
                    new MediaTypeHeaderValue("application/json");

                // HTTP POST
                var response =
                    await client.PostAsync(
                            string.Format(
                                "inquirercase/{0}/close",
                                caseNumber),
                            stringContent);

                result = response.IsSuccessStatusCode;
            }
            return result;
        }

        [Test]
        public void GetInquirerCase() {
            var task = GetInquirerCaseByNumber(CaseNumber, _context.Token);
            Assert.IsNotNull(task);
            Assert.IsNotNull(task.Result);

            ChecksForInquirerCase(task.Result);
        }

        [Test]
        public void GetInquirerCaseAsPdf() {
            var task = GetInquirerCaseByNumberAsPdf();
            Assert.IsNotNull(task);
            Assert.IsNotNull(task.Result);

            Assert.IsNotNull(task, "The task was null");
            Assert.IsNotNull(task.Result, "There result is empty");

            //Is valid pdf document
            //There is no easy way to check if this is valid pdf document so we will do basic check. 
            //If this is PDF document, in first line it should have PDF version specification.
            //http://stackoverflow.com/questions/3108201/detect-if-pdf-file-is-correct-header-pdf
            var result =
                Encoding.UTF8.GetString(
                    task.Result.Take(7).ToArray());
            Assert.IsTrue(result == "%PDF-1.");

            //Save file as requested in ticket comment
            using (
                var sourceStream =
                    new FileStream(CaseNumber + ".pdf",
                        FileMode.Append,
                        FileAccess.Write)) {
                sourceStream.WriteAsync(task.Result, 0, task.Result.Length);
            }
        }

        [Test]
        public void GetInquirerCaseReplies() {

            // Status toconfirm -> recipinents for which the inquirer can do confirmations
            string status = EnumUtils.ToDescription(Constants.Case_Status.TO_CONFIRM);

            var task = GetInquirerCaseRepliesToConfirm(status);
            Assert.IsNotNull(task);
            Assert.IsNotNull(task.Result);

            // only run this test if any CO has give an answer to this case.
            if (task.Result.Any()) {
                ChecksForInquirerCaseRecipients(task.Result);
            }

        }

        /// <summary>
        /// As an inquirer, you can only confirm once the answers given by the cable owners.
        /// </summary>
        [Test]
        public void PostInquirerCaseConfirm() {
            // get an open case
            var openCaseNumber = GetMyOrganizationsInquiries(StatusType.Open).Result.First().Id;
            var taskCase = GetInquirerCaseByNumber(openCaseNumber, _context.Token);
            var inqCase = taskCase.Result;

            var recipientsTask = GetInquirerCaseRepliesToConfirm(inqCase.Id);
            var caseRecipients = recipientsTask.Result;
            if (caseRecipients != null && caseRecipients.Any()) {
                // There are answers to confirm =>
                // pick the first recipient and confirm that answer
                var caseRecipient = caseRecipients.First();
                var task = PostInquirerCaseConfirmation(inqCase.Id, caseRecipient.Recipient.Id);
                Assert.IsNotNull(task);
                Assert.IsNotNull(task.Result);
                Assert.IsTrue(task.Result);
            } else {
                // There are no answers to confirm
                // Then I will confirm an answer which has already been confirmed 
                // and I will not be able to do it. The answer should be false
                var caseRecipient = inqCase.InvolvedRecipients.FirstOrDefault() ??
                                    inqCase.InterestedRecipients.First();
                var task = PostInquirerCaseConfirmation(inqCase.Id, caseRecipient.Recipient.Id);

                Assert.IsNotNull(task);
                Assert.IsNotNull(task.Result);
                Assert.IsFalse(task.Result);
            }
        }

        /// <summary>
        /// This test tries to get ubl information of two recipents, one that applies UBL and one which doesn't. 
        /// The recipients get selected from two random cases. 
        /// If the recipients in the case has ubl applied on this case, the GetUblInformation endpoint should return
        /// the information provided.
        /// </summary>
        [Test]
        public void TestUblInformation() {

            //get a random open cases and try to pick one that applies to ubl and one that doesn't.
            var openCases = GetMyOrganizationsInquiries(StatusType.Open).Result;
            var openCasesWithUbl = openCases.Where(c => (c.Type == Constants.COLLABORATION_INQUIRY_PURPOSE) && c.CollaborationInquiry.PublishableUnderUbl);
            var openCasesWithoutUbl = openCases.Where(c => (c.Type == Constants.COLLABORATION_INQUIRY_PURPOSE) && !c.CollaborationInquiry.PublishableUnderUbl);

            //for a case that should contain ubl information
            if (openCasesWithUbl.Any()) {
                var caze = openCasesWithUbl.First();
                foreach (var recipient in caze.InterestedRecipients) {
                   if (recipient.AppliesUbl) {
                       Assert.IsNotNull(GetUblInformation(caze.Id,
                                               recipient.Recipient.Id).Result);
                   } else {
                       // Doing this so in case it fails, we know which organization is the one that is causing this test to fail.
                       var ublInfo = GetUblInformation(caze.Id, recipient.Recipient.Id).Result;
                       var errMsg = "Got info: ";
                       if (ublInfo != null) {
                           errMsg = "Got info: " + ublInfo.OrganizationName +
                                     ublInfo.ApplicationComment;
                       }
                       Assert.IsNull(ublInfo, errMsg);
                   }
                }
            }

            //for a case that shouldn't contain ubl information
            if (openCasesWithoutUbl.Any()) {
                var caze = openCasesWithoutUbl.First();
                foreach (var recipient in caze.InterestedRecipients) {
                    if (recipient.AppliesUbl) {
                        Assert.Fail(string.Format("This fails as Ubl applies for organizaiton {0} and in this test no organizations should have ubl", 
                            recipient.Organization.Name));
                    } else {
                        Assert.IsNull(GetUblInformation(caze.Id,
                                                recipient.Recipient.Id).Result);
                    }
                }
            }
        }

        /// <summary>
        /// Note that we cannot (always) make certain that this test case succeeds due to it requiring 
        /// cases in a certain state (and we don't want to automate the creation of the ideal testing case)
        /// </summary>
        [Test]
        public void PostInquirerCaseClose() {

            // Make sure we're fetching a cable inquiry
            var openCase =
                GetMyOrganizationsInquiries(StatusType.Open).Result.Last();

            // Check if case is eligible to be closed due to passing the thirty day limit.
            bool eligibleCase =
                (Convert.ToDateTime(openCase.Created) - DateTime.Now).TotalDays < 30;

            // Not sure if we should split these, so that we can have a backup case?
            // Or what we should do in case we cant find an eligible case?
            Assert.IsTrue(eligibleCase, "Cant find a case eligible for closing with comments (Less than 30 days old)");
            
            // Get the first recipient from either interested or involved.
            var recipient =
                openCase.InvolvedRecipients.FirstOrDefault() ??
                openCase.InterestedRecipients.FirstOrDefault();

            // Generate some comments!
            var listOfClosedCaseComments = GetClosingCase(recipient, Constants.CLOSING_CASE_COMMENT);

            var closedOrCanceledCases = GetMyInquiries(StatusType.Canceled).Result;
            var canceledCase = closedOrCanceledCases.FirstOrDefault();

            //Try to close closed or canceled case
            if (canceledCase != null) {

                // Need to get separate comments for the recipient of the canceled case.
                var listOfCanceledCaseComments = new CloseCase() {
                    ClosingComments = new [] { new ClosingCaseCommentToRecipient() }
                };

                var task = PostInquirerCaseClosing(canceledCase.Id, listOfCanceledCaseComments);
                Assert.IsNotNull(task);
                Assert.IsNotNull(task.Result);

                //Result should be unsuccessful
                Assert.IsFalse(task.Result);
            }

            // Close open case
            if (openCase != null) {

                var recipientsLeftToConfirm = new List<InquirerCaseRecipient>();

                if (openCase.InvolvedRecipients.Any()) {

                    // Cannot get any replies to confirm unless the case has any?
                    var taskCofirmed = GetInquirerCaseRepliesToConfirm(
                                        EnumUtils.ToDescription(Constants.Case_Status.TO_CONFIRM),
                                        openCase.Id);
                    Assert.IsNotNull(taskCofirmed);
                    Assert.IsNotNull(taskCofirmed.Result);
                    recipientsLeftToConfirm = taskCofirmed.Result;
                }
                
                if ((recipientsLeftToConfirm != null && recipientsLeftToConfirm.Any())
                    || !openCase.InvolvedRecipients.TrueForAll(ir => ir.Confirmed)) {
                    // you can not close this case
                    // can not do anything nothing more
                } else {
                    // woohoo, you can close this case.

                    var task = PostInquirerCaseClosing(openCase.Id, listOfClosedCaseComments);
                    Assert.IsNotNull(task);
                    Assert.IsNotNull(task.Result);

                    //Result should be successful
                    Assert.IsTrue(task.Result);

                    //Get case and check if it's closed
                    var inquirerCaseTask = GetInquirerCaseByNumber(openCase.Id, _context.Token);
                    Assert.IsNotNull(inquirerCaseTask);
                    Assert.IsNotNull(inquirerCaseTask.Result);
                    Assert.That(inquirerCaseTask.Result.Status.ToLower(), Is.EqualTo("closed"));
                }
            }

        }

        [Test]
        public void PostInquirerCancelCase_Test() {

            //Get open case
            var openCase = GetMyInquiries().Result.FirstOrDefault();

            Assert.IsNotNull(openCase);
            Assert.IsNotNullOrEmpty(openCase.Type, "Case has no type.");

            // Create CancelCase object
            var cancelCase = GetCancelCase(openCase);

            var postCancelCaseTask = PostCancelCase(cancelCase, openCase.Id);
            Assert.IsNotNull(postCancelCaseTask);
            Assert.IsNotNull(postCancelCaseTask.Result);
            Assert.IsTrue(postCancelCaseTask.Result);
        }

        [Test]
        public void GetInquirerCase_ForOrganization() {

            const StatusType caseStatus = StatusType.Open;
            //Get open case
            var orgCases = GetMyOrganizationsInquiries(caseStatus).Result;

            var lastCase = orgCases.Last();

            var myCases = GetMyInquiries(caseStatus, ResultType.After, lastCase.Id).Result;

            // myCases should be contained within the organization cases
            Assert.That(myCases.Count, Is.LessThanOrEqualTo(orgCases.Count));

            var organizationIds = orgCases.Select(c => c.Id).ToList();
            var inquiryIds = myCases.Select(c => c.Id).ToList();
            foreach (var id in inquiryIds) {
                Assert.That(organizationIds.Contains(id), Is.True, 
                        "Case not in the list: " + id);
            }

        }

        /// <summary>
        /// Basic checking of InquirerCase
        /// </summary>
        private void ChecksForInquirerCase(InquirerCase inquirerCase) {
            Assert.IsNotNullOrEmpty(inquirerCase.Id, "Inquirer case has no Id.");
            Assert.IsNotNull(inquirerCase.Inquirer, "No inquirer for this inquirer case");
            Assert.IsNotNullOrEmpty(inquirerCase.Inquirer.Id, "Inquirer has no Id for this inquirer case.");
        }


        /// <summary>
        /// Loops through a list of inquirer case recipients and performs basic checking
        /// </summary>
        private void ChecksForInquirerCaseRecipients(
            List<InquirerCaseRecipient> recipientList) {
            Assert.Greater(recipientList.Count, 0);
            foreach (var r in recipientList) {
                ChecksForInquirerCaseRecipient(r);
            }
        }

        /// <summary>
        /// Basic checking of InquirerCaseRecipient
        /// </summary>
        private void ChecksForInquirerCaseRecipient(
            InquirerCaseRecipient inquirerCaseRecipient) {
            Assert.IsNotNull(inquirerCaseRecipient.Organization,
                "No organization for this inquirer case.");
            Assert.IsNotNullOrEmpty(inquirerCaseRecipient.Organization.Id,
                "Organization has no Id for this inquirer case.");
            Assert.IsNotNull(inquirerCaseRecipient.Recipient,
                "No recipients for this inquirer case.");
            Assert.IsNotNullOrEmpty(inquirerCaseRecipient.Recipient.Id,
                "Recipient has no Id for this inquirer case.");
        }

    }
}
