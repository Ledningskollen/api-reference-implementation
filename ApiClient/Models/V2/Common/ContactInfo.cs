using System;
using Api.Interfaces.V2.Common;

namespace ApiClient.Models.V2.Common {
	/// <summary>
	/// This model represents generic contact information
	/// </summary>
	public class ContactInfo : IContactInfo, IEquatable<ContactInfo> {
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string FaxNumber { get; set; }
		public string Address { get; set; }

		public override bool Equals(object obj) {
			return Equals(obj as ContactInfo);
		}

		public bool Equals(ContactInfo other) {
			if (other == null)
				return false;

			return (Email == other.Email ||
					Email != null &&
					Email.Equals(other.Email)) &&
				   (PhoneNumber == other.PhoneNumber ||
					PhoneNumber != null &&
					PhoneNumber.Equals(other.PhoneNumber)) &&
				   (FaxNumber == other.FaxNumber ||
					FaxNumber != null &&
					FaxNumber.Equals(other.FaxNumber)) &&
				   (Address == other.Address ||
					Address != null &&
					Address.Equals(other.Address));


		}
	}
}