using System.Collections.Generic;
using BepInEx.Logging;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace QTE;

public class PluginOptions : OptionInterface
{
    private readonly ManualLogSource Logger;

    public PluginOptions(QTE modInstance, ManualLogSource loggerSource)
    {
        Logger = loggerSource;
        DisableLizzardRNG = this.config.Bind<bool>("DisableLizzardRNG", true);
        PunishFailure = this.config.Bind<bool>("PunishFailure", false, new ConfigurableInfo("If enabled, kills slugcat of the player who failed QTE. Disabled by default."));
        TimeSlowMode = this.config.Bind<string>("TimeSlowMode", "Stop");
    }

    public readonly Configurable<bool> DisableLizzardRNG;
    public readonly Configurable<bool> PunishFailure;
    public readonly Configurable<string> TimeSlowMode;
    private UIelement[] UIArrGeneral;
    private static readonly string[] TimeSlowModeArr = { "Stop", "Slow" };
    OpComboBox timeSlowComboBox;


    public override void Initialize()
    {
        base.Initialize();
        var opTab = new OpTab(this, "Options");
        this.Tabs = new[]
        {
            opTab
        };
        timeSlowComboBox = new OpComboBox(TimeSlowMode, new Vector2(120f, 400f), 80f, TimeSlowModeArr);

        UIArrGeneral = new UIelement[]
        {
            new OpLabel(10f, 550f, "Options", true),
            new OpLabel(10f, 520f, "Disable Lizzard bite RNG death"),
            new OpCheckBox(DisableLizzardRNG, 10f, 490f),
            new OpLabel(10f, 460f, "Punish QTE failure"),
            new OpCheckBox(PunishFailure, 10f, 430f){ description = PunishFailure.info.description },
            new OpLabel(10f, 400f, "Select time behavior during QTE"),
            timeSlowComboBox
        };
        opTab.AddItems(UIArrGeneral);
    }

    public override void Update()
    {
        
    }

}