using TMS.Application.Interfaces;
using TMS.Application.Enums;

namespace TMS.Application.Services;

public class FuelService : IFuelService
{
    private readonly IFuelPriceRepository _fuelPriceRepository;

    public FuelService(IFuelPriceRepository fuelPriceRepository)
    {
        _fuelPriceRepository = fuelPriceRepository;
    }
    public double GetFuelCost(FuelType fuelType)
    {
        return _fuelPriceRepository.GetFuelPrice(fuelType);
    }
    public double CalculateFuelCostAsync(double distance, double fuelEfficiency, double fuelPrice)
    {
        // ?
        return distance * fuelEfficiency * fuelPrice;
    }
}