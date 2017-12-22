using System.Collections.Generic;
using Api.Interfaces.V2.Common;

namespace ApiClient.Models.V2.Common {
    /// <summary>
    /// This model represents a generic event for responses to inquirers
    /// </summary>
    public class Event: IEvent {
        public string Time { get; set; }
        public string Comment { get; set; }
        public List<string> Measure { get; set; }
    }
}