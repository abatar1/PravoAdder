using System;
using System.Collections.Generic;
using System.Linq;

namespace PravoAdder.Api.Domain
{
	public class ParticipantType : DatabaseEntityItem
	{
		public string TypeName { get; set; }
		public string NameEn { get; set; }

		public static readonly string PersonTypeName = "Person";
		public static readonly string CompanyTypeName = "Company";

		private static List<ParticipantType> _participantTypes;

		public static ParticipantType GetPersonType(HttpAuthenticator authenticator)
		{
			return GetType(authenticator, PersonTypeName);		
		}

		public static ParticipantType GetCompanyType(HttpAuthenticator authenticator)
		{
			return GetType(authenticator, CompanyTypeName);
		}

		public static ParticipantType GetType(HttpAuthenticator authenticator, string name)
		{
			if (name != PersonTypeName && name != CompanyTypeName) throw new ArgumentException("Wrong participant type name");
			if (_participantTypes == null) _participantTypes = ApiRouter.Bootstrap.GetParticipantTypes(authenticator);
			return _participantTypes.First(t => t.Name.Equals(name));
		}
	}
}
