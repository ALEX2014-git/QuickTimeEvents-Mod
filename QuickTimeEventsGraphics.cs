using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using static QTE.QTEvent;

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
            if (this.qte == null || qte?.State == QTEState.Destroy)
            {
                if (this.qte != null)
                {
                    this.qte = null;
                }
                if (this.GraphicsEndingState == QuickTimeEventsGraphics.GraphicsEndingEnum.Ended)
                {
                    this.Destroy();
                    return;
                }
                if (this.GraphicsEndingState == QuickTimeEventsGraphics.GraphicsEndingEnum.Empty)
                {
                    this.GraphicsEndingState = QuickTimeEventsGraphics.GraphicsEndingEnum.None;
                }
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

        public Color TransitionToEndingColor(Color cl)
        {
            return Color.Lerp(cl, this.GetEndingColor, 0.05f);
        }

        public class GraphicsPart
        {
            public int totalSprites;
            public int firstSprite;
            public Vector3 rotation3;
            public Vector3 lastRotation3;
            public Vector2 offset3;
            public Vector2 lastOffset3;
            public float transform;
            public float lastTransform;
            public float partFade;
            public float lastPartFade;
            public bool visible = true;
            public float fadeExponent = 1f;
            public bool allSpritesHologramShader = true;
            public Color color;
            public Color lastColor;
            public List<GraphicsPart.Line> lines;

            public void AddClosedPolygon(List<Vector2> vL)
            {
                for (int i = 1; i < vL.Count; i++)
                {
                    this.AddLine(vL[i - 1], vL[i]);
                }
                this.AddLine(vL[vL.Count - 1], vL[0]);
            }

            public void AddClosed3DPolygon(List<Vector2> vL, float depth)
            {
                for (int i = 1; i < vL.Count; i++)
                {
                    this.Add3DLine(vL[i - 1], vL[i], depth);
                }
                this.Add3DLine(vL[vL.Count - 1], vL[0], depth);
            }

            public void Add3DLine(Vector2 A, Vector2 B, float depth)
            {
                this.AddLine(new Vector3(A.x, A.y, -depth), new Vector3(B.x, B.y, -depth));
                this.AddLine(new Vector3(A.x, A.y, depth), new Vector3(B.x, B.y, depth));
                this.AddLine(new Vector3(A.x, A.y, -depth), new Vector3(A.x, A.y, depth));
            }

            public void AddLine(Vector2 A, Vector2 B)
            {
                this.AddLine(new Vector3(A.x, A.y, 0f), new Vector3(B.x, B.y, 0f));
            }

            public void AddLine(Vector3 A, Vector3 B)
            {
                this.lines.Add(new GraphicsPart.Line(A, B, this.firstSprite + this.totalSprites));
                this.totalSprites++;
            }

            #region QTEKeys
            public class QTEKey : GraphicsPart
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

                public class KeyboardKey : QTEKey
                {
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

                    public KeyboardKey(QTEvent.QTEAction action)
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

                public class GamepadKey : QTEKey
                {

                }
            }
            #endregion

            public class Line : GraphicsPart
            {
                public Vector3 A;
                public Vector3 B;
                public Vector3 A2;
                public Vector3 B2;
                public int sprite;

                public Line(Vector3 A, Vector3 B, int sprite)
                {
                    this.A = A;
                    this.B = B;
                    this.A2 = A;
                    this.B2 = B;
                    this.sprite = sprite;
                }
            }
        }
    }
    public class QuickTimeEventButtonSequenceGraphic : QuickTimeEventsGraphics
    {

        public List<GraphicsPart.QTEKey> buttons = new List<GraphicsPart.QTEKey>();
        public int totalButtons;
        private int firstButtonSpriteIndex;
        public QuickTimeEventButtonSequenceGraphic(QTEvent qe, Player player, Room rm) : base(qe, player, rm)
        {
            this.room = rm;
            this.qte = qe;
            this.col = new Color(1f, 1f, 0f);
            this.lastCol = this.col;
            Initiate();
        }

        private void Initiate()
        {
            ConvertSequenceToSpriteData();
        }

        private void ConvertSequenceToSpriteData()
        {
            for (int i = 0; i < qte.requiredSequence.Count; i++)
            {
                this.buttons.Add(new QuickTimeEventsGraphics.GraphicsPart.QTEKey.KeyboardKey(qte.requiredSequence[i]));
            }
            this.totalButtons = this.buttons.Count;
            QTE.Logger.LogInfo($"totalButtons: {this.totalButtons}");
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            QTE.Logger.LogInfo("Started sprite initialization procedure:");
            sLeaser.sprites = new FSprite[totalButtons + 2];
            sLeaser.sprites[0] = new FSprite("Futile_White", true);
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["LightSource"];

            sLeaser.sprites[1] = new FSprite("pixel");


            firstButtonSpriteIndex = 2;
            float buttonSpacing = 30f;
            
            float[] xOffsets = new float[totalButtons];
            for (int i = 0; i < totalButtons; i++)
            {
                xOffsets[i] = (i - (totalButtons - 1) / 2f) * buttonSpacing;
            }

            for (int i = firstButtonSpriteIndex; i < totalButtons + firstButtonSpriteIndex; i++)
            {
                sLeaser.sprites[i] = new FSprite("key" + buttons[i - firstButtonSpriteIndex].spriteName + "A", true);
                sLeaser.sprites[i].rotation = buttons[i - firstButtonSpriteIndex].rotation;
                sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["GateHologram"];
                sLeaser.sprites[i].scale = 1.2f;
                sLeaser.sprites[i].data = xOffsets[i - firstButtonSpriteIndex];
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
            sLeaser.sprites[0].scale = Mathf.Lerp(0.5f, 2f, timeStacker);

            if (this.endingDelay > 0)
            {
                for (int i = firstButtonSpriteIndex; i < totalButtons + firstButtonSpriteIndex; i++)
                {
                    sLeaser.sprites[i].color = buttons[i - firstButtonSpriteIndex].GetToColor;
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
    }
}
