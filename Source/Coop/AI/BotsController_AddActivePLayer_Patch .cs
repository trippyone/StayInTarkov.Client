//using Comfort.Common;
//using EFT;
//using EFT.Counters;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;

//namespace StayInTarkov.Coop.AI
//{
//    internal class BotsController_AddActivePLayer_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(BotsController);

//        public override string MethodName => "AddActivePLayer";

//        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

//        [PatchPrefix]
//        public static bool PrePatch(BotsController __instance, EFT.Player player, BotSpawner ___botSpawner)
//        {
//            var playerViewer = Singleton<GameWorld>.Instance.allObservedPlayersByID.Where(x => x.Key == player.ProfileId).FirstOrDefault().Value;
//            if (playerViewer != null)
//            {
//                var method_5 = ___botSpawner.GetType().GetMethod("method_5", BindingFlags.NonPublic | BindingFlags.Instance);
//                if (method_5 != null)
//                {
//                    method_5.Invoke(___botSpawner, [playerViewer]);
//                }
//            }

//            return true;
//        }

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            return;
//        }
//    }
//}
