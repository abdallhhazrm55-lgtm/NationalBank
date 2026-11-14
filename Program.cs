using Microsoft.EntityFrameworkCore;
using NationalBank.Data;
using NationalBank.Services;
using static NationalBank.Services.ConsoleUi;

namespace NationalBank;

public class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        using var db = new BankContext();

        try
        {
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            Error("Could not connect to or update the database.");
            Error(ex.InnerException?.Message ?? ex.Message);
            return;
        }

        var actions = new BankActions(db);
        bool running = true;

        try
        {
            while (running)
            {
                ShowMenu();
                var input = Console.ReadLine();
                if (input is null) break;

                Console.WriteLine();
                try
                {
                    switch (input.Trim())
                    {
                        case "1": actions.AddCustomer(); break;
                        case "2": actions.OpenAccount(); break;
                        case "3": actions.UpdateAccountStatus(); break;
                        case "4": actions.RemoveAccountFromCustomer(); break;
                        case "5": actions.ListCustomers(); break;
                        case "0": running = false; break;
                        default: Error("Invalid choice. Please enter a number from the menu (0-5)."); break;
                    }
                }
                catch (EndOfStreamException) { throw; }
                catch (DbUpdateException ex)
                {
                    Error("The database rejected the operation: " + (ex.InnerException?.Message ?? ex.Message));
                }
                catch (Exception ex)
                {
                    Error("Something went wrong: " + ex.Message);
                }
                finally
                {
                    db.ChangeTracker.Clear();
                }

                if (running) Pause();
            }
        }
        catch (EndOfStreamException)
        {

        }


        db.Database.CloseConnection();
        Console.WriteLine("Connection closed. Goodbye!");
    }

    private static void ShowMenu()
    {
        ClearScreen();
        Console.WriteLine("========================================");
        Console.WriteLine("    National Bank - Management");
        Console.WriteLine("========================================");
        Console.WriteLine("  1) Add a new Customer");
        Console.WriteLine("  2) Open a new Account for a Customer");
        Console.WriteLine("  3) Update Account Status (Active / Closed)");
        Console.WriteLine("  4) Remove an Account from a Customer");
        Console.WriteLine("  5) List all Customers (with accounts)");
        Console.WriteLine("  0) Exit");
        Console.WriteLine("----------------------------------------");
        Console.Write("  Enter choice: ");
    }
}
