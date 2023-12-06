using JetBrains.Annotations;

public static class Constants
{
    public static bool IS_PRESENTING = false;
    // Energy Costs
    public static float LIGHT_ENERGY_COST_PER_MINUTE = 500;
    public static float HVAC_SYSTEM_COST_PER_MINUTE = 500;
    public static float TIME_UNTIL_PLAYER_LOSES_MONEY = 5f;
    public static float DIVIDE_FACTOR = 50;  // Divide factor to present money costs to user (1_000 too large, but 1 is reasonable)
    
    // Cost Per Item
    public static float COST_TO_PLANT_LENTIL = IS_PRESENTING ? 100_000 : 20_000;
    public static float COST_TO_PLANT_COW = IS_PRESENTING ? 249_999 : 50_000;
    
    // Profit of item
    public static float PROFIT_FROM_HARVESTING_LENTIL = IS_PRESENTING ? 50_000 : 10_000;
    public static float PROFIT_FROM_HARVESTING_COW = 50_000;
    
    // Efficiency Increases
    public static float LED_LIGHT_EFFICIENCY_INCREASE = 0.3f;
    public static float GEOTHERMAL_EFFICIENCY_INCREASE = 0.95f;
    public static float LIGHTNING_ENERGY_PROVIDER_COST_MULTIPLIER = .8f;
    public static float TREE_HUGGER_ENERGY_PROVIDER_COST_MULTIPLIER = 1.2f;
    
    // Money
    public static int PLAYER_START_MONEY = IS_PRESENTING ? 300_000 : 50_000;
    public static int ENEMY_START_MONEY = 500_000;
    public static double PLAYER_PROPORTION_OF_MONEY_SPENT_ON_PLANTS = .5f;
    public static double ENEMY_PROPORTION_OF_MONEY_SPENT_ON_COWS = IS_PRESENTING ? .6f : .3f;
    public static float MONEY_PROPORTION_NEEDED_TO_SEE_GREEN_NEWSPAPER = 0f;
    public static float GEOTHERMAL_ENERGY_COST = IS_PRESENTING ? 20_000 : 200_000;
    private static int GEOTHERMAL_ENERGY_COST_DISPLAYED_TO_USER = (int)(GEOTHERMAL_ENERGY_COST / DIVIDE_FACTOR);
    
    // Times
    public static float ENEMY_SHOOT_TIME = 5f;
    public static float ENEMY_SHOOT_TIME_AFTER_PLAYER_HIT = 2f;
    public static float ENEMY_SHOOT_TIME_DELAY_AFTER_SPAWN = 5f;
    public static float TIME_BEFORE_BULLET_DESPAWNS = 5f;
    
    // Speeds
    public static float ENEMY_SPEED = 0f;
    public static float PLAYER_SPEED = 10f;
    public static float BULLET_SPEED = 20f;
    
    // Jumping
    public static float X_SENSITIVITY = 120f;
    public static float Y_SENSITIVITY = 120f;
    
    // -Movement
    public static float JUMP_HEIGHT = 3f;
    public static float TIME_TO_VERTEX = .6f;
    public static int MAX_JUMPS = 2;
    
    // Other
    public static float ENEMY_FOLLOW_DISTANCE = 4f;
    
    
    // Messages
    public static string GEOTHERMAL_NEWSPAPER_TEXT =
"You can install Geothermal Power for $" + GEOTHERMAL_ENERGY_COST_DISPLAYED_TO_USER + " Geothermal Power will increase your house's energy efficiency by 95% " +
@"and it is better for the enviornment!";
    
    public static string ENERGY_PROVIDER_NEWSPAPER_TEXT =
@"You have two options for your energy provider:

Tree Hugger Power: 30% more expensive than current provider, but generates all the electricity through renewables

Lightning Power: 20% cheaper than current provider, but generates all the electricity using fossil fuels

Press 'X' for Tree Hugger Power

Press 'Y' for Lightning Power
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

    public static string DAY_2_TEXT =
"Day 2 of your adventure. I seem to earn money for each cow I steal and for each lentil I harvest. But that money goes into my piggy bank (or inventory) and I don't seem to get it until after the day ends. My neighbor also seems " +
"to earn money from any lentils and cows I didn't take that day. Anyways, I should probably not get shot by the robot because I heard rumors it put you to sleep. So " +
@"I think the neighbor will take that opportunity to steal my inventory and the remaining lentils + cows.

Press 'X' to continue";

    public static string DAY_3_TEXT =
"I wonder what all the technology around the landscape does. I should probably approach it. After all, those newspapers " +
@"on the door posts and that weird looking security camera could help me some how.

Press 'X' to continue";

    public static string GOT_SHOT_TEXT =
@"That stupid cow farmer's robot shot you. He stole all your inventory and your plants. This is unbelievable! You lost: ";

    public static string GAME_WON_TEXT =
"Congratulations! You won! Your neighbor followed your green habits and is no longer farming cows. He switched to more " +
@"green energy also. The environment is still damaged, but there is hope it will get better.

Press 'X' to start a new game";

    public static string ALL_MONEY_TEXT =
"You Won? You drove that cow farmer out of business, but at what cost. You contributed the destruction of the environment and " +
@"the environment may never recover. Profiting from cows at the expense of the environment and not choosing good energy choices will affect generations to come.

Press 'X' to start a new game and maybe this time think about your actions";

    public static string GREEN_NEWSPAPER_TEXT =
@"Pass this information to everyone you know! Here are the 100 reasons you should be green:

1. Healthy Earth = Healthy You
2. You love a good hug from a tree
3. You care about future generations
.
.
.

Press 'X' to continue";
    
}