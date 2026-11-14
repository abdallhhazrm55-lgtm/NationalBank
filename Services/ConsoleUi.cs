using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace NationalBank.Services;

public static class ConsoleUi
{
    // ---------- messages ----------
    public static void Success(string msg) => Write(msg, ConsoleColor.Green);
    public static void Info(string msg) => Write(msg, ConsoleColor.Cyan);
    public static void Error(string msg) => Write(msg, ConsoleColor.Red);
    public static void Warn(string msg) => Write(msg, ConsoleColor.Yellow);

    private static void Write(string msg, ConsoleColor color)
    {
        var old = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(msg);
        Console.ForegroundColor = old;
    }

    public static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to return to the menu...");
        try { Console.ReadKey(true); }
        catch (InvalidOperationException) { Console.ReadLine(); }
    }

    public static void ClearScreen()
    {
        try { Console.Clear(); } catch (IOException) { }
    }

    // ---------- input ----------
    private static string ReadLineOrThrow()
    {
        var line = Console.ReadLine();
        if (line is null) throw new EndOfStreamException();
        return line.Trim();
    }


    public static string ReadText(string label, Func<string, bool> isValid, string errorMessage)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            var input = ReadLineOrThrow();
            if (isValid(input)) return input;
            Error("  " + errorMessage);
        }
    }

    public static int ReadInt(string label)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            if (int.TryParse(ReadLineOrThrow(), NumberStyles.None, CultureInfo.InvariantCulture, out var value) && value > 0)
                return value;
            Error("  Please enter a valid positive whole number.");
        }
    }

    public static DateTime ReadDate(string label)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            var input = ReadLineOrThrow();
            if (!DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                Error("  Invalid date. Use the format yyyy-MM-dd (for example 2000-01-25).");
                continue;
            }
            if (date > DateTime.Today || date < new DateTime(1900, 1, 1))
            {
                Error("  The date must be between 1900-01-01 and today.");
                continue;
            }
            return date;
        }
    }

    public static T ReadEnum<T>(string title, params T[] options) where T : struct, Enum
    {
        Console.WriteLine($"{title}:");
        for (int i = 0; i < options.Length; i++)
            Console.WriteLine($"    {i + 1}) {options[i]}");

        while (true)
        {
            Console.Write("  Choice: ");
            if (int.TryParse(ReadLineOrThrow(), out var n) && n >= 1 && n <= options.Length)
                return options[n - 1];
            Error($"  Please enter a number between 1 and {options.Length}.");
        }
    }

    // ---------- validators ----------
    public static bool IsValidEmail(string s)
    {
        if (s.Length is 0 or > 150) return false;
        try { return new MailAddress(s).Address == s && s.Contains('.'); }
        catch (FormatException) { return false; }
    }

    public static bool IsValidPhone(string s) => Regex.IsMatch(s, @"^\+?\d{7,15}$");
    public static bool IsValidNationalId(string s) => Regex.IsMatch(s, @"^\d{10,14}$");
    public static bool IsValidAccountNumber(string s) => Regex.IsMatch(s, @"^[A-Za-z0-9-]{3,20}$");
    public static bool IsValidBranchCode(string s) => Regex.IsMatch(s, @"^[A-Za-z0-9-]{2,10}$");
}
