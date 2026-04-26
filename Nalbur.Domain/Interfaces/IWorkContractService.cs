using Nalbur.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nalbur.Domain.Interfaces
{
    public interface IWorkContractService
    {
        Task<List<WorkContract>> GetAllAsync();

        Task<List<WorkContract>> GetFilteredAsync(DateTime? startDate, DateTime? endDate, string? searchText);

        Task AddAsync(WorkContract contract);

        Task UpdateAsync(WorkContract contract);

        Task DeleteAsync(int id);
    }
}
