using Api.Interfaces.V2.Inquiry;
using ApiClient.Helper;

namespace ApiClient.Models.V2.Inquiry {
    /// <summary>
    /// Specific PLANNING case creation
    /// </summary>
    public class PlanningInquiry : ExtendedInquiry, IPlanningInquiry {

        /// <summary>
        /// In case the inquirer needs to be contacted, this is the prefered contact way
        /// it can be email/fax/address
        /// </summary>
        public string PreferedContactWay { get; set; }
        public string PreferedContactValue { get; set; }

        public PlanningInquiry()
            : base(Constants.PLANNING_INQUIRY_PURPOSE) {
        }
    }
}
