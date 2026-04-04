using TMS.Application.Enums;

namespace TMS.Domain.Interfaces;

public readonly record struct OperatorFuelPrice(string Operator, Dictionary<FuelType, double?> FuelPrices);

public interface IFuelPriceRepository
{
    Task<OperatorFuelPrice> GetOperatorFuelPriceAsync(string operatorName);
    Task<IEnumerable<OperatorFuelPrice>> GetAllOperatorFuelPricesAsync();
    Task<IEnumerable<OperatorFuelPrice>> GetCheapestOperatorByFuelTypeAsync(FuelType fuelType);
}