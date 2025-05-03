using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTE
{
    public partial class QTE
    {
        private void Room_Update(On.Room.orig_Update orig, Room self)
        {
            orig(self);
            foreach (AbstractCreature abstractCreature in self.game.Players)
            {
                if (abstractCreature.realizedCreature != null && abstractCreature.realizedCreature.room == self)
                {
                    Player player = abstractCreature.realizedCreature as Player;
                    player.GetCustomData().qtEvent?.Update();
                }
            }
        }
    }
}
