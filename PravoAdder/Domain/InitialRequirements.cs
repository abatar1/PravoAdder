namespace PravoAdder.Domain
{
    public class InitialRequirements
    {
        public double TotalClaims { get; set; }
        public double StateFee { get; set; }
        public double TotalClaimsWithStateFee { get; set; }
        public string Requisition { get; set; }
        public double PaymentsArrears { get; set; }
        public double InterestArrears { get; set; }
        public double Percents { get; set; }
        public double IndebtednessForInsurance { get; set; }
        public RequirementsType Type { get; set; }

        public enum RequirementsType { Initial, Karkade }
    }
}
