using Api.Interfaces.V2.Inquiry;
using ApiClient.Helper;

namespace ApiClient.Models.V2.Inquiry {
    /// <summary>
    /// Specific PROJECT case creation
    /// </summary>   
    public class ProjectInquiry : ExtendedInquiry, IProjectInquiry {

        /// <summary>
        /// Indicates if the inquirer prefers the answerts to be sent as pdf
        /// </summary>
        public bool ReplyAsPdf { get; set; }

        /// <summary>
        /// The max depth needed to dig.
        /// </summary>
        public double ExcavationDepth { get; set; }

        /// <summary>
        /// In case the inquirer needs to be contacted, this is the prefered contact way
        /// it can be email/fax/address
        /// </summary>
        public string PreferedContactWay { get; set; }

        public string PreferedContactValue { get; set; }

        public ProjectInquiry()
            : base(Constants.PROJECT_INQUIRY_PURPOSE) {

        }
    }
}