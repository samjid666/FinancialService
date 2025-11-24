using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NftApi.Core.Models.Csv
{
    public class FinancialRecordCsv
    {
        public string? FirstName { get; set; }
        public string? Surname { get; set; }
        public string? Dob { get; set; }
        public string? Postcode { get; set; }
        public string? AccountType { get; set; }
        public string? InitialAmount { get; set; }
        public string? TotalPaymentAmount { get; set; }
        public string? RepaymentAmount { get; set; }
        public string? RemainingAmount { get; set; }
        public string? TransactionDate { get; set; }
        public string? MinimumPaymentAmount { get; set; }
        public string? InterestRate { get; set; }
        public string? InitialTerm { get; set; }
        public string? RemainingTerm { get; set; }
        public string? Status { get; set; }
    }

    public sealed class FinancialRecordCsvMap : ClassMap<FinancialRecordCsv>
    {
        public FinancialRecordCsvMap()
        {
            Map(m => m.FirstName).Name("FirstName");
            Map(m => m.Surname).Name("Surname");
            Map(m => m.Dob).Name("Dob");
            Map(m => m.Postcode).Name("Postcode");
            Map(m => m.AccountType).Name("AccountType");
            Map(m => m.InitialAmount).Name("InitialAmount");
            Map(m => m.TotalPaymentAmount).Name("TotalPaymentAmount");
            Map(m => m.RepaymentAmount).Name("RepaymentAmount");
            Map(m => m.RemainingAmount).Name("RemainingAmount");
            Map(m => m.TransactionDate).Name("TransactionDate");
            Map(m => m.MinimumPaymentAmount).Name("MinimumPaymentAmount");
            Map(m => m.InterestRate).Name("InterestRate");
            Map(m => m.InitialTerm).Name("InitialTerm");
            Map(m => m.RemainingTerm).Name("RemainingTerm");
            Map(m => m.Status).Name("Status");
        }
    }
}
