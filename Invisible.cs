using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text.Json.Serialization;
using static CounterStrikeSharp.API.Core.Listeners;

namespace Invisible;

public partial class Invisible : BasePlugin, IPluginConfig<ConfigInvisible>
{
    public override string ModuleName => "Invisible";
    public override string ModuleAuthor => "DeadSwim";
    public override string ModuleDescription => "Invisible when not moveing";
    public override string ModuleVersion => "V. 1.0.6";



    public required ConfigInvisible Config { get; set; }

    private static readonly float?[] X = new float?[64];
    private static readonly float?[] Y = new float?[64];
    private static readonly float?[] Z = new float?[64];
    private static readonly int?[] Visible = new int?[64];
    private static readonly string?[] ActiveWeapon = new string?[64];
    public static readonly CounterStrikeSharp.API.Modules.Timers.Timer?[] timer = new CounterStrikeSharp.API.Modules.Timers.Timer?[64];
    public static readonly CounterStrikeSharp.API.Modules.Timers.Timer?[] timer_check = new CounterStrikeSharp.API.Modules.Timers.Timer?[64];
    public static readonly CounterStrikeSharp.API.Modules.Timers.Timer?[] timer_check2 = new CounterStrikeSharp.API.Modules.Timers.Timer?[64];
    public static readonly CounterStrikeSharp.API.Modules.Timers.Timer?[] timer_check3 = new CounterStrikeSharp.API.Modules.Timers.Timer?[64];


    public void OnConfigParsed(ConfigInvisible config)
    {
        Config = config;
    }
    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventPlayerSpawn>(EventPlayerSpawn);
        RegisterEventHandler<EventRoundEnd>(EventRoundEnd);
        RegisterEventHandler<EventPlayerDeath>(EventPlayerDeath);
        RegisterEventHandler<EventWeaponZoom>(EventWeaponZoom);
        RegisterEventHandler<EventWeaponReload>(EventWeaponReload);


        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnMapEnd>(MapEnd);
    }
    public override void Unload(bool hotReload)
    {
        KillAllTimers();
        DeregisterEventHandler<EventPlayerSpawn>(EventPlayerSpawn);
        DeregisterEventHandler<EventRoundEnd>(EventRoundEnd);
        DeregisterEventHandler<EventPlayerDeath>(EventPlayerDeath);
        DeregisterEventHandler<EventWeaponZoom>(EventWeaponZoom);
        DeregisterEventHandler<EventWeaponReload>(EventWeaponReload);

        RemoveListener<Listeners.OnTick>(OnTick);
        RemoveListener<Listeners.OnMapEnd>(MapEnd);
    }
    HookResult EventRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        KillAllTimers();
        return HookResult.Continue;
    }
    public void MapEnd()
    {
        KillAllTimers();
    }
    HookResult EventWeaponZoom(EventWeaponZoom @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null) return HookResult.Continue;

        ChangePlayerVisible(player, 255);
        TimerVisible(player);

        return HookResult.Continue;
    }
    HookResult EventWeaponReload(EventWeaponReload @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null) return HookResult.Continue;

        ChangePlayerVisible(player, 255);
        TimerVisible(player);

        return HookResult.Continue;
    }
    HookResult EventPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null) return HookResult.Continue;

        timer[player.Index]?.Kill();
        timer_check[player.Index]?.Kill();
        timer_check2[player.Index]?.Kill();
        timer_check3[player.Index]?.Kill();

        return HookResult.Continue;
    }
    HookResult EventPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null) return HookResult.Continue;
        Visible[player.Index] = 1;

        ChangePlayerVisible(player, 255);
        StartCheckingPlayer(player);

        return HookResult.Continue;
    }
    [ConsoleCommand("css_killtimers")]
    public void Kill(CCSPlayerController? player, CommandInfo info)
    {

        KillAllTimers();
        player.PrintToChat("Killed");
    }
    public void StartCheckingPlayer(CCSPlayerController? player)
    {
        if (player == null) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;
        var index = player.Index;

        timer_check3[index] = AddTimer(0.1f, () =>
        {
            if (pawn != null || player.PawnIsAlive)
            {
                X[index] = pawn?.AbsOrigin!.X;
                Y[index] = pawn?.AbsOrigin!.Y;
                Z[index] = pawn?.AbsOrigin!.Z;
                ActiveWeapon[index] = pawn!.WeaponServices?.ActiveWeapon.Value?.DesignerName;
            }
        }, TimerFlags.REPEAT);

        timer[index] = AddTimer(0.2f, () =>
        {
            if (pawn != null || player.PawnIsAlive)
            {
                if (X[index] != pawn?.AbsOrigin!.X) { ChangePlayerVisible(player, 255); TimerVisible(player); }
                if (Y[index] != pawn?.AbsOrigin!.Y) { ChangePlayerVisible(player, 255); TimerVisible(player); }
                if (Z[index] != pawn?.AbsOrigin!.Z) { ChangePlayerVisible(player, 255); TimerVisible(player); }
                if (ActiveWeapon[index] != pawn!.WeaponServices?.ActiveWeapon.Value?.DesignerName) { ChangePlayerVisible(player, 255); TimerVisible(player); }
            }
        }, TimerFlags.REPEAT);
    }
    public void TimerVisible(CCSPlayerController? player)
    {
        if (player == null) return;

        if (timer_check[player.Index] != null)
        {
            timer_check[player.Index]?.Kill();
        }

        timer_check[player.Index] = AddTimer(1f, () => { ChangePlayerVisible(player, 0); timer_check[player.Index]?.Kill(); });
    }
    public void OnTick()
    {
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            var ent = NativeAPI.GetEntityFromIndex(i);
            if (ent == 0)
                continue;

            var player = new CCSPlayerController(ent);
            if (player == null) return;
            if (!player.PawnIsAlive)
                return;

            
            var pawn = player.PlayerPawn.Value;
            if (pawn == null) return;
            var index = player.Index;

            var buttons = player.Buttons;
            var client = player.Index;
            var PP = player.PlayerPawn.Value;
            var flags = (PlayerFlags)PP!.Flags;

            if (Visible[index] == 1)
            {
                player.PrintToCenterHtml($"<center><font class='fontSize-l' color='red'></font>VISIBLE</center>\r\n▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬");
            }
            else
            {
                player.PrintToCenterHtml($"<center><font class='fontSize-l' color='green'></font>INVISIBLE</center>\r\n▥▥▥▥▥▥▥▥▥▥▥▥▥▥▥▥▥▥▥▥▥▥▥▥▥▥");
            }
            if ((flags & PlayerFlags.FL_ONGROUND) == 0)
            {
                ChangePlayerVisible(player, 255);
                TimerVisible(player);
            }
            if (PlayerButtons.Inspect == buttons || PlayerButtons.Bullrush == buttons)
            {
                ChangePlayerVisible(player, 255);
                TimerVisible(player);
            }
            if (PlayerButtons.Attack == buttons || PlayerButtons.Attack2 == buttons || PlayerButtons.Attack3 == buttons)
            {
                ChangePlayerVisible(player, 255);
                TimerVisible(player);
            }
        }
    }
}