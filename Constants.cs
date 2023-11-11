using JetBrains.Annotations;

public static class Constants
{
    // Energy Costs
    public static float LIGHT_ENERGY_COST_PER_MINUTE = 100;
    public static float HVAC_SYSTEM_COST_PER_MINUTE = 200;
    
    // Cost Per Item
    public static float COST_TO_PLANT_LENTIL = 5_000;
    public static float COST_TO_PLANT_COW = 10_000;
    
    // Profit of item
    public static float PROFIT_FROM_HARVESTING_LENTIL = 100;
    public static float PROFIT_FROM_HARVESTING_COW = 1_000;
    public static float PROFIT_FROM_KILLING_ROBOT = 10_000;
    
    // Efficiency Increases
    public static float LED_LIGHT_EFFICIENCY_INCREASE = 0.3f;
    public static float GEOTHERMAL_EFFICIENCY_INCREASE = 0.95f;
    public static float LIGHTNING_ENERGY_PROVIDER_COST_MULTIPLIER = .8f;
    public static float TREE_HUGGER_ENERGY_PROVIDER_COST_MULTIPLIER = 1.2f;
    
    // Money
    public static int PLAYER_START_MONEY = 10_000;
    public static int ENEMY_START_MONEY = 1_000_000;
    public static double PLAYER_PROPORTION_OF_MONEY_SPENT_ON_PLANTS = .5;
    public static double ENEMY_PROPORTION_OF_MONEY_SPENT_ON_PLANTS = .1;
    public static float MONEY_PROPORTION_NEEDED_TO_SEE_GREEN_NEWSPAPER = 0.5f;
    
    // Messages
    public static string GEOTHERMAL_NEWSPAPER_TEXT =
"You can install Geothermal Power for $3, 000 Geothermal Power will increase your house's energy efficiency by 95% " +
@"and it is better for the enviornment!

Press 'X' to install Geothermal Power
Press 'B' to deny";

    public static string ENERGY_PROVIDER_NEWSPAPER_TEXT =
@"You have two options for your energy provider:

Tree Hugger Power: 30% more expensive than current provider, but generates all the electricity through renewables

Lightning Power: 20% cheaper than current provider, but generates all the electricity using fossil fuels

Press 'X' for Tree Hugger Power

Press 'B' for Lightning Power
";

    public static string LED_CAMERA_TEXT =
@"You have discovered LED technology! LED is 30% more energy efficient than what you have currently and it is better for the environment!

Press 'X' to continue";

    public static string GAME_OVER_TEXT = @"Game Over. You have lost all your money. Let's try to stop that cow farmer again!

Press 'X' to start a new game";

    public static string DAY_1_TEXT =
"Day 1 of your adventure. That stupid cow farming neighbor is destroying the environment. You are a lentil farmer, " +
"which is much better for the environment. You must stop him from destroying the environment, but how? You could somehow run him out of business. " +
@"If you got all the share of money that would work (obviously there are no ulterior motives)!

Press 'X' to continue";

    public static string GOT_SHOT_TEXT =
@"Have a pleasant nap. Oh I almost forgot to thank you. Thanks for all the plants and your inventory too? You're too kind.

Pleasure Doing Business With You,
Your Friendly Cow Farmer

Press 'X' to continue";
    
    


}