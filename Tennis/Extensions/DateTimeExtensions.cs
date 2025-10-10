using System.Globalization;

namespace Tennis.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// Parses a date range string from ATP Tour tournament dates into start and end dates.
    /// </summary>
    /// <param name="value">The date range string to parse. Expected formats:
    /// 1. "d MMMM, yyyy - d MMMM, yyyy" (e.g. "1 January, 2024 - 15 January, 2024")
    /// 2. "d MMMM - d MMMM, yyyy" (e.g. "28 December - 7 January, 2024")
    /// 3. "d - d MMMM, yyyy" (e.g. "31 - 7 January, 2024")
    /// </param>
    /// <returns>A tuple containing the start date (dateFrom) and end date (dateTo)</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when:
    /// - The input string cannot be split into exactly two parts with " - "
    /// - The end date cannot be parsed in "d MMMM, yyyy" format
    /// - The start date cannot be parsed in any supported format
    /// </exception>
    /// <remarks>
    /// The method handles several edge cases:
    /// 1. When start date is in a different year than end date (e.g. Dec 2023 - Jan 2024)
    /// 2. When start date is in a different month than end date (e.g. 31 Dec - 7 Jan)
    /// 3. When only the day is provided for start date (e.g. "31 - 7 January, 2024")
    /// </remarks>
    public static (DateTime dateFrom, DateTime dateTo) ParseAtpTourDateRange(string value)
    {
        var parts = value.Split(" - ", StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid date range format: {value}");
        }

        // Try to parse end date first as it always has the year
        if (!DateTime.TryParseExact(parts[1], "d MMMM, yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dateTo))
        {
            throw new ArgumentException($"Invalid end date format: {parts[1]}. Value={value}");
        }

        // Try to parse start date as full date first (with year)
        if (DateTime.TryParseExact(parts[0], "d MMMM, yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dateFrom))
        {
            return (dateFrom, dateTo);
        }

        // Try to parse start date with month but no year
        var startDateWithYear = $"{parts[0]}, {dateTo.Year}";
        if (DateTime.TryParseExact(startDateWithYear, "d MMMM, yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dateFrom))
        {
            // If the parsed date is after the end date, it means we need to use the previous year
            if (dateFrom > dateTo)
            {
                startDateWithYear = $"{parts[0]}, {dateTo.Year - 1}";
                if (!DateTime.TryParseExact(startDateWithYear, "d MMMM, yyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out dateFrom))
                {
                    throw new ArgumentException($"Invalid start date format: {startDateWithYear}. Value={value}");
                }
            }
            return (dateFrom, dateTo);
        }

        // If start date is just a day, combine it with month and year from end date
        if (int.TryParse(parts[0], out int startDay))
        {
            // If the start day is greater than the end day, and they're in different months,
            // we need to use the previous month
            var startMonth = dateTo.Month;
            var startYear = dateTo.Year;

            if (startDay > dateTo.Day)
            {
                startMonth = dateTo.Month == 1 ? 12 : dateTo.Month - 1;
                startYear = startMonth == 12 ? startYear - 1 : startYear;
            }

            var startDateStr = $"{startDay} {new DateTime(startYear, startMonth, 1):MMMM}, {startYear}";

            if (!DateTime.TryParseExact(startDateStr, "d MMMM, yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out dateFrom))
            {
                throw new ArgumentException($"Invalid start date format: {startDateStr}. Value={value}");
            }
        }
        else
        {
            throw new ArgumentException($"Invalid start date format: {parts[0]}. Value={value}");
        }

        return (dateFrom, dateTo);
    }

    /// <summary>
    /// Parses a date range string from ITF Tennis tournament dates into start and end dates.
    /// </summary>
    /// <param name="value">The date range string to parse (e.g. "06 Oct to 12 Oct 2025")</param>
    /// <returns>A tuple containing the start date (dateFrom) and end date (dateTo)</returns>
    /// <exception cref="ArgumentException">Thrown when the input string cannot be split into exactly two parts with "to"</exception>
    public static (DateTime dateFrom, DateTime dateTo) ParseItfDateRange(string value)
    {
        // Split the string by "to" and trim whitespace
        var parts = value.Split("to", StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid date range format: {value}");
        }

        // Parse the first date (e.g. "27 Oct")
        var fromParts = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var day1 = int.Parse(fromParts[0]);
        var month1 = DateTime.ParseExact(fromParts[1], "MMM", CultureInfo.InvariantCulture).Month;

        // Parse the second date (e.g. "02 Nov 2025")
        var toParts = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var day2 = int.Parse(toParts[0]);
        var month2 = DateTime.ParseExact(toParts[1], "MMM", CultureInfo.InvariantCulture).Month;
        var year = int.Parse(toParts[2]);

        // Create DateTime objects
        var dateFrom = new DateTime(year, month1, day1);
        var dateTo = new DateTime(year, month2, day2);

        return (dateFrom, dateTo);
    }
}
