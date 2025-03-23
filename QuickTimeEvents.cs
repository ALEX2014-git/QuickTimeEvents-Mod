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
    public abstract class QTEvent
    {
        public enum QTEState { Active, Won, Lost, None }
        public Player targetPlayer { get; protected set; }
        public QuickTimeEventsController controller { get; protected set; }
        public QTEState State { get; protected set; } = QTEState.Active;
        public float Timer { get; protected set; }
        public bool isActive { get; protected set; }
        public enum QTEAction
        {
            MoveLeft,   // Соответствует x = -1
            MoveRight,  // Соответствует x = 1
            MoveUp,     // Соответствует y = 1
            MoveDown,   // Соответствует y = -1
            Jump,       // jmp = true
            Throw,      // thrw = true
            PickUp      // pckp = true
        }

        public bool IsActionPerformed(QTEAction expectedAction, Player.InputPackage input)
        {
            switch (expectedAction)
            {
                case QTEAction.MoveLeft: return input.x == -1 && input.y == 0;
                case QTEAction.MoveRight: return input.x == 1 && input.y == 0;
                case QTEAction.MoveUp: return input.y == 1 && input.x == 0;
                case QTEAction.MoveDown: return input.y == -1 && input.x == 0;
                case QTEAction.Jump: return input.jmp && input.x == 0 && input.y == 0;
                case QTEAction.Throw: return input.thrw && input.x == 0 && input.y == 0;
                case QTEAction.PickUp: return input.pckp && input.x == 0 && input.y == 0;
                default: return false;
            }
        }

        public QTEvent(Player player, QuickTimeEventsController controller)
        {
            this.targetPlayer = player;
            this.controller = controller;
            this.State = QTEState.Active; 
        }
        public abstract void Update();
        public virtual void Destroy()
        {
            this.targetPlayer = null;
            this.controller = null;
            this.State = QTEState.None;
        }
        protected abstract void WinCondition();
        protected abstract void LoseCondition();


    }

    public class ButtonSequenceQTE : QTEvent
    {
        public List<QTEAction> requiredSequence;
        private Player.InputPackage previousInput;
        private int currentStep;
        private int bufferFrames;

        public ButtonSequenceQTE(Player player, QuickTimeEventsController controller) : base(player, controller)
        {
            this.GenerateSequence();
            this.isActive = true;
            this.currentStep = 0;
            this.bufferFrames = 5;
        }

        public void GenerateSequence()
        {
            this.requiredSequence = new List<QTEAction>();
            QTEAction[] possibleActions = {
            QTEAction.MoveLeft,
            QTEAction.MoveRight,
            QTEAction.MoveUp,
            QTEAction.MoveDown };
            for (int i = 0; i < 4; i++) // Генерируем 4 кнопки, потом можно сделать настраиваемый рандом?
            {
                this.requiredSequence.Add(possibleActions[UnityEngine.Random.Range(0, possibleActions.Length)]);
            }
            Custom.LogWarning(new string[]
            {
                $"Generated sequence: {string.Join(", ", this.requiredSequence)}"
            });
            //QTE.Logger.LogInfo($"Generated sequence: {string.Join(", ", this.requiredSequence)}");
        }

        public override void Update()
        {
            if (!isActive) return;

            int playerNumber = targetPlayer.playerState.playerNumber;
            if (ModManager.MSC && targetPlayer.abstractCreature.world.game.IsArenaSession && targetPlayer.abstractCreature.world.game.GetArenaGameSession.chMeta != null)
            {
                playerNumber = 0;
            }
            Player.InputPackage playerInput = RWInput.PlayerInput(playerNumber);
            if (currentStep < requiredSequence.Count)
            {
                QTEAction expectedKey = requiredSequence[currentStep];
                if (!InputEquals(playerInput, previousInput))
                {
                    if (IsActionPerformed(expectedKey, playerInput))
                    {
                        QTE.Logger.LogInfo($"Input: x={playerInput.x}, y={playerInput.y}, jmp={playerInput.jmp}, thrw={playerInput.thrw}, pckp={playerInput.pckp}");
                        bufferFrames = 0;
                        currentStep++;
                        QTE.Logger.LogInfo($"Correct input! Step: {currentStep}/{requiredSequence.Count}");
                    }
                    else if (playerInput.AnyInput)
                    {
                        QTE.Logger.LogInfo($"Input: x={playerInput.x}, y={playerInput.y}, jmp={playerInput.jmp}, thrw={playerInput.thrw}, pckp={playerInput.pckp}");
                        if (bufferFrames > 0)
                        {
                            QTE.Logger.LogInfo("Wrong input, but pity counter saved you!");
                        }
                        else
                        {
                            QTE.Logger.LogInfo("Wrong input! QTE failed.");
                            this.LoseCondition();
                        }
                    }
                }
            }
            else
            {
                QTE.Logger.LogInfo("QTE completed successfully!");
                this.WinCondition();
            }
            previousInput = playerInput;
            bufferFrames--;
        }

        public override void Destroy()
        {
            this.isActive = false;
            base.Destroy();
        }

        protected override void WinCondition()
        {
            this.isActive = false;
            this.State = QTEState.Won;
        }

        protected override void LoseCondition()
        {
            this.isActive = false;
            this.State = QTEState.Lost;          
        }

        private bool InputEquals(Player.InputPackage a, Player.InputPackage b)
        {
            return a.x == b.x
                && a.y == b.y
                && a.jmp == b.jmp
                && a.thrw == b.thrw
                && a.pckp == b.pckp;
        }
    }
}
