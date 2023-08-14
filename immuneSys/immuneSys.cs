using GTA;
using GTA.Native;
using GTA.Math;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using LemonUI;
using LemonUI.TimerBars;
using System.IO; 
using System.Runtime.InteropServices;

namespace immuneSys
{
    public class immuneSys : Script
    {

        private static readonly ObjectPool pool = new ObjectPool();
        private static readonly TimerBarCollection bar_pool = new TimerBarCollection();
        private static readonly TimerBarProgress rainBar = new TimerBarProgress("CLOTHES") { Progress = 0.0f };
        private static readonly TimerBarProgress immBar = new TimerBarProgress("IMMUNITY") { Progress = 0.0f };
      

        float bar_zero = 0.0f; // Wet Clothes Degree: Michael
        float bar_one = 0.0f; // Wet Degree: Franklin
        float bar_two = 0.0f; // Wet Degree: Trevor

        int jump_zero = 0; // Number of jumps: Michael
        int jump_one = 0; // Number of jumps: Franklin
        int jump_two = 0; // Number of jumps: Trevor

        float sprint_zero = 0.0f; // Running length: Michael
        float sprint_one = 0.0f; // Running length: Franklin
        float sprint_two = 0.0f; // Running length: Trevor

        int veh_zero = 0; // Degree of heat: Michael
        int veh_one = 0; // Degree of heat: Franklin
        int veh_two = 0; // Degree of heat: Trevor

        int water_zero = 0; // Swim timer: Michael
        int water_one = 0; // Swim timer: Franklin
        int water_two = 0; // Swim timer: Trevor

        float imm_bar_zero = 0.0f; // Immunity: Michael
        float imm_bar_one = 0.0f; // Immunity: Franklin
        float imm_bar_two = 0.0f; // Immunity: Trevor

        int health_null_zero = 0; // Health Reduction Timer: Michael
        int health_null_one = 0; // Health Reduction Timer: Trevor
        int health_null_two = 0; // Health Reduction Timer: Michael

        int imm_show = 0; // Show immunity progress bar (immBar)

        int help_m1 = 0; // Show notification when clothes are wet
        int help_m2 = 0; // Show notification when Michael's immunity is zero
        int help_m3 = 0; // Show notification when Franklin's immunity is zero
        int help_m4 = 0; // Show notification when Trevor's immunity is zero

        int DeadEvent = 0; // 0 - the player is alive, 1 - the player is dead
        int LockDead = 0; // Disable the ability to remove immunity repeatedly at the time of death     
        Keys enable;

        static string path = @".\scripts\immuneSys.ini";
        ScriptSettings config = ScriptSettings.Load(path);

        //****************Value balancing//****************//

        private static readonly int ImmuneCountValue = 1;                   
        private static readonly int ImmuneHealthValue = 5;
        private static readonly int JumpsValue = 1;                       
        private static readonly float SprintValue = 0.1f;                 
        private static readonly int PlayerInWaterValue = 1;         
        private static readonly float PlayerJumpToImmunValue = 0.2f;
        private static readonly float PlayerSprintToImmunValue = 0.2f;
        private static readonly float PlayerWaterToImmunValue = 0.2f;
        private static readonly float PlayerVehToImmunValue = 0.2f;
        private static readonly int AutoSmokeValue = 1;
        private static readonly float DeadEventValue = 0.2f; 
        private static readonly float SetImmunMinusValue = 0.10f; 
        private static readonly float SetImmunPlusValue = 0.10f;
        private static readonly float minImmunValue = 2.0f;
        private static readonly float maxImmunValue = 99.0f;
        private static readonly int maxTimerJumpsValue = 20;
        private static readonly int maxTimerSprintValue = 150;
        private static readonly int maxTimerWaterValue = 1000;
        private static readonly int maxTimerVehValue = 5000;
        private static readonly float maxRainLevelValue = 90.0f;

        public immuneSys()
        {

            rainBar.BackgroundColor = Color.Black;
            rainBar.ForegroundColor = Color.Red;
            bar_pool.Add(rainBar);

            immBar.BackgroundColor = Color.Black;
            immBar.ForegroundColor = Color.Green;
            bar_pool.Add(immBar);

            pool.Add(bar_pool);

            imm_bar_zero = config.GetValue<float>("MAIN", "ZERO", 100.0f);
            imm_bar_one = config.GetValue<float>("MAIN", "ONE", 100.0f);
            imm_bar_two = config.GetValue<float>("MAIN", "TWO", 100.0f);
            enable = config.GetValue<Keys>("KEYS", "ENABLE", Keys.I);

            Tick += OnTick;
            KeyDown += OnKeyDown;
        }

        void OnTick(object sender, EventArgs e)
        {
            DrawBarOnFrame();
            SetWetClothes();
            SetDryClothes();
            PlayerBarToBarPercentage();
            PlayerImmunSys();
            ImmunBarToBarPercentage();
            ShowHelpMess();
            OnDeadEvent();
            PlayerJump();
            PlayerSprint();
            PlayerJumpToImmun();
            PlayerSprintToImmun();
            PlayerVehToImmun();
            AutoSmoke();
            PlayerWaterToImmun();
            PlayerInWater();
            SpecialAbilityMAX();
            SpecialAbilityLock();
            ImmuneCount();
            ImmuneHealth();


        }
        
        void ImmuneCount()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash && minImmunValue >= imm_bar_zero && 200 > health_null_zero)
            {
                health_null_zero += ImmuneCountValue;
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash && minImmunValue >= imm_bar_one && 200 > health_null_one)
                {
                    health_null_one += ImmuneCountValue;
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash && minImmunValue >= imm_bar_two && 200 > health_null_two)
                    {
                        health_null_two += ImmuneCountValue;
                    }
                }
            }
        }

        void ImmuneHealth()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash && minImmunValue >= imm_bar_zero && health_null_zero >= 200)
            {
                health_null_zero = 0;
                Game.Player.Character.Health -= ImmuneHealthValue;
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash && minImmunValue >= imm_bar_one && health_null_one >= 200)
                {
                    health_null_one = 0;
                    Game.Player.Character.Health -= ImmuneHealthValue;

                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash && minImmunValue >= imm_bar_two && health_null_two >= 200)
                    {
                        health_null_two = 0;
                        Game.Player.Character.Health -= ImmuneHealthValue;
                    }
                }
            }
        }

        void SpecialAbilityMAX()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash && imm_bar_zero >= maxImmunValue && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_SPECIAL_ABILITY_ENABLED, Game.Player) == true)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.SPECIAL_ABILITY_CHARGE_LARGE, Game.Player, true, true);
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash && imm_bar_one >= maxImmunValue && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_SPECIAL_ABILITY_ENABLED, Game.Player) == true)
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.SPECIAL_ABILITY_CHARGE_LARGE, Game.Player, true, true);
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash && imm_bar_two >= maxImmunValue && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_SPECIAL_ABILITY_ENABLED, Game.Player) == true)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.SPECIAL_ABILITY_CHARGE_LARGE, Game.Player, true, true);
                    }
                }
            }
        }

        void SpecialAbilityLock()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash && minImmunValue > imm_bar_zero && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_SPECIAL_ABILITY_ENABLED, Game.Player) == true)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.SPECIAL_ABILITY_RESET, Game.Player);            
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash && minImmunValue > imm_bar_one && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_SPECIAL_ABILITY_ENABLED, Game.Player) == true)
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.SPECIAL_ABILITY_RESET, Game.Player);
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash && minImmunValue > imm_bar_two && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_SPECIAL_ABILITY_ENABLED, Game.Player) == true)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.SPECIAL_ABILITY_RESET, Game.Player);
                    }
                }
            }
        }

        void PlayerJump()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash && GTA.Native.Function.Call<float>(GTA.Native.Hash.GET_RAIN_LEVEL) == 0.0 && Game.Player.Character.IsJumping)
            {
                jump_zero += JumpsValue;
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash && GTA.Native.Function.Call<float>(GTA.Native.Hash.GET_RAIN_LEVEL) == 0.0 && Game.Player.Character.IsJumping)
                {
                    jump_one += JumpsValue;
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash && GTA.Native.Function.Call<float>(GTA.Native.Hash.GET_RAIN_LEVEL) == 0.0 && Game.Player.Character.IsJumping)
                    {
                        jump_two += JumpsValue;
                    }
                }
            }
        }

        void PlayerSprint()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash && GTA.Native.Function.Call<float>(GTA.Native.Hash.GET_RAIN_LEVEL) == 0.0 && Game.Player.Character.IsSprinting)
            {
                sprint_zero += SprintValue;
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash && GTA.Native.Function.Call<float>(GTA.Native.Hash.GET_RAIN_LEVEL) == 0.0 && Game.Player.Character.IsSprinting)
                {
                    sprint_one += SprintValue;
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash && GTA.Native.Function.Call<float>(GTA.Native.Hash.GET_RAIN_LEVEL) == 0.0 && Game.Player.Character.IsSprinting)
                    {
                        sprint_two += SprintValue;
                    }
                }
            }
        }

        void PlayerInWater() 
        {
            if (GTA.World.Weather == GTA.Weather.ExtraSunny && Game.Player.Character.IsInWater && Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
            {
                water_zero += PlayerInWaterValue;
            }
            else
            {
                if (GTA.World.Weather == GTA.Weather.ExtraSunny && Game.Player.Character.IsInWater && Game.Player.Character.Model.Hash == new Model("player_one").Hash)
                {
                    water_one += PlayerInWaterValue;
                }
                else
                {
                    if (GTA.World.Weather == GTA.Weather.ExtraSunny && Game.Player.Character.IsInWater && Game.Player.Character.Model.Hash == new Model("player_two").Hash)
                    {
                        water_two += PlayerInWaterValue;
                    }
                }
            }
        }

        void PlayerJumpToImmun()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash && jump_zero >= maxTimerJumpsValue)
            {
                jump_zero = 0;
                imm_bar_zero += PlayerJumpToImmunValue;
                config.SetValue<float>("MAIN", "ZERO", imm_bar_zero);
                config.Save();
                GTA.UI.Notification.Show("Your immune system has begun to improve.");
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash && jump_one >= maxTimerJumpsValue)
                {
                    jump_one = 0;
                    imm_bar_one += PlayerJumpToImmunValue;
                    config.SetValue<float>("MAIN", "ONE", imm_bar_one);
                    config.Save();
                    GTA.UI.Notification.Show("Your immune system has begun to improve.");
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash && jump_two >= maxTimerJumpsValue)
                    {
                        jump_two = 0;
                        imm_bar_two += PlayerJumpToImmunValue;
                        config.SetValue<float>("MAIN", "TWO", imm_bar_two);
                        config.Save();
                        GTA.UI.Notification.Show("Your immune system has begun to improve.");
                    }
                }
            }
        }

        void PlayerSprintToImmun()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash && sprint_zero >= maxTimerSprintValue)
            {
                sprint_zero = 0;
                imm_bar_zero += PlayerSprintToImmunValue;
                config.SetValue<float>("MAIN", "ZERO", imm_bar_zero);
                config.Save();
                GTA.UI.Notification.Show("Your immune system has begun to improve.");
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash && sprint_one >= maxTimerSprintValue)
                {
                    sprint_one = 0;
                    imm_bar_one += PlayerSprintToImmunValue;
                    config.SetValue<float>("MAIN", "ONE", imm_bar_one);
                    config.Save();
                    GTA.UI.Notification.Show("Your immune system has begun to improve.");
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash && sprint_two >= maxTimerSprintValue)
                    {
                        sprint_two = 0;
                        imm_bar_two += PlayerSprintToImmunValue;
                        config.SetValue<float>("MAIN", "TWO", imm_bar_two);
                        config.Save();
                        GTA.UI.Notification.Show("Your immune system has begun to improve.");
                    }
                }
            }
        }

        void PlayerWaterToImmun()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash && water_zero >= maxTimerWaterValue)
            {
                water_zero = 0;
                imm_bar_zero += PlayerWaterToImmunValue;
                config.SetValue<float>("MAIN", "ZERO", imm_bar_zero);
                config.Save();
                GTA.UI.Notification.Show("Your immune system has begun to improve.");
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash && water_one >= maxTimerWaterValue)
                {
                    water_one = 0;
                    imm_bar_one += PlayerWaterToImmunValue;
                    config.SetValue<float>("MAIN", "ONE", imm_bar_one);
                    config.Save();
                    GTA.UI.Notification.Show("Your immune system has begun to improve.");
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash && water_two >= maxTimerWaterValue)
                    {
                        water_two = 0;
                        imm_bar_two += PlayerWaterToImmunValue;
                        config.SetValue<float>("MAIN", "TWO", imm_bar_two);
                        config.Save();
                        GTA.UI.Notification.Show("Your immune system has begun to improve.");
                    }
                }
            }
        }

        void PlayerVehToImmun()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash && veh_zero >= maxTimerVehValue && GTA.World.Weather == GTA.Weather.ExtraSunny)
            {
                veh_zero = 0;
                imm_bar_zero -= PlayerVehToImmunValue;
                config.SetValue<float>("MAIN", "ZERO", imm_bar_zero);
                config.Save();
                GTA.UI.Notification.Show("It's very hot in the car. Immunity has started to decline.");
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash && veh_one >= maxTimerVehValue && GTA.World.Weather == GTA.Weather.ExtraSunny)
                {
                    veh_one = 0;
                    imm_bar_one -= PlayerVehToImmunValue;
                    config.SetValue<float>("MAIN", "ONE", imm_bar_one);
                    config.Save();
                    GTA.UI.Notification.Show("It's very hot in the car. Immunity has started to decline.");
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash && veh_two >= maxTimerVehValue && GTA.World.Weather == GTA.Weather.ExtraSunny)
                    {
                        veh_two = 0;
                        imm_bar_two -= PlayerVehToImmunValue;
                        config.SetValue<float>("MAIN", "TWO", imm_bar_two);
                        config.Save();
                        GTA.UI.Notification.Show("It's very hot in the car. Immunity has started to decline.");
                    }
                }
            }
        }

        void AutoSmoke()
        {
            if(GTA.World.Weather == GTA.Weather.ExtraSunny && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_SITTING_IN_ANY_VEHICLE, Game.Player.Character) == true && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_ON_ANY_BIKE, Game.Player.Character) == false && Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
            {
                veh_zero += AutoSmokeValue;
            }
            else
            {
                if (GTA.World.Weather == GTA.Weather.ExtraSunny && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_SITTING_IN_ANY_VEHICLE, Game.Player.Character) == true && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_ON_ANY_BIKE, Game.Player.Character) == false && Game.Player.Character.Model.Hash == new Model("player_one").Hash)
                {
                    veh_one += AutoSmokeValue;
                }
                else
                {
                    if (GTA.World.Weather == GTA.Weather.ExtraSunny && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_SITTING_IN_ANY_VEHICLE, Game.Player.Character) == true && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_ON_ANY_BIKE, Game.Player.Character) == false && Game.Player.Character.Model.Hash == new Model("player_two").Hash)
                    {
                        veh_two += AutoSmokeValue;              
                    }
                }
            }
        }

        void OnDeadEvent()
        {
            if(GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_ENTITY_DEAD, Game.Player.Character) == false)
            {
                DeadEvent = 0;
                LockDead = 0;
            }
            else
            {
                DeadEvent = 1;           
            }

            if(DeadEvent == 1 && LockDead == 0)
            {
                if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash && imm_bar_zero >= DeadEventValue)
                {
                    imm_bar_zero -= DeadEventValue;
                    config.SetValue<float>("MAIN", "ZERO", imm_bar_zero);
                    config.Save();
                    bar_zero = 0.0f;
                    LockDead = 1;
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_one").Hash && imm_bar_one >= DeadEventValue)
                    {
                        imm_bar_one -= DeadEventValue;
                        config.SetValue<float>("MAIN", "ONE", imm_bar_one);
                        config.Save();
                        bar_one = 0.0f;
                        LockDead = 1;
                    }
                    else
                    {
                        if (Game.Player.Character.Model.Hash == new Model("player_two").Hash && imm_bar_two >= DeadEventValue)
                        {
                            imm_bar_two -= DeadEventValue;
                            config.SetValue<float>("MAIN", "TWO", imm_bar_two);
                            config.Save();
                            bar_two = 0.0f;
                            LockDead = 1;
                        }
                    }
                }
            }
        }

        void ShowHelpMess()
        {
            if (help_m1 == 0 && rainBar.Progress >= maxRainLevelValue)
            {
                GTA.UI.Notification.Show("Your clothes are soaked, your immune system has begun to decline.");
                help_m1 = 1;
            }
            else
            {
                if (help_m2 == 0 && minImmunValue >= imm_bar_zero && Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
                {
                    GTA.UI.Notification.Show("Your immune system is weakened. Your health is beginning to decline.");
                    help_m2 = 1;
                }
                else
                {
                    if (help_m3 == 0 && minImmunValue >= imm_bar_one && Game.Player.Character.Model.Hash == new Model("player_one").Hash)
                    {
                        GTA.UI.Notification.Show("Your immune system is weakened. Your health is beginning to decline.");
                        help_m3 = 1;
                    }
                    else
                    {
                        if (help_m4 == 0 && minImmunValue >= imm_bar_two && Game.Player.Character.Model.Hash == new Model("player_two").Hash)
                        {
                            GTA.UI.Notification.Show("Your immune system is weakened. Your health is beginning to decline.");
                            help_m4 = 1;
                        }
                    }
                }
            }
        }

        void DrawBarOnFrame()
        {
            if(rainBar.Progress > 0.0 || imm_show == 1)
            {
                if(GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PLAYER_SWITCH_IN_PROGRESS) == false)
                {
                    pool.Process();
                }      
            }
        }

        void PlayerImmunSys()
        {
            if (rainBar.Progress >= maxRainLevelValue)
            {
                SetImmunMinus();
            }
        }

        void PlayerBarToBarPercentage()
        {
            if(Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
            {
                if (bar_zero <= 100.0)
                {
                    rainBar.Progress = bar_zero;
                }
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash)
                {
                    if (bar_one <= 100.0)
                    {
                        rainBar.Progress = bar_one;
                    }
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash)
                    {
                        if (bar_two <= 100.0)
                        {
                            rainBar.Progress = bar_two;
                        }
                    }
                }
            }       
        }

        void SetImmunMinus()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
            {
                if (imm_bar_zero >= SetImmunMinusValue)
                {
                    imm_bar_zero -= SetImmunMinusValue;
                    config.SetValue<float>("MAIN", "ZERO", imm_bar_zero);
                    config.Save();
                }
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash)
                {
                    if (imm_bar_one >= SetImmunMinusValue)
                    {
                        imm_bar_one -= SetImmunMinusValue;
                        config.SetValue<float>("MAIN", "ONE", imm_bar_one);
                        config.Save();
                    }
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash)
                    {
                        if (imm_bar_two >= SetImmunMinusValue)
                        {
                            imm_bar_two -= SetImmunMinusValue;
                            config.SetValue<float>("MAIN", "TWO", imm_bar_two);
                            config.Save();
                        }
                    }
                }
            }
        }

        void ImmunBarToBarPercentage()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
            {
                immBar.Progress = imm_bar_zero;
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash)
                {
                    immBar.Progress = imm_bar_one;
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash)
                    {
                        immBar.Progress = imm_bar_two;
                    }
                }
            }
        }

        void SetWetClothes()
        {
            if(GTA.Native.Function.Call<float>(GTA.Native.Hash.GET_RAIN_LEVEL) > 0.0 && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_SITTING_IN_ANY_VEHICLE, Game.Player.Character) == false || GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_ON_ANY_BIKE, Game.Player.Character) == true)
            {
                if(GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_INTERIOR_SCENE) == false && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PLAYER_SWITCH_IN_PROGRESS) == false && rainBar.Progress < 100.0)
                {
                    SetPlayerBarPlus();
                }
            }

        }

        void SetDryClothes()
        {
            if(GTA.Native.Function.Call<float>(GTA.Native.Hash.GET_RAIN_LEVEL) == 0.0 || GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_INTERIOR_SCENE) == true || GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_SITTING_IN_ANY_VEHICLE, Game.Player.Character) == true && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_ON_ANY_BIKE, Game.Player.Character) == false)     
            {
                if(GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PLAYER_SWITCH_IN_PROGRESS) == false && rainBar.Progress > 0.0)
                {
                    SetPlayerBarMinus();
                }             
            }

        }

        void SetPlayerBarPlus()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
            {
                if (bar_zero <= 100.0)
                {
                    bar_zero += SetImmunPlusValue;
                }

            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash)
                {
                    if (bar_one <= 100.0)
                    {
                        bar_one += SetImmunPlusValue;
                    }
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash)
                    {
                        if (bar_two <= 100.0)
                        {
                            bar_two += SetImmunPlusValue;
                        }
                    }
                }
            }
        }

        void SetPlayerBarMinus()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
            {
                if (bar_zero > SetImmunPlusValue)
                {
                    bar_zero -= SetImmunPlusValue;
                }
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash)
                {
                    if (bar_one > SetImmunPlusValue)
                    {
                        bar_one -= SetImmunPlusValue;
                    }
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash)
                    {
                        if (bar_two > SetImmunPlusValue)
                        {
                            bar_two -= SetImmunPlusValue;
                        }
                    }
                }
            }
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == enable && 0 >= rainBar.Progress)
            {
                if(imm_show == 0)
                {
                    imm_show = 1;
                }
                else
                {
                    imm_show = 0;
                }
            }          
        } 
    }
}

