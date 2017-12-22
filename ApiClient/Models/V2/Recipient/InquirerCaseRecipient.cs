using Api.Interfaces.V2.Common;
using Api.Interfaces.V2.Recipient;

namespace ApiClient.Models.V2.Recipient {
    /// <summary>
    /// This model represent specific inquirer case recipient
    /// </summary>
    public class InquirerCaseRecipient : CaseRecipientEvents, IInquirerCaseRecipient {
        public IOrganization Organization { get; set; }
        /// <summary>
        /// If set, this shows the organizations Term of Service regarding
        /// the information provided.
        /// This information must be accepted in order to confirm their reply
        /// </summary>
        public string OrganizationToS { get; set; }
        public bool Confirmed { get; set; }
    }
}