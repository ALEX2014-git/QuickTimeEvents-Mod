using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using System.Globalization;
using CoralBrain;
using Expedition;
using HUD;
using JollyCoop;
using JollyCoop.JollyMenu;
using MoreSlugcats;
using Noise;

namespace QTE
{
    public class QuickTimeEventsController
    {
        public int mushroomCounterBeforeQTE;
        public int dangerTimer;
        public Player targetPlayer;
        public Room room;
        public QTEvent qte;
        public int timer;

        public QuickTimeEventsController(Player player)
        {
            QTE.Logger.LogInfo("Creating QTEvent obj");
            this.targetPlayer = player;
            this.room = player.room;
            this.dangerTimer = 0;
            this.mushroomCounterBeforeQTE = targetPlayer.mushroomCounter;
            QTE.Logger.LogInfo($"mushroomCounterBeforeQTE: {this.mushroomCounterBeforeQTE}");
            Init();
        }

        public void Init()
        {
            this.CreateQTE();
            this.SlowTime();
            LockShortcuts();
        }

        public void CreateQTE()
        {
            this.qte = new ButtonSequenceQTE(this, this.targetPlayer, this.room);
            this.timer = (int)Math.Floor(120 * QTE.Instance.options.timerMultiplier.Value);
        }

        public void Update()
        {
            qte.Update();
            QTE.Logger.LogInfo("QTEvent Update()");
            QTE.Logger.LogInfo($"QTE timer {this.dangerTimer}");
            if (targetPlayer.dangerGrasp == null || targetPlayer.dead)
            {
                this.Destroy();
                return;
            }
            targetPlayer.dangerGraspTime = 0;
            if (qte.State == QTEvent.QTEState.Won)
            {
                if (qte.State == QTEvent.QTEState.Won)
                {
                    WonQTE();
                }
            }
            else if (this.dangerTimer > timer || qte.State == QTEvent.QTEState.Lost)
            {
                LostQTE();
            }
            dangerTimer++;
        }

        public void WonQTE()
        {
            for (int i = 0; i < targetPlayer.grabbedBy.Count; i++)
            {
                QTE.Logger.LogMessage($"Iteration {i + 1} of grabbedBy property");
                if (targetPlayer.grabbedBy[i] != null)
                {
                    QTE.Logger.LogMessage($"Trying to release player");
                    try
                    {
                        var crit = targetPlayer.grabbedBy[i].grabber;
                        crit.ReleaseGrasp(targetPlayer.grabbedBy[i].graspUsed);
                        targetPlayer.stun = 0;
                        targetPlayer.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, crit.firstChunk.pos, 1f, 2f);
                        crit.Die();
                        qte.Destroy();
                        this.Destroy();
                    }
                    catch (Exception ex)
                    {
                        QTE.Logger.LogError(ex);
                        throw;
                    }

                }
            }
        }

        public void LostQTE()
        {
            QTE.Logger.LogInfo("QTE Failed");
            this.qte?.Lose();
            targetPlayer.dangerGraspTime = 60;
            if (QTE.Instance.options.PunishFailure.Value) targetPlayer.Die();
            qte.Destroy();
            this.Destroy();
        }

        public void SlowTime()
        {
            targetPlayer.mushroomCounter = 9999;
            this.targetPlayer.mushroomEffect = 0.75f;
            if (QTE.Instance.options.TimeSlowMode.Value == "Stop")
            {
                this.targetPlayer.abstractCreature.world.game.pauseUpdate = true;
            }
        }

        private void LockShortcuts()
        {
            if (this.room.lockedShortcuts.Count == 0)
            {
                for (int i = 0; i < this.room.shortcutsIndex.Length; i++)
                {
                    this.room.lockedShortcuts.Add(this.room.shortcutsIndex[i]);
                }
            }
        }

        private void UnlockShortcuts()
        {
            this.room.lockedShortcuts.Clear();
        }

        public virtual void Destroy()
        {
            UnlockShortcuts();
            this.targetPlayer.abstractCreature.world.game.pauseUpdate = false;
            this.qte?.Destroy();
            this.qte = null;
            targetPlayer.mushroomCounter = mushroomCounterBeforeQTE;
            QTE.Logger.LogInfo($"Destroying QTEobj, owner: {this.targetPlayer}");
            targetPlayer.GetCustomData().qtEvent = null;
        }
    }
}
