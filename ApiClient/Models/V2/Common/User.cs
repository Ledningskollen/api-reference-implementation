using Api.Interfaces.V2.Common;

namespace ApiClient.Models.V2.Common {
    /// <summary>
    /// This model represents a base for user information
    /// </summary>
    public class User : IUser {
        public string Id { get; set; }
        public IContactInfo ContactInfo { get; set; }
        public IOrganization Organization { get; set; }
    }
}