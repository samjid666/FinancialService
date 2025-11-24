using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NftApi.Core.Services
{
    public record ProcessingResult(int SuccessfulRecords, int FailedRecords, List<string> Errors);

    public interface IFileProcessingService
    {
        Task<ProcessingResult> ProcessPeopleFileAsync(Stream fileStream);
        Task<ProcessingResult> ProcessFinancialRecordsFileAsync(Stream fileStream);
    }
}
