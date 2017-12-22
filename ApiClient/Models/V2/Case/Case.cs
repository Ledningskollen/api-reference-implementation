using System.Collections.Generic;
using Api.Interfaces.V2.Case;
using Api.Interfaces.V2.Common;
using Api.Interfaces.V2.Inquiry;

namespace ApiClient.Models.V2.Case {
    /// <summary>
    /// This model intends to unify a cable owner and inquirer view of a case
    /// </summary>
    public abstract class Case : ICase {
        public string Id { get; set; }
        public string Created { get; set; }
        public string Status { get; set; }
        public IUser Inquirer { get; set; }

        /// <summary>
        /// This represents the center coordinate in the projections SWEREF99TM, RT90 and WGS84
        /// </summary>
        public Dictionary<string, List<double[]>> Center { get; set; }

        /// <summary>
        /// Only one of these Inquiries have content at runtime
        /// </summary>
        public ICableInquiry CableInquiry { get; set; }

        /// <summary>
        /// Only one of these Inquiries have content at runtime
        /// </summary>
        public ICollaborationInquiry CollaborationInquiry { get; set; }

        /// <summary>
        /// Only one of these Inquiries have content at runtime
        /// </summary>
        public IEmergencyInquiry EmergencyInquiry { get; set; }

        /// <summary>
        /// Only one of these Inquiries have content at runtime
        /// </summary>
        public IPlanningInquiry PlanningInquiry { get; set; }

        /// <summary>
        /// Only one of these Inquiries have content at runtime
        /// </summary>
        public IProjectInquiry ProjectInquiry { get; set; }

        public List<string> Regions { get; set; }
        public string Type { get; set; }
    }
}