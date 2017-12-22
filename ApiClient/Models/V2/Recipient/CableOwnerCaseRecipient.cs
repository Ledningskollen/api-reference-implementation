using Api.Interfaces.V2.Recipient;

namespace ApiClient.Models.V2.Recipient {
    /// <summary>
    /// Specific model for case owner recipient
    /// </summary>
    public class CableOwnerCaseRecipient : CaseRecipientEvents, ICableOwnerCaseRecipient {
        public bool RequiresReply { get; set; }
    }
}