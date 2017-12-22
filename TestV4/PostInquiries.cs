using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Api.External.Models.V4.Common;
using Api.External.Models.V4.Inquiry;
using Api.External.Models.V4.Recipient;
using ApiClient.Helper;
using ApiClient.Models;
using NUnit.Framework;
using GlobalSettings = ApiClient.Helper.GlobalSettings;

namespace TestV4 {
    [TestFixture]
    public class PostInquiries {
        private static Context _context;

        // 625m square in Gothenburg
        private readonly string _geojson = 
            "{\"type\":\"Polygon\",\"coordinates\":[[[6401470.1824371638,321765.78432757215],[6401465.8308711154,321790.4632852918],[6401441.199078504,321786.12003563583],[6401445.5506445458,321761.4410779092],[6401470.1824371638,321765.78432757215]]]}"
        ;

        // same geometry as the one above, but since we use Feature collections instead of just geojson, here is its representation
        private const string _featureCollection = "{\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[[6401470.1824371638,321765.78432757215],[6401465.8308711154,321790.4632852918],[6401441.199078504,321786.12003563583],[6401445.5506445458,321761.4410779092],[6401470.1824371638,321765.78432757215]]]},\"properties\":{\"CreatedWith\":\"WEBSERVICE\"}}],\"type\":\"FeatureCollection\"}";

        private const string _crsFeatureCollection =
                @"{""type"":""FeatureCollection"",""features"":[{""type"":""Feature"",""properties"":{""crs"":4326},""geometry"":{""type"":""Polygon"",""coordinates"":[[[11.962287425994873,57.70461728172189],[11.96271926164627,57.70461728172189],[11.96271926164627,57.70482220900525],[11.962287425994873,57.70482220900525],[11.962287425994873,57.70461728172189]]]}}]}";

        [SetUp]
        public void Init() {
            // LOGIN
            _context = Authentication.Login(GlobalSettings.UserName_Inq, GlobalSettings.Password_Inq).Result;
            Assert.IsFalse(string.IsNullOrWhiteSpace(_context.Token));

            //// GET USER ORGANIZATION ID
            //var organizationId = User.GetOrganizationId(_context).Result;
            //Assert.IsFalse(string.IsNullOrWhiteSpace(_context.OrganizationId));
        }

        [TearDown]
        public void Dispose() {
            Authentication.Logout(_context).Wait();
            Assert.IsTrue(string.IsNullOrWhiteSpace(_context.Token));
        }

        private async Task<List<int>> PostAttachments(
            Dictionary<string, byte[]> attachments) {

            List<int> result;

            using (var client = new HttpClient()) {
                // Configure request
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // Configure request body
                var jsonString =
                    JsonUtils<Dictionary<string, byte[]>>.Serialize(attachments);
                var stringContent = new StringContent(jsonString);
                stringContent.Headers.ContentType =
                    new MediaTypeHeaderValue("application/json");

                // HTTP POST
                var response = await
                    client.PostAsync("inquiry/attachments", stringContent);

                result = JsonUtils<List<int>>.Deserialize(
                    response.Content.ReadAsStringAsync().Result,
                    response.IsSuccessStatusCode);
            }

            return result;
        }

        /// <typeparam name="TInput">
        /// Generic type. When you use the method you are 
        /// forced to use a class that are inherited from Inquiry.
        /// </typeparam>
        private async Task<List<InquirerCaseRecipient>> GetInvolvedRecipients
            <TInput>(
            TInput inquiry)
            where TInput : Inquiry {

            List<InquirerCaseRecipient> result;
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
                var jsonString = JsonUtils<TInput>.Serialize(inquiry);
                var stringContent = new StringContent(jsonString);
                stringContent.Headers.ContentType =
                    new MediaTypeHeaderValue("application/json");

                // HTTP POST
                var url = string.Format("inquiry/{0}/involvedrecipients",
                    typeof (TInput).Name.ToLower());
                var response = await client.PostAsync(url, stringContent);

                result = JsonUtils<List<InquirerCaseRecipient>>.Deserialize(
                        response.Content.ReadAsStringAsync().Result, 
                        response.IsSuccessStatusCode);

            }
            return result;
        }

        /// <typeparam name="TOutput">
        /// Generic type. When you use the method you are 
        /// forced to use a class that are inherited from Inquiry.
        /// </typeparam>
        /// <typeparam name="TInput">
        /// Generic type. When you use the method you are 
        /// forced to use a class that are inherited from CreateInquiry.
        /// </typeparam>
        private async Task<List<string>> PostInquiry<TOutput, TInput>(
            TInput inquiry)
            where TInput : CreateInquiry {

            List<string> result;
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
                var jsonString = JsonUtils<TInput>.Serialize(inquiry);
                var stringContent = new StringContent(jsonString);
                stringContent.Headers.ContentType =
                    new MediaTypeHeaderValue("application/json");

                var url = string.Format("inquiry/{0}",
                    typeof (TOutput).Name.ToLower());
                // HTTP POST
                var response = await client.PostAsync(url, stringContent);

                result = JsonUtils<List<string>>.Deserialize(
                    response.Content.ReadAsStringAsync().Result, 
                    response.IsSuccessStatusCode);
            }
            return result;
        }


        [Test]
        public void GetInvovlvedRecipientsForCaseArea() {
            var cableInquiryRecipients =
                GetInvolvedRecipients(new CableInquiry {
                    Geometry = _featureCollection
                }).Result;


            var collaborationInquiryRecipients =
                GetInvolvedRecipients(new CollaborationInquiry {
                    Geometry = _featureCollection
                }).Result;

            var emergencyInquiryRecipients =
                GetInvolvedRecipients(new EmergencyInquiry {
                    Geometry = _featureCollection
                }).Result;

            var planningInquiryRecipients =
                GetInvolvedRecipients(new PlanningInquiry {
                    Geometry = _featureCollection
                }).Result;

            var projectInquiryRecipients =
                GetInvolvedRecipients(new ProjectInquiry {
                    Geometry = _featureCollection
                }).Result;

            Assert.That(cableInquiryRecipients.Count,
                Is.GreaterThanOrEqualTo(2),
                "There should be two case recipients.");

            //No collaboration inquiry recipients for areas of interest
            Assert.That(collaborationInquiryRecipients.Count,
                Is.GreaterThanOrEqualTo(1),
                "There should be one case recipients.");

            Assert.That(emergencyInquiryRecipients.Count,
                Is.GreaterThanOrEqualTo(2),
                "There should be two case recipients.");

            Assert.That(planningInquiryRecipients.Count,
                Is.GreaterThanOrEqualTo(4),
                "There should be four case recipients.");

            Assert.That(projectInquiryRecipients.Count,
                Is.GreaterThanOrEqualTo(2),
                "There should be two case recipients.");
        }

        private List<int> AddAttachmentFile() {
            var path = Environment.CurrentDirectory;
            var dir = new DirectoryInfo(path.Substring(0,
                path.IndexOf("bin", StringComparison.Ordinal)) +
                                        "TestingFiles\\");
            var post = dir.GetFiles("*.*")
                .ToDictionary(x => x.Name, x => File.ReadAllBytes(x.FullName));

            var attachments = PostAttachments(post).Result;
            
            Assert.That(attachments.Count,
                Is.GreaterThanOrEqualTo(1),
                "There should be one attachment.");

            return attachments;
        }


        [Test]
        public void PostAttachments() {

            var attachments = AddAttachmentFile();

            var cableInquiryRecipients =
                GetInvolvedRecipients(new CableInquiry {
                    Geometry = _featureCollection
                }).Result;

            Assert.That(cableInquiryRecipients.Count,
                Is.GreaterThanOrEqualTo(2),
                "There should be two case recipients.");

            var categories = GetWorkCategories(Constants.CABLE_INQUIRY_PURPOSE).Result;
            var random = new Random();
            var category = categories[random.Next(categories.Count)];

            var cableInquiry =
                PostInquiry<CableInquiry, CreateCableInquiry>(
                    new CreateCableInquiry {
                        Attachments = attachments, // optional
                        ExemptedInvolvedOrganzations = new List<string> {
                            cableInquiryRecipients[0].Organization.Id
                        }, // optional
                        Inquiry = new CableInquiry {
                            // purpose is set in constructor
                            Name = "Testing adding case through API",
                            Comment = "Testing adding case through API",
                            // optional
                            Geometry = _featureCollection,
                            StartDate = DateTime.Today.AddDays(1).ToString("O"),
                            // must be at least one day in the future
                            EndDate = DateTime.Today.AddDays(31).ToString("O"),
                            // must be less than 31 days from start date
                            WorkMethods = new List<string> {
                                CableInquiry.AllowedWorkTypes.First()
                            },
                            WorkCategory = new List<string> { category.UniqueName },
                            // must be at least one
                            ExcavationDepth = 1, // greater than 0
                            PreferedContactWay = Constants.CONTACT_WAY_FAX,
                            PreferedContactValue = "0132166506",
                            SiteContactName = "Arne Testperson",
                            SiteContactPhone = "+46 123 321123",
                            MeetUpAddress = new MeetUpAddress {
                                CityName = "Göteborg",
                                PostCode = "41668",
                                StreetNameAndNumber = "Svangatan 19"
                            },
                            PropertyDesignation = "FSAS 123:01", // optional
                            NotifyOnReply = true, // optional
                            NotificationViaSms = "+46 123 321123"
                            // Mobile number, requiried if NotifyOnReply is true
                        }
                    }).Result;

            Assert.IsNotNull(cableInquiry);
            Assert.IsNotNull(cableInquiry.FirstOrDefault());
            Assert.IsNotNullOrEmpty(cableInquiry.First());
            CheckForNewCase(cableInquiry.First());
        }

        public async Task<List<WorkCategory>> GetWorkCategories(string caseType) {
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", _context.Token);

                var uri = string.Format("inquiry/workCategories/{0}", caseType);

                var response = await client.GetAsync(uri);

                return response.IsSuccessStatusCode
                    ? JsonUtils<List<WorkCategory>>.Deserialize(
                        response.Content.ReadAsStringAsync().Result)
                    : null;
            }
        }

        [Test]
        public void CreateCableInquiryCase() {
            var categories = GetWorkCategories(Constants.CABLE_INQUIRY_PURPOSE).Result;
            var random = new Random();
            var category = categories[random.Next(categories.Count)];
            
            var res =
                PostInquiry<CableInquiry, CreateCableInquiry>(
                    new CreateCableInquiry {
                        Attachments = null,
                        ExemptedInvolvedOrganzations = null,
                        Inquiry = new CableInquiry {
                            // purpose is set in constructor
                            Name =
                                "Testing adding cable indication case through API",
                            Comment =
                                "Testing adding cable indication case through API",
                            // optional
                            Geometry = _featureCollection,
                            StartDate = DateTime.Today.AddDays(1).ToString("O"),
                            // must be at least one day in the future
                            EndDate = DateTime.Today.AddDays(31).ToString("O"),
                            // must be less than 31 days from start date
                            WorkMethods = new List<string> {
                                CableInquiry.AllowedWorkTypes.First()
                            }, // must be at least one
                            ExcavationDepth = 1, // greater than 0
                            PreferedContactWay = Constants.CONTACT_WAY_EMAIL,
                            PreferedContactValue = "ugga.bugga@snugga.com",
                            SiteContactName = "Arne Testperson",
                            SiteContactPhone = "112345649",
                            MeetUpAddress = new MeetUpAddress {
                                CityName = "Göteborg",
                                PostCode = "41668",
                                StreetNameAndNumber = "Svangatan 19"
                            },
                            PropertyDesignation = "FSAS 123:01", // optional
                            NotifyOnReply = true, // optional
                            NotificationViaSms = "+46 123 321123",
                            // Mobile number, requiried if NotifyOnReply is true
                            WorkCategory = new List<string> { category.UniqueName }
                        }
                    });

            var cableCase = res.Result;

            Assert.IsNotNull(cableCase);
            Assert.IsNotNull(cableCase.FirstOrDefault());
            Assert.IsNotNullOrEmpty(cableCase.First());
            CheckForNewCase(cableCase.First());
        }

        [Test]
        public void TestGetWorkCategories() {

            var caseTypesWithWorkCategories = new[] {
                Constants.PROJECT_INQUIRY_PURPOSE,
                Constants.CABLE_INQUIRY_PURPOSE,
            };

            var caseTypesWithoutWorkCategories = new[] {
                Constants.COLLABORATION_INQUIRY_PURPOSE,
                Constants.PLANNING_INQUIRY_PURPOSE
            };

            // These case types should have work categories
            foreach (var caseType in caseTypesWithWorkCategories) {
                var allCategories = GetWorkCategories(caseType).Result;

                // Make sure the categories are created in a proper way following the pattern in the Model.
                var duplicates = allCategories.Where(d =>
                        d.UniqueName.Equals(string.Format("{0}_{1}", d.Name.ToUpper(), d.Name.ToUpper())));
                
                Assert.That(!duplicates.Any());   
            
                // Try some specific cases
                Assert.That(
                    allCategories.Any(
                        wk => wk.UniqueName.Equals("1_1_VA")));
                Assert.That(
                   allCategories.Any(
                       wk => wk.UniqueName.Equals("2_3_BOSTÄDER")));
            }

            // Work categories should not be available for these case types.
            foreach (var caseType in caseTypesWithoutWorkCategories) {
                var allCategories = GetWorkCategories(caseType).Result;
                Assert.IsEmpty(allCategories);
            }

        }

        [Test]
        public void CreateCollaborationCase() {

            var collaborationInquiryRecipients =
                GetInvolvedRecipients(new CollaborationInquiry {
                    Geometry = _featureCollection
                }).Result;

            Assert.That(collaborationInquiryRecipients.Count,
                Is.GreaterThanOrEqualTo(2),
                "There should be two case recipients.");

            var collaborationCase =
                PostInquiry<CollaborationInquiry, CreateCollaborationInquiry>(
                    new CreateCollaborationInquiry {
                        Attachments = null,
                        ExemptedInvolvedOrganzations = null,
                        ExemptedInterestedOrganzations = new List<string>() {
                            collaborationInquiryRecipients[0].Organization.Id
                        },
                        Inquiry = new CollaborationInquiry {
                            // purpose is set in constructor
                            Name = "Testing collaboration case (API) - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                            Comment = "Testing adding collaboration case through API",
                            Geometry = _featureCollection,
                            StartDate = "", // set automaticly
                            EndDate = "", // set automaticly
                            WorkMethods = new List<string> {
                                CollaborationInquiry.AllowedWorkTypes.First()
                            }, // must be at least one
                            ExcavationDepth = 1, // optional
                            ExcavationWidth = 2, // optional
                            Milestones = new List<Milestone> { // milestones can be empty
                                new Milestone{ Date = "2014-01-01", Description = "Action 1"},
                                new Milestone{ Date = "2014-02-01", Description = "Action 2"},
                                new Milestone{ Date = "2014-05-19", Description = "Action 3"}
                            },
                            // Mobile number, requiried if NotifyOnReply is true
                            NotifyOnReply = true, // optional
                            NotificationViaSms = "+46 123 321123",
                            ProjectStartDate = DateTime.Today.AddDays(1).ToString("O"),
                            ProjectEndDate = DateTime.Today.AddDays(31).ToString("O"),
                            // no need for this ones 
                            //PreferedContactWay = Constants.CONTACT_WAY_EMAIL,
                            //PreferedContactValue = "olle.bolle@golle.com"
                        }
                    }).Result;

            Assert.IsNotNull(collaborationCase);
            Assert.IsNotNull(collaborationCase.FirstOrDefault());
            Assert.IsNotNullOrEmpty(collaborationCase.First());
            CheckForNewCase(collaborationCase.First());
        }

        [Test]
        public void CreateEmergencyCase() {
            var emergencyCase =
                PostInquiry<EmergencyInquiry, CreateEmergencyInquiry>(
                    new CreateEmergencyInquiry {
                        Attachments = null,
                        ExemptedInvolvedOrganzations = null,
                        Inquiry = new EmergencyInquiry {
                            // An emeregency does not have to have a name.
                            // Only through the API is that we allow a name to be set.
                            Name = "Testing adding emergency case through API",
                            Comment =
                                "Testing adding emergency case through API",
                            Geometry = _featureCollection,
                            StartDate = DateTime.Now.AddHours(4).ToString("O"),
                            // must be at least 3 hours from now, remember to include time to post the case
                            EndDate = DateTime.Now.AddHours(10).ToString("O"),
                            // must be less than 21 days from now
                            ContactName = "Arne testperson",
                            ContactInformation = new ContactInfo {
                                PhoneNumber = "07079754131"
                                // Either Phonenumber or email
                            }
                        }
                    }).Result;

            Assert.IsNotNull(emergencyCase);
            Assert.IsNotNull(emergencyCase.FirstOrDefault());
            Assert.IsNotNullOrEmpty(emergencyCase.First());
            CheckForNewCase(emergencyCase.First());
        }

        [Test]
        public void CreatePlanningCase() {
            var attachments = AddAttachmentFile();

            var planningCase =
                PostInquiry<PlanningInquiry, CreatePlanningInquiry>(
                    new CreatePlanningInquiry {
                        Attachments = attachments,
                        ExemptedInvolvedOrganzations = null,
                        Inquiry = new PlanningInquiry {
                            Name = "Testing adding planning case through API",
                            Comment = "Testing adding planning case through API",
                            Geometry = _featureCollection,
                            WorkMethods = new List<string> {
                                PlanningInquiry.AllowedWorkTypes.First()
                            }, // must be at least one
                            StartDate = "", // set automaticly
                            EndDate = "", // set automaticly
                            ProjectStartDate = DateTime.Today.AddDays(1).ToString("O"),
                            ProjectEndDate = DateTime.Today.AddDays(31).ToString("O"),
                            Milestones = new List<Milestone> { // milestones can be empty
                                new Milestone{ Date = "2014-05-19", Description = "Action 3"},
                                new Milestone{ Date = "2015-01-01", Description = "Final action 1"},
                            },
                            // gis file format and the file coordinate system can also be emtpy
                            GisFileFormat = Constants.LkSupportedGisFileFormats().Last(),
                            FileCoordinateSystem = Constants.LkSupportedCoordinateSystems().Keys.First(),
                            NotifyOnReply = true,
                            // for this case type, the only allowed option is email
                            PreferedContactWay = Constants.CONTACT_WAY_EMAIL,
                            PreferedContactValue = "olle.bolle@golle.com"
                        }
                    }).Result;

            Assert.IsNotNull(planningCase);
            Assert.IsNotNull(planningCase.FirstOrDefault());
            Assert.IsNotNullOrEmpty(planningCase.First());
            CheckForNewCase(planningCase.First());
        }

        [Test]
        public void CreateProjectCase() {
            var categories = GetWorkCategories(Constants.CABLE_INQUIRY_PURPOSE).Result;
            var random = new Random();
            var category = categories[random.Next(categories.Count)];

            var projectInquiryRecipients =
                GetInvolvedRecipients(new ProjectInquiry {
                    Geometry = _featureCollection
                }).Result;

            var removeInvolvedOrgs = new List<string>();
            var totalAffectedOrgs = projectInquiryRecipients.Count(cr => cr.AreaType == "affected");
            if (totalAffectedOrgs > 1) { // so at least 2 => remove half
                var caseRecipients = projectInquiryRecipients.Where(cr => cr.AreaType == "affected");
                var index = 0;
                foreach (var cr in caseRecipients) {
                    if (index%2 == 0) {
                        removeInvolvedOrgs.Add(cr.Organization.Id);
                    }
                    index++;
                }
            }

            var removeInterestedOrgs = new List<string>();
            var totalControlOrgs = projectInquiryRecipients.Count(cr => cr.AreaType == "control");
            if (totalControlOrgs > 1) { // so at least 2 => remove half
                var caseRecipients = projectInquiryRecipients.Where(cr => cr.AreaType == "control");
                var index = 0;
                foreach (var cr in caseRecipients) {
                    if (index%2 == 0) {
                        removeInterestedOrgs.Add(cr.Organization.Id);
                    }
                    index++;
                }
            }
            var attachments = AddAttachmentFile();

            var projectCase =
                PostInquiry<ProjectInquiry, CreateProjectInquiry>(
                    new CreateProjectInquiry {
                        Attachments = attachments,
                        ExemptedInvolvedOrganzations = removeInvolvedOrgs,
                        ExemptedInterestedOrganzations = removeInterestedOrgs,
                        Inquiry = new ProjectInquiry {
                            Name = "Testing adding project case through API",
                            Comment =
                                "Testing adding project case through API",
                            Geometry = _featureCollection,
                            ProjectStartDate = DateTime.Today.AddDays(1).ToString("O"),
                            ProjectEndDate = DateTime.Today.AddDays(31).ToString("O"),
                            NotifyOnReply = true,
                            NotificationViaSms = "+46 123 321123",
                            PreferedContactValue = "olle.bolle@golle.com",
                            PreferedContactWay = Constants.CONTACT_WAY_EMAIL,
                            // The properties ReplyAsPdf, GisFileFormat and FileCoordinateSystem are only
                            // valid/set/saved if the prefered contact way (PreferedContactWay) is email.
                                ReplyAsPdf = true,
                                // GisFileFormat and FileCoordinateSystem can also be emtpy
                                GisFileFormat = Constants.LkSupportedGisFileFormats().Last(),
                                FileCoordinateSystem = Constants.LkSupportedCoordinateSystems().Keys.First(),
                            ExcavationDepth = 1,
                            WorkCategory = new List<string> { category.UniqueName }
                        }
                    }).Result;

            Assert.IsNotNull(projectCase);
            Assert.IsNotNull(projectCase.FirstOrDefault());
            Assert.IsNotNullOrEmpty(projectCase.First());
            CheckForNewCase(projectCase.First());

            // doing extra checks, so the number of existing involved and interested orgs are as expected
            var task = Inquirer.GetInquirerCaseByNumber(projectCase.First(), _context.Token);
            Assert.That(task.Result.InvolvedRecipients.Count > 0);
            Assert.That(task.Result.InterestedRecipients.Count > 0);
        }

        private void CheckForNewCase(string caseNumber) {

            var task = Inquirer.GetInquirerCaseByNumber(caseNumber, _context.Token);

            Assert.IsNotNull(task);
            Assert.IsNotNull(task.Result);

            switch (task.Result.Type) {
                case Constants.CABLE_INQUIRY_PURPOSE:
                    ChecksForCableOwnerCase(task.Result.CableInquiry);
                    break;
                case Constants.PROJECT_INQUIRY_PURPOSE:
                    ChecksForCableOwnerCase(task.Result.ProjectInquiry);
                    break;
                case Constants.COLLABORATION_INQUIRY_PURPOSE:
                    ChecksForCableOwnerCase(task.Result.CollaborationInquiry);
                    break;
                case Constants.PLANNING_INQUIRY_PURPOSE:
                    ChecksForCableOwnerCase(task.Result.PlanningInquiry);
                    break;
                case Constants.EMERGENCY_INQUIRY_PURPOSE:
                    ChecksForCableOwnerCase(task.Result.EmergencyInquiry);
                    break;
            }
        }

        private static void ChecksForCableOwnerCase<TInput>(TInput inquiry)
            where TInput : Inquiry {

            Assert.That(!string.IsNullOrEmpty(inquiry.Name),
                Is.True, "Inquiry has no name.");
            Assert.That(!string.IsNullOrEmpty(inquiry.Comment),
                Is.True, "Inquiry has no comment.");
            Assert.That(!string.IsNullOrEmpty(inquiry.Purpose),
                Is.True, "Inquiry has no purpose.");
            Assert.That(!string.IsNullOrEmpty(inquiry.CreatedUsing),
                Is.True, "Inquiry has no createdusing.");
            Assert.That(inquiry.Geometry != null && inquiry.Geometry.Any(),
                Is.True, "Inquiry has no geometries.");
        }
    }
}
