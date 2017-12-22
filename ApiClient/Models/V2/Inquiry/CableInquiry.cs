using System.Collections.Generic;
using Api.Interfaces.V2.Common;
using Api.Interfaces.V2.Inquiry;
using ApiClient.Helper;

namespace ApiClient.Models.V2.Inquiry {
    /// <summary>
    /// Specific CABLE_INDICATION case creation, 
    /// </summary>
    public class CableInquiry : ExtendedInquiry, ICableInquiry {

        public string PropertyDesignation { get; set; }

        /// <summary>
        /// The name of the person who will be on the site in case a physical
        /// placement is needed.
        /// </summary>
        public string SiteContactName { get; set; }

        /// <summary>
        /// The phonenumber of the person who will be on the site in case a physical
        /// placement is needed.
        /// </summary>
        public string SiteContactPhone { get; set; }

        /// <summary>
        /// The meeting address in case a physical placement is needed.
        /// </summary>
        public IMeetUpAddress MeetUpAddress { get; set; }
        
        /// <summary>
        /// The types of work which will take place for this job.
        /// </summary>
        public List<string> Work { get; set; }

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

        public CableInquiry()
            : base(Constants.CABLE_INQUIRY_PURPOSE) {
        }

        public static List<string> AllowedWorkTypes {
            get {
                return new List<string> {
                    "machine earthwork",
                    "milling plowing",
                    "drilling",
                    "blasting",
                    "pilling sheetpiling",
                    "logging",
                    "ground work",
                    "lake routing"
                };
            }
        }
    }
}