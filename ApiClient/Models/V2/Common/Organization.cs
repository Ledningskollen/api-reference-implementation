using Api.Interfaces.V2.Common;

namespace ApiClient.Models.V2.Common {
    /// <summary>
    /// This model represents an organization
    /// </summary>
    public class Organization: IOrganization {
        public string Id { get; set; }
        public string Name { get; set; }
        public string OrganizationNumber { get; set; }
    }
}