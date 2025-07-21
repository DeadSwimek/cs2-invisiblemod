using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Invisible
{
    public partial class Invisible
    {
        public void ChangePlayerVisible(CCSPlayerController? player, int count)
        {
            if (player == null) return;
            var playerPawn = player!.PlayerPawn.Value;
            if (playerPawn == null)
                return;

            if (count == 0) { Visible[player.Index] = 0; }
            if (count == 255) { Visible[player.Index] = 1; }
            playerPawn.Render = Color.FromArgb(count, 255, 255, 255);
            Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");


            var Weapons = playerPawn.WeaponServices?.MyWeapons;
            if (Weapons == null) return;
            foreach (var weapon in Weapons)
            {
                var w = weapon.Value;
                if (w == null) return;
                w.Render = Color.FromArgb(count, 255, 255, 255);
                w.ShadowStrength = 0f;
                Utilities.SetStateChanged(w, "CBaseModelEntity", "m_clrRender");
            }
        }
        public void KillAllTimers()
        {
            for (int i = 0; i < timer.Length; i++)
            {
                if (timer[i] != null)
                {
                    timer[i]?.Kill();
                    timer[i] = null;
                }
            }
            for (int i = 0; i < timer_check.Length; i++)
            {
                if (timer_check[i] != null)
                {
                    timer_check[i]?.Kill();
                    timer_check[i] = null;
                }
            }
            for (int i = 0; i < timer_check2.Length; i++)
            {
                if (timer_check2[i] != null)
                {
                    timer_check2[i]?.Kill();
                    timer_check2[i] = null;
                }
            }
            for (int i = 0; i < timer_check3.Length; i++)
            {
                if (timer_check3[i] != null)
                {
                    timer_check3[i]?.Kill();
                    timer_check3[i] = null;
                }
            }
        }
    }
}