using System.Collections.Generic;
using Api.Interfaces.V2.Case;
using Api.Interfaces.V2.Recipient;

namespace ApiClient.Models.V2.Case {
    /// <summary>
    /// This model represents a inquirer view of a case
    /// </summary>
    public class InquirerCase : Case, IInquirerCase {
        public List<IInquirerCaseRecipient> Recipients { get; set; }
        public List<string> OtherKnownOrganizations { get; set; }
    }
}