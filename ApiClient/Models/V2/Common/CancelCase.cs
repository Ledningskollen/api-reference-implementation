using Api.Interfaces.V2.Common;

namespace ApiClient.Models.V2.Common {
    /// <summary>
    /// Model for case cancellations
    /// </summary>
    public class CancelCase : ICancelCase {
        public string Reason { get; set; }
        public string Comment { get; set; }
    }
}