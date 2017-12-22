using System;
using Api.Interfaces.V2.Recipient;
using Api.Interfaces.V2.Common;

namespace ApiClient.Models.V2.Recipient {
    /// <summary>
    /// This model represents the base for an recipient
    /// </summary>
    public class Recipient : IRecipient, IEquatable<Recipient> {
        public string Id { get; set; }
        public string Name { get; set; }
        public IContactInfo ContactInfo { get; set; }

        public override bool Equals(object obj) {
            return Equals(obj as Recipient);
        }

        public bool Equals(Recipient other) {
            if (other == null)
                return false;

            return Id.Equals(other.Id)
                   &&
                   (Name != null &&
                    Name.Equals(other.Name)) &&
                   (
                       ContactInfo != null &&
                       ContactInfo.Equals(other.ContactInfo)
                       );
        }
    }
}

