namespace PravoAdder.Api.Domain
{
	public class ContactDetail
	{
		public ContactDetail(string phone, string email, string site)
		{
			Phone = phone;
			Email = email;
			Site = site;
		}

		public string Phone { get; }
		public string Email { get; }
		public string Site { get; }
	}
}
