using TMS.Application.Enums;

namespace TMS.Application.Interfaces;

public interface IFuelService
{
    // Метод для отримання ціни пального за типом (звернення до зовнішнього API)
    double GetFuelCost(FuelType fuelType, FuelOperatorType operatorType);
    // Метод для розрахунку вартості пального на основі відстані, витрати пального та ціни
    double CalculateFuelCostAsync(double distance, double fuelEfficiency, double fuelPrice);
}