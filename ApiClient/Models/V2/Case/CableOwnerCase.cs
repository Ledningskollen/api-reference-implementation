using System.Collections.Generic;
using Api.Interfaces.V2.Case;
using Api.Interfaces.V2.Recipient;

namespace ApiClient.Models.V2.Case {
    /// <summary>
    /// This model represents a cable owner view of a case
    /// </summary>
    public class CableOwnerCase : Case, ICableOwnerCase {
        public string Name { get; set; }
        public List<ICableOwnerCaseRecipient> Recipients { get; set; }
    }
}