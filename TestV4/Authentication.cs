using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Context = ApiClient.Models.Context;
using GlobalSettings = ApiClient.Helper.GlobalSettings;

namespace TestV4 {
    [TestFixture]
    public static class Authentication {

        [Test]
        public static void TestLoginLogOut() {
            var context = Login().Result;
            Assert.IsNotNull(context, "The context after logging in is null.");
            Assert.That(string.IsNullOrWhiteSpace(context.Token), Is.False, "The token has not been set.");

            Logout(context).Wait();
            Assert.That(context.Token, Is.Null,
                "Token after logout has not been cleared.");
        }

        public static async Task<Context> Login(string userName = null, string password = null) {
            var context = new Context();
            using (var client = new HttpClient()) {

                var user = string.IsNullOrEmpty(userName) ? GlobalSettings.UserName_Co : userName ;
                var pwd = string.IsNullOrEmpty(password) ? GlobalSettings.Password_Co : password;

                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Create encoded credentials 
                var credentials = user + ":" + pwd;
                var bCredentials = Encoding.GetEncoding("ISO-8859-1").GetBytes(credentials);
                var base64Credentials = Convert.ToBase64String(bCredentials);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                //Set Authorization header and encoded credentials
                client.DefaultRequestHeaders.Add("Authorization",
                    "Basic " + base64Credentials);

                // HTTP POST
                var response = await client.PostAsync("auth", new StringContent(""));

                if (!response.IsSuccessStatusCode) return null;

                context.Token = await response.Content.ReadAsStringAsync();
            }
            return context;
        }

        public static async Task Logout(Context context) {
            // If you are not logged in, then call the login method first
            if (string.IsNullOrWhiteSpace(context.Token)) {
                context = await Login();
            }

            using (var client = new HttpClient()) {

                //Set timeout to 20 seconds
                client.Timeout = TimeSpan.FromSeconds(20);

                //Configure request
                client.BaseAddress = new Uri(GlobalSettings.Path);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("X-Auth-Token", context.Token);

                // HTTP DELETE
                var response = await client.DeleteAsync("auth");
                if (response.IsSuccessStatusCode) {
                    //User is logged out
                    context.Token = null;
                }
            }
        }
    }
}