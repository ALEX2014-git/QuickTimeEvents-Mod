using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QTE
{
        public class QuickTimeEventsController
        {
            public QuickTimeEventsController(Player player)
            {
                QTE.Logger.LogInfo("Creating QTEvent obj");
                this.targetPlayer = player;
                this.dangerTimer = 0;
                this.mushroomCounterBeforeQTE = targetPlayer.mushroomCounter;
                QTE.Logger.LogInfo($"mushroomCounterBeforeQTE: {this.mushroomCounterBeforeQTE}");
                if (targetPlayer.controller == null)
                {
                    QTE.Logger.LogInfo($"Initiated player controller");
                    //targetPlayer.controller = new Player.NullController();            
                }
                this.CreateQTE();
                this.SlowTime();
            }

            public void CreateQTE()
            {
                this.qte = new ButtonSequenceQTE(this.targetPlayer, this);
            }

            public void Update()
            {
                qte.Update();
                QTE.Logger.LogInfo("QTEvent Update()");
                QTE.Logger.LogInfo($"QTE timer {this.dangerTimer}");
                if (targetPlayer.dangerGrasp == null || targetPlayer.dead)
                {
                    this.qte.Destroy();
                    this.Destroy();
                    return;
                }
                this.SlowTime();
                if (qte.State == QTEvent.QTEState.Won)
                {                                 
                    if (qte.State == QTEvent.QTEState.Won)
                    {
                        WonQTE();
                    }
                }
                else if (this.dangerTimer > 60 || qte.State == QTEvent.QTEState.Lost)
                {
                    LostQTE();
                }
                dangerTimer++;
            }

            public void WonQTE()
            {
                for (int i = 0; i < targetPlayer.grabbedBy.Count; i++)
                {
                    QTE.Logger.LogMessage($"Iteration {i} of grabbedBy property");
                    if (targetPlayer.grabbedBy[i] != null)
                    {
                        QTE.Logger.LogMessage($"Trying to release player");
                        try
                        {
                            var crit = targetPlayer.grabbedBy[i].grabber;
                            crit.ReleaseGrasp(targetPlayer.grabbedBy[i].graspUsed);
                            targetPlayer.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, crit.firstChunk.pos, 1f, 2f);
                            crit.Die();
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
                targetPlayer.dangerGraspTime = 60;
                if (QTE.Instance.options.PunishFailure.Value) targetPlayer.Die();
                this.Destroy();
            }

            public void SlowTime()
            {
                targetPlayer.mushroomCounter = 25;
            }

            public void Destroy()
            {
                this.qte = null;
                targetPlayer.controller = null;
                targetPlayer.mushroomCounter = mushroomCounterBeforeQTE;
                QTE.Logger.LogInfo($"Destroying QTEobj, owner: {this.targetPlayer}");
                targetPlayer.GetCustomData().qtEvent = null;
            }

            public int mushroomCounterBeforeQTE;
            private bool TESTBUTTON;
            public int dangerTimer;
            public Player targetPlayer;
            public QTEvent qte;
        }

}
