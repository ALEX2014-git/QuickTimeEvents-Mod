using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static QTE.QTEKey;
using RWCustom;

namespace QTE
{
    public abstract class QuickTimeEventsGraphics : CosmeticSprite
    {
        protected QTEvent qte;
        protected Player player;
        protected int endingDelay = 10;
        public Color col;
        public Color lastCol;
        public float fade;
        public float lastFade;

        public enum GraphicsEndingEnum
        {
            Empty,
            None,
            Fail,
            Won,
            Ended
        }
        public GraphicsEndingEnum GraphicsEndingState;
        public QuickTimeEventsGraphics(QTEvent qe, Player player, Room rm)
        {
            this.room = rm;
            this.qte = qe;
            this.player = player;
            ResolveGraphicsPosition();
            this.lastPos = this.pos;
            this.fade = 1f;
            this.lastFade = this.fade;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (this.qte == null)
            {
                if (this.GraphicsEndingState != QuickTimeEventsGraphics.GraphicsEndingEnum.Ended)
                {
                    this.Destroy();
                    return;
                }
                if (this.GraphicsEndingState == QuickTimeEventsGraphics.GraphicsEndingEnum.Empty)
                {
                    this.GraphicsEndingState = QuickTimeEventsGraphics.GraphicsEndingEnum.None;
                }
                return;
            }
            CheckRemoveCondition();
            ResolveGraphicsPosition();
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public void CheckRemoveCondition()
        {
            if (this.player.room != this.room && !this.room.BeingViewed)
            {
                this.Destroy();
            }
        }

        public void ResolveGraphicsPosition()
        {
            this.pos = this.player.mainBodyChunk.pos + new Vector2(0f, 30f);
        }
    }

    public class QTEKey
    {
        public QTEvent.QTEAction action;
        public string spriteName;
        public float rotation;
        private Color myDefaultColor;
        public enum KeyState
        {
            None,
            Failed,
            Completed
        }

        public KeyState State { get; set; }

        public Color GetToColor
        {
            get
            {
                switch (this.State)
                {                
                    case KeyState.Failed:
                        return new Color(1f, 0f, 0f);
                    case KeyState.Completed:
                        return new Color(0f, 1f, 0f);
                    default:
                        return this.myDefaultColor;
                }
            }
        }

        private readonly Dictionary<QTEvent.QTEAction, string> ButtonToSpriteList = new Dictionary<QTEvent.QTEAction, string>
        {
            { QTEvent.QTEAction.MoveLeft, "Arrow" },
            { QTEvent.QTEAction.MoveRight, "Arrow" },
            { QTEvent.QTEAction.MoveUp, "Arrow" },
            { QTEvent.QTEAction.MoveDown, "Arrow" },
            { QTEvent.QTEAction.PickUp, "Shift" },
            { QTEvent.QTEAction.Throw, "X" },
            { QTEvent.QTEAction.Jump, "Z" }
        };

        public QTEKey(QTEvent.QTEAction action)
        {
            this.action = action;
            this.spriteName = ResolveSpriteName(this.action);
            this.rotation = ResolveSpriteRotation(this.action);
            this.myDefaultColor = new Color(1f, 1f, 0f);
        }

        private string ResolveSpriteName(QTEvent.QTEAction action)
        {
            if (ButtonToSpriteList.TryGetValue(action, out string value))
            {
                return value;
            }
            return "NoSpriteName";
        }

        private float ResolveSpriteRotation(QTEvent.QTEAction action)
        {
            float result = 0f;
            switch (action)
            {
                case QTEvent.QTEAction.MoveLeft:
                    result = -90f;
                    break;
                case QTEvent.QTEAction.MoveRight:
                    result = 90f;
                    break;
                case QTEvent.QTEAction.MoveDown:
                    result = -180f;
                    break;
                default:
                    break;
            }
            return result;
        }
    }

    public class QuickTimeEventButtonSequenceGraphic : QuickTimeEventsGraphics
    {

        public List<QTEKey> buttons = new List<QTEKey>();
        public int totalButtons;
        public QuickTimeEventButtonSequenceGraphic(QTEvent qe, Player player, Room rm) : base(qe, player, rm)
        {
            this.room = rm;
            this.qte = qe;
            this.col = new Color(1f, 1f, 0f);
            this.lastCol = this.col;
            Initiate();
        }

        public Color GetEndingColor
        {
            get
            {
                switch (this.GraphicsEndingState)
                {
                    case QuickTimeEventsGraphics.GraphicsEndingEnum.Fail:
                        return new Color(1f, 0f, 0f);
                    case QuickTimeEventsGraphics.GraphicsEndingEnum.Won:
                        return new Color(0f, 1f, 0f);
                    default:
                        return new Color(1f, 1f, 1f);
                }
            }
        }

        private void Initiate()
        {
            ConvertSequenceToSpriteData();
        }

        private void ConvertSequenceToSpriteData()
        {
            for (int i = 0; i < qte.requiredSequence.Count; i++)
            {
                this.buttons.Add(new QTEKey(qte.requiredSequence[i]));
            }
            this.totalButtons = this.buttons.Count;
            QTE.Logger.LogInfo($"totalButtons: {this.totalButtons}");
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            QTE.Logger.LogInfo("Started sprite initialization procedure:");
            sLeaser.sprites = new FSprite[totalButtons + 1];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["LightSource"];

            float buttonSpacing = 30f;

            float[] xOffsets = new float[totalButtons];
            for (int i = 0; i < totalButtons; i++)
            {
                xOffsets[i] = (i - (totalButtons - 1) / 2f) * buttonSpacing;
            }


            for (int i = 1; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i] = new FSprite("key" + buttons[i - 1].spriteName + "A", true);
                sLeaser.sprites[i].rotation = buttons[i - 1].rotation;
                sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["GateHologram"];
                sLeaser.sprites[i].scale = 1.2f;
                sLeaser.sprites[i].data = xOffsets[i - 1];
            }
            this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 objPos = Vector2.Lerp(this.lastPos, this.pos, timeStacker);
            float num = Mathf.Lerp(this.lastFade, this.fade, timeStacker);
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                float xOffset;
                if (sLeaser.sprites[i].data != null)
                {
                    xOffset = (float)sLeaser.sprites[i].data;
                }
                else
                {
                    xOffset = 0f;
                }

                sLeaser.sprites[i].x = objPos.x + xOffset - camPos.x;
                sLeaser.sprites[i].y = objPos.y - camPos.y;
            }

            if (this.endingDelay > 0)
            {
                for (int i = 1; i < totalButtons + 1; i++)
                {
                    sLeaser.sprites[i].color = buttons[i - 1].GetToColor;
                }
            }
            if (this.endingDelay <= 0)
            {
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].color = TransitionToEndingColor(sLeaser.sprites[i].color);
                    sLeaser.sprites[i].alpha = num;
                    bool shouldRemove = true;
                    if (sLeaser.sprites[i].alpha > 0)
                    {
                        shouldRemove = false;
                    }
                    if (shouldRemove)
                    {
                        this.GraphicsEndingState = GraphicsEndingEnum.Ended;
                    }
                }
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void Update(bool eu)
        {
            QTE.Logger.LogError("Started graphics update");
            base.Update(eu);
            this.lastCol = this.col;
            this.lastFade = this.fade;
            if (this.GraphicsEndingState != QuickTimeEventsGraphics.GraphicsEndingEnum.Empty &&
                this.GraphicsEndingState != QuickTimeEventsGraphics.GraphicsEndingEnum.Ended)
            {
                if (this.endingDelay < 0)
                {
                    this.col = Color.Lerp(this.col, this.GetEndingColor, 1f);
                    if (this.endingDelay < -40)
                    {
                        QTE.Logger.LogError($"Current fade value: {this.fade}");
                        this.fade = Custom.LerpAndTick(this.fade, 0f, 0.01f, 0.02f);
                    }
                }
                this.endingDelay--;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public Color TransitionToEndingColor(Color cl)
        {
            return Color.Lerp(cl, this.GetEndingColor, 0.05f);
        }
    }
}
