using System.Collections.Generic;
using Api.Interfaces.V2.Common;
using Api.Interfaces.V2.Recipient;

namespace ApiClient.Models.V2.Recipient {
    /// <summary>
    /// This model represent an extension of the base recipient 
    /// </summary>
    public abstract class CaseRecipientEvents : ICaseRecipientEvents {
        public IRecipient Recipient { get; set; }
        public string Status { get; set; }
        public List<IEvent> Events { get; set; }
        public double ResponseTime { get; set; }
        public double PhysicalResponseTime { get; set; }
    }
}