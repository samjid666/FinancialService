using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NftApi.Core.Models.Csv
{
    public class PersonCsv
    {
        public string? FirstName { get; set; }
        public string? Surname { get; set; }
        public string? Dob { get; set; }
        public string? Address { get; set; }
        public string? Postcode { get; set; }
    }

    public sealed class PersonCsvMap : ClassMap<PersonCsv>
    {
        public PersonCsvMap()
        {
            Map(m => m.FirstName).Name("FirstName");
            Map(m => m.Surname).Name("Surname");
            Map(m => m.Dob).Name("Dob");
            Map(m => m.Address).Name("Address");
            Map(m => m.Postcode).Name("Postcode");
        }
    }
}
