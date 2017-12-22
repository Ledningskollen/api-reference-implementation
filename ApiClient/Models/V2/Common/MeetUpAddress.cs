using Api.Interfaces.V2.Common;

namespace ApiClient.Models.V2.Common {
    /// <summary>
    /// This model represents posting meeting address
    /// </summary>
    public class MeetUpAddress : IMeetUpAddress {
        public string StreetNameAndNumber { get; set; }
        public string PostCode { get; set; }
        public string CityName { get; set; }
    }
}