using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using RWCustom;
using BepInEx;
using Debug = UnityEngine.Debug;
using System.Data.SqlClient;
using BepInEx.Logging;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using CoralBrain;
using Expedition;
using HUD;
using JollyCoop;
using JollyCoop.JollyMenu;
using MoreSlugcats;
using Noise;
using static UpdatableAndDeletable;
using Menu;
using Rewired;
using UnityEngine.SocialPlatforms;
using UnityEngine.Rendering;
using System.Runtime.Serialization;
using Steamworks;
using IL.Menu.Remix.MixedUI;
using Menu.Remix.MixedUI;
using IL;
using On.Menu.Remix.MixedUI;
using System.Runtime.Remoting.Messaging;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using System.Runtime.Remoting.Lifetime;
using System.Xml;
using System.Reflection;
using MonoMod.RuntimeDetour.HookGen;
using System.Reflection.Emit;
using MonoMod;
using Unity.Collections.LowLevel.Unsafe;
using On;
using HarmonyLib.Tools;
using System.Runtime.CompilerServices;
using System.IO;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace QTE;

[BepInPlugin("ALEX2014.QTE", "Quick Time Events", "1.0.0")]
public partial class QTE : BaseUnityPlugin
{
    internal PluginOptions options;
    private bool IsInit;
 

    public static ManualLogSource Logger { get; private set; }

    private static QTE _instance;
    public static QTE Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new Exception("Instance of CustomRoboLock is not created yet!");
            }
            return _instance;
        }
    }
    public QTE()
    {
        try
        {
            _instance = this;
            options = new PluginOptions(this, Logger);
            Logger.LogInfo($"Set up options from CTOR. Is NULL? {options == null}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }

    private void OnEnable()
    {
        QTE.Logger = base.Logger;
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
    }

    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (IsInit) return;
            On.RainWorldGame.ShutDownProcess += RainWorldGameOnShutDownProcess;
            On.GameSession.ctor += GameSessionOnctor;
            On.Player.Grabbed += Player_Grabbed;
            On.Player.Update += Player_Update;
            On.Player.DeathByBiteMultiplier += Player_DeathByBiteMultiplier;
            On.Player.ThrowToGetFree += Player_ThrowToGetFree;
            On.Player.DangerGraspPickup += Player_DangerGraspPickup;
            On.Room.Update += Room_Update;
            IL.PlayerGraphics.Update += PlayerGraphics_Update;
            
            MachineConnector.SetRegisteredOI("ALEX2014.QTE", options);


            IsInit = true;  
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }

    private void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);
    }


/*
    private static void LoadModResources()
    {
        try
        {
            Futile.atlasManager.LoadAtlas("Atlases/robolockGlyph");
            Futile.atlasManager.LoadAtlas("Atlases/robolockSigns");
            Logger.LogMessage("Loaded atlases");
        }
        catch (Exception)
        {
            Logger.LogError("Couldn't load sprites!");
            throw;
        }
    }
*/

    private void RainWorldGameOnShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
    {
        orig(self);
        ClearMemory();
    }
    private void GameSessionOnctor(On.GameSession.orig_ctor orig, GameSession self, RainWorldGame game)
    {
        orig(self, game);
        ClearMemory();
    }

    #region Helper Methods

    private void ClearMemory()
    {
        //If you have any collections (lists, dictionaries, etc.)
        //Clear them here to prevent a memory leak
        //YourList.Clear();
    }

    #endregion
}
