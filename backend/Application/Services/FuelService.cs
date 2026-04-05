using TMS.Application.Interfaces;
using TMS.Application.Enums;
using TMS.Domain.Interfaces;

namespace TMS.Application.Services;

public class FuelService : IFuelService
{
    private readonly IFuelPriceRepository _fuelPriceRepository;

    public FuelService(IFuelPriceRepository fuelPriceRepository)
    {
        _fuelPriceRepository = fuelPriceRepository;
    }
    public double GetFuelCost(FuelType fuelType, FuelOperatorType operatorType)
    {
        var price = _fuelPriceRepository.GetOperatorFuelPriceAsync(operatorType).GetAwaiter().GetResult().FuelPrices[fuelType];
        return price ?? throw new InvalidOperationException($"Fuel price for {fuelType} is not defined for operator {operatorType}.");
    }
    public double CalculateFuelCostAsync(double distance, double fuelEfficiency, double fuelPrice)
    {
        return distance * fuelEfficiency * fuelPrice;
    }
}