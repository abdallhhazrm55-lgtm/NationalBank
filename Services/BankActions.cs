using Microsoft.EntityFrameworkCore;
using NationalBank.Data;
using NationalBank.Models;
using static NationalBank.Services.ConsoleUi;

namespace NationalBank.Services;

public class BankActions
{
    private readonly BankContext _db;
    public BankActions(BankContext db) => _db = db;

    // =================== 1) Add a new Customer ===================
    public void AddCustomer()
    {
        Console.WriteLine("--- Add New Customer ---");

        var name = ReadText("Full Name      ", s => s.Length is >= 3 and <= 100, "Full name must be 3-100 characters.");
        var nationalId = ReadText("National ID    ", IsValidNationalId, "National ID must contain 10-14 digits only.");
        var dob = ReadDate("Date of Birth  (yyyy-MM-dd)");
        var email = ReadText("Email          ", IsValidEmail, "Please enter a valid email address.");
        var phone = ReadText("Phone          ", IsValidPhone, "Phone must contain 7-15 digits (optional leading +).");
        var address = ReadText("Address        ", s => s.Length is >= 5 and <= 200, "Address must be 5-200 characters.");
        var type = ReadEnum("Customer Type", CustomerType.Individual, CustomerType.Business);

        Console.WriteLine();

        if (type == CustomerType.Individual && dob > DateTime.Today.AddYears(-18))
        {
            Error("An individual customer must be at least 18 years old.");
            return;
        }
        if (_db.Customers.Any(c => c.NationalId == nationalId))
        {
            Error($"A customer with National ID '{nationalId}' already exists.");
            return;
        }
        if (_db.Customers.Any(c => c.Email == email))
        {
            Error($"A customer with email '{email}' already exists.");
            return;
        }

        var customer = new Customer
        {
            FullName = name,
            NationalId = nationalId,
            DateOfBirth = dob,
            Email = email,
            PhoneNumber = phone,
            Address = address,
            CustomerType = type
        };
        _db.Customers.Add(customer);
        _db.SaveChanges();

        Success($"Customer created successfully. CustomerId = {customer.Id}");
    }

    // =================== 2) Open a new Account ===================
    public void OpenAccount()
    {
        Console.WriteLine("--- Open New Account ---");

        var number = ReadText("Account Number ", IsValidAccountNumber, "Account number: 3-20 letters, digits or '-'.");
        var type = ReadEnum("Account Type", AccountType.Savings, AccountType.Current, AccountType.Business);
        var branchCode = ReadText("Branch Code    ", IsValidBranchCode, "Branch code: 2-10 letters, digits or '-'.").ToUpperInvariant();
        var customerId = ReadInt("Customer Id    ");
        var role = ReadEnum("Ownership Role", OwnershipType.Primary, OwnershipType.CoHolder);

        Info($"Validating branch '{branchCode}' and customer #{customerId}...");

        if (!_db.Branches.Any(b => b.Code == branchCode))
        {
            Error($"Branch '{branchCode}' does not exist.");
            return;
        }
        if (!_db.Customers.Any(c => c.Id == customerId))
        {
            Error($"Customer #{customerId} does not exist.");
            return;
        }

        var account = _db.Accounts.FirstOrDefault(a => a.AccountNumber == number);

        if (account is null)
        {

            if (role == OwnershipType.CoHolder)
            {
                Error("A new account must start with a Primary owner. Open it as Primary first, then add co-holders.");
                return;
            }

            account = new Account
            {
                AccountNumber = number,
                AccountType = type,
                BranchCode = branchCode,
                CurrentBalance = 0m,
                OpeningDate = DateTime.Today
            };
            _db.Accounts.Add(account);
            _db.CustomerAccounts.Add(NewLink(customerId, number, role));
            _db.SaveChanges();

            Success($"Account '{number}' created and linked to customer {customerId} as {role} owner.");
            return;
        }


        if (account.BranchCode != branchCode || account.AccountType != type)
        {
            Error($"Account '{number}' already exists with a different branch or type. Use the same details to add a co-holder.");
            return;
        }
        if (_db.CustomerAccounts.Any(ca => ca.AccountNumber == number && ca.CustomerId == customerId))
        {
            Error($"Customer {customerId} is already linked to account '{number}'.");
            return;
        }
        if (role == OwnershipType.Primary &&
            _db.CustomerAccounts.Any(ca => ca.AccountNumber == number && ca.OwnershipType == OwnershipType.Primary))
        {
            Error($"Account '{number}' already has a Primary owner. Add this customer as CoHolder.");
            return;
        }

        _db.CustomerAccounts.Add(NewLink(customerId, number, role));
        _db.SaveChanges();
        Success($"Existing account '{number}' linked to customer {customerId} as {role} owner.");
    }

    private static CustomerAccount NewLink(int customerId, string accountNumber, OwnershipType role) => new()
    {
        CustomerId = customerId,
        AccountNumber = accountNumber,
        OwnershipType = role,
        OwnershipStartDate = DateTime.Today,
        AccountStatus = AccountStatus.Active
    };

    // =================== 3) Update Account Status ===================
    public void UpdateAccountStatus()
    {
        Console.WriteLine("--- Update Account Status ---");

        var number = ReadText("Account Number ", IsValidAccountNumber, "Account number: 3-20 letters, digits or '-'.");
        var customerId = ReadInt("Customer Id    ");


        var link = _db.CustomerAccounts.Find(customerId, number);
        if (link is null)
        {
            Error($"Customer {customerId} is not linked to account '{number}'.");
            return;
        }

        var newStatus = ReadEnum("New Status", AccountStatus.Active, AccountStatus.Closed);
        if (link.AccountStatus == newStatus)
        {
            Warn($"The account is already {newStatus}. Nothing to change.");
            return;
        }

        link.AccountStatus = newStatus;
        _db.SaveChanges();
        Success($"Status updated to {newStatus}.");
    }

    // =================== 4) Remove an Account from a Customer ===================
    public void RemoveAccountFromCustomer()
    {
        Console.WriteLine("--- Remove Account From Customer ---");

        var number = ReadText("Account Number ", IsValidAccountNumber, "Account number: 3-20 letters, digits or '-'.");
        var customerId = ReadInt("Customer Id    ");
        Console.WriteLine();

        var link = _db.CustomerAccounts.Find(customerId, number);
        if (link is null)
        {
            Error($"Customer {customerId} is not linked to account '{number}'.");
            return;
        }

        bool lastOwner = !_db.CustomerAccounts.Any(ca => ca.AccountNumber == number && ca.CustomerId != customerId);
        Account? account = null;

        if (lastOwner)
        {
            account = _db.Accounts.Find(number);
            if (account is not null && account.CurrentBalance != 0)
            {
                Error($"Cannot remove the last owner: account '{number}' still has a balance of {account.CurrentBalance:N2}.");
                return;
            }
        }

        _db.CustomerAccounts.Remove(link);
        if (account is not null)
            _db.Accounts.Remove(account);
        _db.SaveChanges();

        Success("  Ownership link deleted.");
        if (account is not null)
            Info($"      That was the last owner - account '{number}' was also removed.");
    }

    // =================== 5) List all Customers (with accounts) ===================
    public void ListCustomers()
    {
        Console.WriteLine("--- All Customers ---");
        Console.WriteLine();


        var customers = _db.Customers
            .AsNoTracking()
            .Include(c => c.CustomerAccounts)
                .ThenInclude(ca => ca.Account)
                    .ThenInclude(a => a.Branch)
            .OrderBy(c => c.Id)
            .ToList();

        if (customers.Count == 0)
        {
            Warn("  No customers yet.");
            return;
        }

        foreach (var c in customers)
        {
            Console.WriteLine($"  #{c.Id} {c.FullName} ({c.CustomerType})");

            if (c.CustomerAccounts.Count == 0)
            {
                Console.WriteLine("      (no accounts)");
                continue;
            }

            foreach (var ca in c.CustomerAccounts.OrderBy(x => x.AccountNumber))
            {
                var a = ca.Account;
                Console.WriteLine(
                    $"      {a.AccountNumber,-10} {a.AccountType,-9} Balance: {a.CurrentBalance,12:N2}   " +
                    $"{ca.OwnershipType,-9} {ca.AccountStatus,-7} @ {a.Branch.Name}");
            }
        }
    }
}
