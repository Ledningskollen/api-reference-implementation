using System.Collections.Generic;
using Api.Interfaces.V2.Inquiry;

namespace ApiClient.Models.V2.Inquiry {
    /// <summary>
    /// The information given by the inquirer about the case at creation. Non-editable
    /// </summary>
    public abstract class Inquiry : IInquiry {
        public string CreatedUsing { get; set; }
        public string Purpose { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public List<string> Geometry { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        /// <summary>
        /// Serialization
        /// </summary>
        protected Inquiry() {}

        protected Inquiry(string purpose) {
            Purpose = purpose;
        }
    }
}