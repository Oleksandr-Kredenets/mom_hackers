using TMS.Application.Enums;

namespace TMS.Domain.Interfaces;

public readonly record struct OperatorFuelPrice(FuelOperatorType Operator, Dictionary<FuelType, double?> FuelPrices);

public interface IFuelPriceRepository
{
    Task<OperatorFuelPrice> GetOperatorFuelPriceAsync(FuelOperatorType operatorType);
    Task<IEnumerable<OperatorFuelPrice>> GetAllOperatorFuelPricesAsync();
    Task<IEnumerable<OperatorFuelPrice>> GetCheapestOperatorByFuelTypeAsync(FuelType fuelType);
}