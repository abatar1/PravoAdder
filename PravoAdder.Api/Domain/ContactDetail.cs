namespace PravoAdder.Api.Domain
{
	public class ContactDetail
	{
		public ContactDetail(string phone, string email, string site, string address)
		{
			Phone = phone;
			Email = email;
			Site = site;
			Address = address;
		}

		public ContactDetail()
		{
		}

		public string Phone { get; set; }
		public string Email { get; set; }
		public string Site { get; set; }
		public string Address { get; set; }
	}
}
