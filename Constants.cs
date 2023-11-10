public static class Constants
{
    // Energy Costs
    public static float LIGHT_ENERGY_COST_PER_MINUTE = 100;
    public static float HVAC_SYSTEM_COST_PER_MINUTE = 200;
    
    // Cost Per Item
    public static float COST_TO_PLANT_LENTIL = 5_000;
    public static float COST_TO_PLANT_COW = 10_000;
    
    // Profit of item
    public static float PROFIT_FROM_HARVESTING_LENTIL = 100 + COST_TO_PLANT_LENTIL;
    public static float PROFIT_FROM_HARVESTING_COW = 1_000 + COST_TO_PLANT_COW;
    public static float PROFIT_FROM_KILLING_ROBOT = 10_000;
    
    // Efficiency Increases
    public static float LED_LIGHT_EFFICIENCY_INCREASE = 0.3f;
    public static float NEW_HVAC_SYSTEM_EFFICIENCY_INCREASE = 0.8f;
    
    // Money
    public static int PLAYER_START_MONEY = 10_000;
    public static int ENEMY_START_MONEY = 1_000_000;
    public static double PLAYER_PROPORTION_OF_MONEY_SPENT_ON_PLANTS = .5;
    public static double ENEMY_PROPORTION_OF_MONEY_SPENT_ON_PLANTS = .1;
}