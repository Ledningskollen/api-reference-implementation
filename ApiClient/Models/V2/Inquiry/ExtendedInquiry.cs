using System.Collections.Generic;
using Api.Interfaces.V2.Inquiry;

namespace ApiClient.Models.V2.Inquiry {
    public abstract class ExtendedInquiry : Inquiry, IExtendedInquiry {
        public List<string> Attachments { get; set; }
        /// <summary>
        /// If the inquirer will get a notification on every reply or after everyone
        /// involved has replied to the case
        /// </summary>
        public bool NotifyOnReply { get; set; }
        /// <summary>
        /// The mobile number to which the inquirer will be notified by sms once all 
        /// organizations involved have answered the case.
        /// </summary>
        public string NotificationViaSms { get; set; }

        /// <summary>
        /// Serialization
        /// </summary>
        protected ExtendedInquiry() {}

        protected ExtendedInquiry(string purpose) : base(purpose) {
        }
    }
}