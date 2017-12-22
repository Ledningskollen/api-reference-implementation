using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ApiClient.Helper;
using ApiClient.Models;
using NUnit.Framework;
using GlobalSettings = ApiClient.Helper.GlobalSettings;

namespace TestV4 {
    [TestFixture]
    public class User {

        private static Context _context;

        [SetUp]
        public void Init() {
            // LOGIN
            _context = Authentication.Login().Result;
            Assert.That(string.IsNullOrWhiteSpace(_context.Token), Is.False, 
                        "The token has not been set after logging in.");
        }

        [TearDown]
        public void Dispose() {
            Authentication.Logout(_context).Wait();
            Assert.That(_context.Token, Is.Null, 
                        "Token after logout has not been cleared.");
        }

        [Test]
        public void TestOrganizationId() {
            Assert.That(string.IsNullOrWhiteSpace(_context.Token), Is.False, 
                        "The token has not been set after logging in.");




            GetOrganizationId(_context).Wait();
            
            Assert.That(string.IsNullOrWhiteSpace(_context.OrganizationId), Is.False, 
                        "The organization Id has not been set.");
            Guid orgGuid;
            Assert.That(Guid.TryParse(_context.OrganizationId, out orgGuid), Is.True,
                        "The conversion for the organization Id to GUID did not work.");
            Assert.IsNotNull(orgGuid, "The organization Id is not a guid.");
        }
        
        public static async Task<string> GetOrganizationId(Context context) {
            if (context == null) {
                context = _context;
            }
            var result = "";
            using (var client = new HttpClient()) {
                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Add("X-Auth-Token", context.Token);

                // HTTP POST
                var response = await client.GetAsync("user/organization");
                Assert.IsTrue(response.IsSuccessStatusCode, "Organization call failed.");
                if (!response.IsSuccessStatusCode) return result;

                result = await response.Content.ReadAsStringAsync();
                Assert.That(string.IsNullOrWhiteSpace(result), Is.False,
                            "The organization Id was not resturned.");

                context.OrganizationId = JsonUtils<List<String>>.Deserialize(
                        result).FirstOrDefault();
            }
            return context.OrganizationId;
        }
    }
}
