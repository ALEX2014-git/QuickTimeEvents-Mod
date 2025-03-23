using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using QTE;
using UnityEngine;

namespace QTE
{
    public partial class QTE
    {
        private void Player_Grabbed(On.Player.orig_Grabbed orig, Player self, Creature.Grasp grasp)
        {
            orig(self, grasp);
            if (grasp.grabber is Lizard || grasp.grabber is Vulture || grasp.grabber is BigSpider || grasp.grabber is DropBug)
            {
                self.GetCustomData().qtEvent = new QuickTimeEventsController(self);
            }
        }

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            var playerInput = self.input[0];
            if (self.GetCustomData().qtEvent != null)
            {
                self.GetCustomData().qtEvent.Update();
            }
            
            orig(self, eu);

            if (Input.GetKey("u") && !TESTBUTTON)
            {
                Logger.LogMessage("Activated NullController");
                self.controller = new Player.NullController();
            }
            TESTBUTTON = Input.GetKey("u");

            if (Input.GetKey("i") && !TESTBUTTON2)
            {
                Logger.LogMessage("Deactivated NullController");
                self.controller = null;
            }
            TESTBUTTON2 = Input.GetKey("i");
        }

            bool TESTBUTTON;
            bool TESTBUTTON2;


        private float Player_DeathByBiteMultiplier(On.Player.orig_DeathByBiteMultiplier orig, Player self)
        {
            if (Instance.options.DisableLizzardRNG.Value)
            {
                return 0;
            }
            return orig(self);
        }
        private void Player_DangerGraspPickup(On.Player.orig_DangerGraspPickup orig, Player self, bool eu)
        {
            if (self.GetCustomData().qtEvent != null) return;
            orig(self, eu);
        }

        private void Player_ThrowToGetFree(On.Player.orig_ThrowToGetFree orig, Player self, bool eu)
        {
            if (self.GetCustomData().qtEvent != null) return;
            orig(self, eu);
        }
    }

    public static class PlayerCWT
    {
        static ConditionalWeakTable<Player, Data> table = new ConditionalWeakTable<Player, Data>();

        public static Data GetCustomData(this Player self) => table.GetOrCreateValue(self);

        public class Data
        {
            public QuickTimeEventsController qtEvent;
        }
    }
}
