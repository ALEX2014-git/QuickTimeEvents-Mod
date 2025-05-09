﻿using System;
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
    public class QTEvent
    {
        public QuickTimeEventsGraphics qteGraphics;
        public List<QTEAction> requiredSequence;
        public enum QTEState { Active, Won, Lost, Destroy }
        public Player player { get; protected set; }
        public Room room { get; protected set; }
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

        public QTEvent(QuickTimeEventsController controller, Player player, Room room)
        {
            this.player = player;
            this.room = room;
            this.controller = controller;
            this.State = QTEState.Active; 
        }

        public virtual void Update()
        {
        }

        public virtual void Destroy()
        {
            this.player = null;
            this.controller = null;
            this.qteGraphics = null;
            this.State = QTEState.Destroy;
        }

        public virtual void Win()
        {
            if (this.isActive == false) return;
        }

        public virtual void Lose()
        {
            if (this.isActive == false) return;     
        }

    }

    public class ButtonSequenceQTE : QTEvent
    {
        private Player.InputPackage previousInput;
        private int currentStep;
        private int bufferFrames;

        public ButtonSequenceQTE(QuickTimeEventsController controller, Player player, Room room) : base(controller, player, room)
        {
            this.GenerateSequence();
            this.isActive = true;
            this.currentStep = 0;
            this.bufferFrames = 5;
            this.qteGraphics = new QuickTimeEventButtonSequenceGraphic(this, this.player, this.room);
            QTE.Logger.LogWarning("Created QTEGraphics");
            this.room.AddObject(this.qteGraphics);
        }

        public void GenerateSequence()
        {
            this.requiredSequence = new List<QTEAction>();
            QTEAction[] possibleActions = {
            QTEAction.MoveLeft,
            QTEAction.MoveRight,
            QTEAction.MoveUp,
            QTEAction.MoveDown };
            for (int i = 0; i < QTE.Instance.options.buttonSequenceAmmount.Value; i++) // Генерируем 4 кнопки, потом можно сделать настраиваемый рандом?
            {
                this.requiredSequence.Add(possibleActions[UnityEngine.Random.Range(0, possibleActions.Length)]);
            }
            Custom.LogWarning(new string[]
            {
                $"Generated sequence: {string.Join(", ", this.requiredSequence)}"
            });
        }

        public override void Update()
        {
            base.Update();
            if (!isActive) return;

            int playerNumber = player.playerState.playerNumber;
            if (ModManager.MSC && player.abstractCreature.world.game.IsArenaSession && player.abstractCreature.world.game.GetArenaGameSession.chMeta != null)
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
                        //bufferFrames = 0; Don't revoke buffer frames after first succeseful input?
                        (this.qteGraphics as QuickTimeEventButtonSequenceGraphic).buttons[currentStep].State = QuickTimeEventsGraphics.GraphicsPart.QTEKey.KeyState.Completed;
                        currentStep++;                       
                        player.room.PlaySound(SoundID.MENU_Karma_Ladder_Increase_Bump, player.firstChunk.pos, 1f, 3f + ((currentStep - 1) / 2f));
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
                            (this.qteGraphics as QuickTimeEventButtonSequenceGraphic).buttons[currentStep].State = QuickTimeEventsGraphics.GraphicsPart.QTEKey.KeyState.Failed;
                            QTE.Logger.LogInfo("Wrong input! QTE failed.");
                            this.Lose();
                        }
                    }
                }
            }
            else
            {
                QTE.Logger.LogInfo("QTE completed successfully!");
                this.Win();
            }
            previousInput = playerInput;
            bufferFrames--;
        }

        public override void Destroy()
        {
            this.isActive = false;
            base.Destroy();
        }

        public override void Win()
        {
            base.Win();
            this.isActive = false;
            (this.qteGraphics as QuickTimeEventButtonSequenceGraphic).GraphicsEndingState = QuickTimeEventsGraphics.GraphicsEndingEnum.Won;
            this.State = QTEState.Won;
        }

        public override void Lose()
        {
            base.Lose();
            this.isActive = false;
            (this.qteGraphics as QuickTimeEventButtonSequenceGraphic).GraphicsEndingState = QuickTimeEventsGraphics.GraphicsEndingEnum.Fail;
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
