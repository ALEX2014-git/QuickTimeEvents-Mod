using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace QTE
{
    public partial class QTE
    {
        private void PlayerGraphics_Update(ILContext il)
        {
            var c = new ILCursor(il);
            var d = new ILCursor(il);
            c.GotoNext(
                MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(typeof(PlayerGraphics).GetField("player", BindingFlags.NonPublic | BindingFlags.Instance)),
                x => x.MatchCallvirt(typeof(Player).GetMethod("get_Adrenaline")),
                x => x.MatchLdcR4(0f),
                x => x.MatchBleUn(out _)
                       );
            c.MoveAfterLabels();
            d.GotoNext(
                MoveType.After,
                x => x.MatchCall(typeof(UnityEngine.Random).GetMethod("get_value")),
                x => x.MatchLdcR4(0.05f),
                x => x.MatchBgeUn(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(typeof(PlayerGraphics).GetField("blink")),
                x => x.MatchLdcI4(3),
                x => x.MatchCall(typeof(System.Math).GetMethod("Max", new Type[] { typeof(Int32), typeof(Int32) })),
                x => x.MatchStfld(typeof(PlayerGraphics).GetField("blink"))
           );
            var endCode = d.DefineLabel();
            d.MarkLabel(endCode);

            c.Emit(OpCodes.Ldarg_0);
            bool IsThereActiveQTE(PlayerGraphics this_arg)
            {
                if (this_arg.player.GetCustomData().qtEvent != null)
                {
                    Logger.LogWarning($"{this_arg.player} has active QTE event, skipping mushroom graphics update");
                    return true;
                }
                return false;
            }
            c.EmitDelegate<Func<PlayerGraphics, bool>>(IsThereActiveQTE);
            c.Emit(OpCodes.Brtrue, endCode);
        }
    }
}
