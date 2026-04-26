using Microsoft.EntityFrameworkCore;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Interfaces;
using Nalbur.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nalbur.Infrastructure.Services
{
    public class WorkContractService : IWorkContractService
    {
        private readonly NalburDbContext _context;

        public WorkContractService(NalburDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkContract>> GetAllAsync()
        {
            return await _context.WorkContracts
                .OrderByDescending(x => x.ContractDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<List<WorkContract>> GetFilteredAsync(DateTime? startDate, DateTime? endDate, string? searchText)
        {
            var query = _context.WorkContracts.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(x => x.ContractDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.ContractDate.Date <= endDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.Trim();

                query = query.Where(x =>
                    x.Title.Contains(search) ||
                    (x.CustomerName != null && x.CustomerName.Contains(search)) ||
                    (x.CustomerPhone != null && x.CustomerPhone.Contains(search)) ||
                    x.WorkDescription.Contains(search) ||
                    x.Materials.Contains(search));
            }

            return await query
                .OrderByDescending(x => x.ContractDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task AddAsync(WorkContract contract)
        {
            contract.CreatedAt = DateTime.Now;
            contract.UpdatedAt = null;

            _context.WorkContracts.Add(contract);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WorkContract contract)
        {
            var existing = await _context.WorkContracts.FindAsync(contract.Id);

            if (existing == null)
                throw new Exception("Sözleşme bulunamadı.");

            existing.Title = contract.Title;
            existing.CustomerName = contract.CustomerName;
            existing.CustomerPhone = contract.CustomerPhone;
            existing.WorkDescription = contract.WorkDescription;
            existing.Materials = contract.Materials;
            existing.Notes = contract.Notes;
            existing.ContractDate = contract.ContractDate;
            existing.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var contract = await _context.WorkContracts.FindAsync(id);

            if (contract == null)
                return;

            _context.WorkContracts.Remove(contract);
            await _context.SaveChangesAsync();
        }
    }
}
