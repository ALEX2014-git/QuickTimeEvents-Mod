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
    }

    public readonly Configurable<bool> DisableLizzardRNG;
    public readonly Configurable<bool> PunishFailure;

    private UIelement[] UIArrGeneral;


    public override void Initialize()
    {
        base.Initialize();
        var opTab = new OpTab(this, "Options");
        this.Tabs = new[]
        {
            opTab
        };

        UIArrGeneral = new UIelement[]
        {
            new OpLabel(10f, 550f, "Options", true),
            new OpLabel(10f, 520f, "Disable Lizzard bite RNG death"),
            new OpCheckBox(DisableLizzardRNG, 10f, 490f),
            new OpLabel(10f, 460f, "Punish QTE failure"),
            new OpCheckBox(PunishFailure, 10f, 430f){ description = PunishFailure.info.description }
        };
        opTab.AddItems(UIArrGeneral);
    }

    public override void Update()
    {
        //UIArrDebug[2].;
    }

}