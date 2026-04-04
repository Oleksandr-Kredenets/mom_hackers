using TMS.Application.Interfaces;
using TMS.Application.Enums;

namespace TMS.Application.Services;

public class FuelService : IFuelService
{
    double GetFuelCost(TMS.Domain.Enums.FuelType fuelType);
    {}
    double CalculateFuelCostAsync(double distance, double fuelEfficiency, double fuelPrice)
    {}
}