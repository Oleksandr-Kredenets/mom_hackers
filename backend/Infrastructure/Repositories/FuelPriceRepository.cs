using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using TMS.Application.Enums;
using TMS.Domain.Interfaces;

namespace TMS.Infrastructure.Repositories;

public class FuelPriceRepository : IFuelPriceRepository
{
    private readonly Dictionary<string, OperatorFuelPrice> _operatorFuelPrices;

    public FuelPriceRepository(string? csvPath = null)
    {
        csvPath ??= Environment.GetEnvironmentVariable("FUEL_PRICE_CSV_PATH");
        if (string.IsNullOrWhiteSpace(csvPath))
            throw new InvalidOperationException("FUEL_PRICE_CSV_PATH environment variable is not set.");

        if (!File.Exists(csvPath))
            throw new FileNotFoundException($"File {csvPath} not found. Could not load fuel prices.");

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        _operatorFuelPrices = csv
            .GetRecords<FuelPriceCsvRow>()
            .Select(ToOperatorFuelPrice)
            .ToDictionary(op => op.Operator);
    }

    private sealed class FuelPriceCsvRow
    {
        [Name("operator")]
        public string Operator { get; set; } = "";

        [Name("a95_plus")]
        public double A95Plus { get; set; }

        [Name("a95")]
        public double A95 { get; set; }

        [Name("a92")]
        public double A92 { get; set; }

        [Name("dp")]
        public double Dp { get; set; }

        [Name("gas")]
        public double Gas { get; set; }
    }

    private static double? NormalizePrice(double value) => value == -1 ? null : value;

    private static OperatorFuelPrice ToOperatorFuelPrice(FuelPriceCsvRow row) => new(
        row.Operator,
        new Dictionary<FuelType, double?>
        {
            [FuelType.A95Plus] = NormalizePrice(row.A95Plus),
            [FuelType.A95] = NormalizePrice(row.A95),
            [FuelType.A92] = NormalizePrice(row.A92),
            [FuelType.DP] = NormalizePrice(row.Dp),
            [FuelType.Gas] = NormalizePrice(row.Gas),
        });

    public Task<OperatorFuelPrice> GetOperatorFuelPriceAsync(string operatorName)
    {
        return Task.FromResult(_operatorFuelPrices[operatorName]);
    }

    public Task<IEnumerable<OperatorFuelPrice>> GetAllOperatorFuelPricesAsync()
    {
        return Task.FromResult(_operatorFuelPrices.Values.ToList());
    }

    public Task<IEnumerable<OperatorFuelPrice>> GetCheapestOperatorByFuelTypeAsync(FuelType fuelType)
    {
        return Task.FromResult(
            _operatorFuelPrices.Values
                .Where(op => op.FuelPrices[fuelType] is not null)
                .OrderBy(op => op.FuelPrices[fuelType])
                .ToList());
    }
}
