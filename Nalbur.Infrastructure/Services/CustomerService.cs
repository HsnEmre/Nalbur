using Microsoft.EntityFrameworkCore;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Interfaces;
using Nalbur.Infrastructure.Data;

namespace Nalbur.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly NalburDbContext _context;

    public CustomerService(NalburDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> GetAllAsync() => await _context.Customers.AsNoTracking().ToListAsync();

    public async Task<Customer?> GetByIdAsync(int id) => await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        var existingCustomer = await _context.Customers.FindAsync(customer.Id);
        if (existingCustomer != null)
        {
            existingCustomer.Name = customer.Name;
            existingCustomer.SurnameCompany = customer.SurnameCompany;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Email = customer.Email;
            existingCustomer.Address = customer.Address;
            
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }
}
