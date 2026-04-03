namespace TMS.Application.Interfaces
{
    public interface IFuelService
    {
        // Метод для отримання ціни пального за типом (звернення до зовнішнього API)
        double GetFuelCost(string fuelType);
        // Метод для розрахунку вартості пального на основі відстані, витрати пального та ціни
        double CalculateFuelCostAsync(string distance, double fuelEfficiency, double fuelPrice);
    }
}