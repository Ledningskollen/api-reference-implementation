using Api.Interfaces.V2.Common;
using Api.Interfaces.V2.Inquiry;
using ApiClient.Helper;

namespace ApiClient.Models.V2.Inquiry {
    /// <summary>
    /// Specific EMERGENCY case creation
    /// </summary>
    public class EmergencyInquiry : Inquiry, IEmergencyInquiry {
        /// <summary>
        /// The name of the person who is responsible for this case.
        /// </summary>
        public string ContactName { get; set; }
        /// <summary>
        /// The contact information for the responsible person. It is either
        /// a mobile number or an email. Although it is prefered to be a mobile.
        /// </summary>
        public IContactInfo ContactInformation { get; set; }

        public EmergencyInquiry() 
            : base(Constants.EMERGENCY_INQUIRY_PURPOSE) {
        }
    }
}