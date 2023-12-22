using EFT.InventoryLogic;
using StayInTarkov.Coop.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StayInTarkov.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_GetMalfunctionState : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "GetMalfunctionState";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPrefix]
        public static bool Prefix(ref Weapon.EMalfunctionState __result, EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            var botPlayer = ____player as CoopBot;
            if (botPlayer != null)
            {
                __result = botPlayer.CurrentMalfunctionState;
                return false;
            }

            var player = ____player as CoopPlayer;
            if (player == null || player.IsYourPlayer)
                return true;

            __result = player.CurrentMalfunctionState;
            return false;
            
        }

        [PatchPostfix]
        public static void Postfix(ref Weapon.EMalfunctionState __result, EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            var player = ____player as CoopPlayer;
            if (player == null || !player.IsYourPlayer)
                return;

            player.WeaponPacket.HasMalfunctionStatePacket = true;
            player.WeaponPacket.MalfunctionState = __result;
            player.WeaponPacket.ToggleSend();
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {

        }
    }
}
