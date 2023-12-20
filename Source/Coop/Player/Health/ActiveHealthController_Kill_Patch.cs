﻿using BepInEx.Logging;
using EFT;
using EFT.HealthSystem;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StayInTarkov.Coop.Player.Health
{
    internal class ActiveHealthController_Kill_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(ActiveHealthController);

        public override string MethodName => "Kill";

        public static Dictionary<string, bool> CallLocally = new();

        private MethodInfo WeaponSoundPlayerRelease;
        private MethodInfo WeaponSoundPlayerStopSoundCoroutine;

        public ActiveHealthController_Kill_Patch()
        {
            WeaponSoundPlayerRelease = ReflectionHelpers.GetMethodForType(typeof(WeaponSoundPlayer), "Release");
            WeaponSoundPlayerStopSoundCoroutine = ReflectionHelpers.GetMethodForType(typeof(WeaponSoundPlayer), "StopSoundCoroutine");
        }

        private ManualLogSource GetLogger()
        {
            return GetLogger(typeof(ActiveHealthController_Kill_Patch));
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        //[PatchPrefix]
        //public static bool PrePatch(
        //    ActiveHealthController __instance
        //    )
        //{
        //    var player = __instance.Player;

        //    var result = false;
        //    if (CallLocally.TryGetValue(player.ProfileId, out var expecting) && expecting)
        //        result = true;
        //    return result;
        //}

        [PatchPostfix]
        public static void PatchPostfix(ActiveHealthController __instance, EDamageType damageType)
        {
            var botPlayer = __instance.Player as CoopBot;
            if (botPlayer != null)
            {
                EFT.UI.ConsoleScreen.Log("Kill Patch: Botplayer");
                botPlayer.HealthPacket.HasObservedDeathPacket = true;
                botPlayer.HealthPacket.ObservedDeathPacket = new()
                {
                    DamageType = damageType
                };
                botPlayer.HealthPacket.ToggleSend();
                return;
            }

            var player = __instance.Player as CoopPlayer;
            if (player != null && player.IsYourPlayer)
            {
                EFT.UI.ConsoleScreen.Log("Kill Patch player");
                player.HealthPacket.HasObservedDeathPacket = true;
                player.HealthPacket.ObservedDeathPacket = new()
                {
                    DamageType = damageType
                };
                player.HealthPacket.ToggleSend();
                return;
            }

            var observedPlayer = __instance.Player as ObservedCoopPlayer;
            if (observedPlayer != null)
            {
                EFT.UI.ConsoleScreen.Log("Kill Patch observedplayer");
                observedPlayer.HealthPacket.HasObservedDeathPacket = true;
                observedPlayer.HealthPacket.ObservedDeathPacket = new()
                {
                    DamageType = damageType
                };
                observedPlayer.HealthPacket.ToggleSend();
                return;
            }

            //Logger.LogDebug("RestoreBodyPartPatch:PatchPostfix");

            //var player = __instance.Player;

            //if (CallLocally.TryGetValue(player.ProfileId, out var expecting) && expecting)
            //{
            //    CallLocally.Remove(player.ProfileId);
            //    return;
            //}

            //KillPacket killPacket = new(player.ProfileId);
            //killPacket.DamageType = damageType;
            //var json = killPacket.Serialize();
            //AkiBackendCommunication.Instance.SendDataToPool(json);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("data"))
                return;

            KillPacket killPacket = new(player.ProfileId);
            killPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(GetType(), player, killPacket))
                return;

            if (CallLocally.ContainsKey(player.ProfileId))
                return;

            GetLogger().LogDebug($"Replicated Kill {player.ProfileId}");

            CallLocally.Add(player.ProfileId, true);
            player.ActiveHealthController.Kill(killPacket.DamageType);
            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                firearmCont.SetTriggerPressed(false);
                WeaponSoundPlayerRelease.Invoke(firearmCont.WeaponSoundPlayer, new object[1] { 0f });
                WeaponSoundPlayerStopSoundCoroutine.Invoke(firearmCont.WeaponSoundPlayer, new object[0]);
            }
        }

        class KillPacket : BasePlayerPacket
        {
            public EDamageType DamageType { get; set; }

            public KillPacket(string profileId) : base(profileId, "Kill")
            {
            }
        }
    }
}
