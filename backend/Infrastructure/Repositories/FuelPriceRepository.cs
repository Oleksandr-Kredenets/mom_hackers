using System.Collections.Frozen;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using TMS.Application.Enums;
using TMS.Domain.Interfaces;

namespace TMS.Infrastructure.Repositories;

public class FuelPriceRepository : IFuelPriceRepository
{
    private static readonly FrozenDictionary<string, FuelOperatorType> CsvOperatorToEnum =
        new Dictionary<string, FuelOperatorType>(StringComparer.Ordinal)
        {
            ["AMIC"] = FuelOperatorType.Amic,
            ["BVS"] = FuelOperatorType.Bvs,
            ["Brent Oil"] = FuelOperatorType.BrentOil,
            ["Chipo"] = FuelOperatorType.Chipo,
            ["EURO5"] = FuelOperatorType.Euro5,
            ["Grand Petrol"] = FuelOperatorType.GrandPetrol,
            ["Green Wave"] = FuelOperatorType.GreenWave,
            ["KLO"] = FuelOperatorType.Klo,
            ["Mango"] = FuelOperatorType.Mango,
            ["Marshal"] = FuelOperatorType.Marshal,
            ["Motto"] = FuelOperatorType.Motto,
            ["Neftek"] = FuelOperatorType.Neftek,
            ["Ovis"] = FuelOperatorType.Ovis,
            ["Parallel"] = FuelOperatorType.Parallel,
            ["RLS"] = FuelOperatorType.Rls,
            ["Rodnik"] = FuelOperatorType.Rodnik,
            ["SOCAR"] = FuelOperatorType.Socar,
            ["SUN OIL"] = FuelOperatorType.SunOil,
            ["U.GO"] = FuelOperatorType.UGo,
            ["UKRNAFTA"] = FuelOperatorType.Ukrnafta,
            ["UPG"] = FuelOperatorType.Upg,
            ["VST"] = FuelOperatorType.Vst,
            ["VostokGaz"] = FuelOperatorType.VostokGaz,
            ["WOG"] = FuelOperatorType.Wog,
            ["ZOG"] = FuelOperatorType.Zog,
            ["Авантаж 7"] = FuelOperatorType.Avantazh7,
            ["Автотранс"] = FuelOperatorType.Avtotrans,
            ["БРСМ-Нафта"] = FuelOperatorType.BrsmNafta,
            ["ДНІПРОНАФТА"] = FuelOperatorType.Dnipronafta,
            ["Катрал"] = FuelOperatorType.Katral,
            ["Кворум"] = FuelOperatorType.Kvorum,
            ["Маркет"] = FuelOperatorType.Market,
            ["ОККО"] = FuelOperatorType.Okko,
            ["Олас"] = FuelOperatorType.Olas,
            ["Рур груп"] = FuelOperatorType.RurGrup,
            ["СВОЇ"] = FuelOperatorType.Svoi,
            ["Фактор"] = FuelOperatorType.Faktor,
        }.ToFrozenDictionary(StringComparer.Ordinal);

    private List<OperatorFuelPrice> ReadOperatorFuelPrices()
    {
        var csvPath = Environment.GetEnvironmentVariable("FUEL_PRICE_CSV_PATH");
        if (string.IsNullOrWhiteSpace(csvPath))
            throw new InvalidOperationException("FUEL_PRICE_CSV_PATH environment variable is not set.");

        if (!File.Exists(csvPath))
            throw new FileNotFoundException($"File {csvPath} not found. Could not load fuel prices.");

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<FuelPriceCsvRow>().Select(ToOperatorFuelPrice).ToList();
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

    private static OperatorFuelPrice ToOperatorFuelPrice(FuelPriceCsvRow row)
    {
        var key = row.Operator.Trim();
        if (!CsvOperatorToEnum.TryGetValue(key, out var fuelOperator))
            throw new InvalidDataException($"Unknown fuel operator in CSV: {row.Operator}");

        return new OperatorFuelPrice(
            fuelOperator,
            new Dictionary<FuelType, double?>
            {
                [FuelType.A95Plus] = NormalizePrice(row.A95Plus),
                [FuelType.A95] = NormalizePrice(row.A95),
                [FuelType.A92] = NormalizePrice(row.A92),
                [FuelType.DP] = NormalizePrice(row.Dp),
                [FuelType.Gas] = NormalizePrice(row.Gas),
            });
    }

    public Task<OperatorFuelPrice> GetOperatorFuelPriceAsync(FuelOperatorType operatorType)
    {
        var byOperator = ReadOperatorFuelPrices().ToDictionary(op => op.Operator);
        return Task.FromResult(byOperator[operatorType]);
    }

    public Task<IEnumerable<OperatorFuelPrice>> GetAllOperatorFuelPricesAsync()
    {
        return Task.FromResult<IEnumerable<OperatorFuelPrice>>(ReadOperatorFuelPrices());
    }

    public Task<IEnumerable<OperatorFuelPrice>> GetCheapestOperatorByFuelTypeAsync(FuelType fuelType)
    {
        var result = ReadOperatorFuelPrices()
            .Where(op => op.FuelPrices[fuelType] is not null)
            .OrderBy(op => op.FuelPrices[fuelType])
            .ToList();
        return Task.FromResult<IEnumerable<OperatorFuelPrice>>(result);
    }
}
