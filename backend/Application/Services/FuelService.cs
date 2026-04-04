using TMS.Application.Interfaces;
using TMS.Application.Enums;

namespace TMS.Application.Services;

public class FuelService : IFuelService
{
    double GetFuelCost(FuelType fuelType, FuelOperatorType operatorType);
    {
        return _fuelPriceRepository.GetOperatorFuelPriceAsync(operatorType).GetAwaiter().GetResult().FuelPrices[fuelType];
    }
    double CalculateFuelCostAsync(double distance, double fuelEfficiency, double fuelPrice)
    {
        return distance * fuelEfficiency * fuelPrice;
    }
}