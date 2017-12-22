using System.Collections.Generic;
using Api.Interfaces.V2.Inquiry;

namespace ApiClient.Models.V2.Inquiry {
    /// <summary>
    /// For asking about recipients
    /// </summary>
    public class CreateRecipients : ICreateRecipients {
        public string Purpose { get; set; }
        public List<string> Geometry { get; set; }

        public CreateRecipients() {
        }

        /// <summary>
        /// Default empty constructor
        /// </summary>
        /// <param name="purpose">Default purpose for inquiry</param>
        public CreateRecipients(string purpose) {
            Purpose = purpose;
        }
    }

    /// <summary>
    /// Common properites for all creations
    /// Read: http://weblog.west-wind.com/posts/2012/May/08/Passing-multiple-POST-parameters-to-Web-API-Controller-Methods
    /// </summary>
    public abstract class CreateInquiry : ICreateInquiry {
        public List<string> ExemptedOrganzations { get; set; }
        public List<int> Attachments { get; set; }
    }

    /// <summary>
    /// Specific CableInquiry creation
    /// </summary>
    public class CreateCableInquiry : CreateInquiry, ICreateCableInquiry {
        public ICableInquiry Inquiry { get; set; }
    }

    /// <summary>
    /// Specific CollaborationInquiry creation
    /// </summary>
    public class CreateCollaborationInquiry : CreateInquiry, ICreateCollaborationInquiry {
        public ICollaborationInquiry Inquiry { get; set; }
    }

    /// <summary>
    /// Specific EmergencyInquiry creation
    /// </summary>
    public class CreateEmergencyInquiry : CreateInquiry, ICreateEmergencyInquiry {
        public IEmergencyInquiry Inquiry { get; set; }
    }

    /// <summary>
    /// Specific PlanningInquiry creation
    /// </summary>
    public class CreatePlanningInquiry : CreateInquiry, ICreatePlanningInquiry {
        public IPlanningInquiry Inquiry { get; set; }
    }

    /// <summary>
    /// Specific ProjectInquiry creation
    /// </summary>
    public class CreateProjectInquiry : CreateInquiry, ICreateProjectInquiry {
        public IProjectInquiry Inquiry { get; set; }
    }
}