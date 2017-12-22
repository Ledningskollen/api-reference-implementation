using System.Collections.Generic;
using Api.Interfaces.V2.Common;

namespace ApiClient.Models.V2.Common {
    /// <summary>
    /// This model represents a generic reply comment
    /// </summary>
    public class Reply : IReply {
        public string Comment { get; set; }
        public List<string> Action { get; set; }
        public bool Final { get; set; }
    }
}