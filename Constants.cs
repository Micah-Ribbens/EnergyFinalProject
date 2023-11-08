public static class Constants
{
    // Energy Costs
    private static float LIGHT_ENERGY_COST_PER_MINUTE = 1;
    private static float HVAC_SYSTEM_COST_PER_MINUTE = 2;
    
    // Cost Per Item
    private static float COST_TO_PLANT_LENTIL = 30;
    private static float COST_TO_PLANT_COW = 100;
    
    // Profit of item
    private static float PROFIT_FROM_HARVESTING_LENTIL = 100 + COST_TO_PLANT_LENTIL;
    private static float PROFIT_FROM_HARVESTING_COW = 1_000 + COST_TO_PLANT_COW;
    private static float PROFIT_FROM_KILLING_ROBOT = 10_000;
    
    // Efficiency Increases
    private static float LED_LIGHT_EFFICIENCY_INCREASE = 0.3f;
    private static float NEW_HVAC_SYSTEM_EFFICIENCY_INCREASE = 0.8f;
}