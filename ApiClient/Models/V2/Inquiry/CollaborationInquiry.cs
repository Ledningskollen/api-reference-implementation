using System.Collections.Generic;
using Api.Interfaces.V2.Inquiry;
using ApiClient.Helper;

namespace ApiClient.Models.V2.Inquiry {
    /// <summary>
    /// Specific COLLABORATION case creation
    /// </summary>    
    public class CollaborationInquiry : ExtendedInquiry, ICollaborationInquiry {
        /// <summary>
        /// The types of work which will take place for this job.
        /// </summary>
        public List<string> Work { get; set; }
        /// <summary>
        /// The max depth needed to dig.
        /// </summary>
        public double ExcavationDepth { get; set; }
        /// <summary>
        /// The max width needed to dig.
        /// </summary>
        public double ExcavationWidth { get; set; }

        /// <summary>
        /// The tentative start date for the project to start
        /// </summary>
        public string ProjectStartDate { get; set; }
        /// <summary>
        /// The tentative end date for the project to start
        /// </summary>
        public string ProjectEndDate { get; set; }

        public CollaborationInquiry()
            : base(Constants.COLLABORATION_INQUIRY_PURPOSE) {
        }

        public static List<string> AllowedWorkTypes {
            get {
                return new List<string> {
                    "road construction",
                    "walk bike road construction",
                    "fiber channel",
                    "asphalt area",
                    "pipes for water",
                    "pipes for electricity",
                    "pipes for telecommunication",
                    "pipes for heating",
                    "living area",
                    "commercial area",
                    "industrial area",
                    "other"
                };
            }
        }
    }
}
