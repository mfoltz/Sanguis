using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using ProjectM.Physics;
using System.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Sanguis.Services;
internal class SanguisService
{
    static readonly ComponentType[] UserComponent =
    [
        ComponentType.ReadOnly(Il2CppType.Of<User>()),
    ];

    static readonly int intervalMinutes = Plugin.UpdateInterval;
    static readonly int tokensPerMinute = Plugin.TokensPerMinute;
    static readonly bool TokenSystem = Plugin.TokenSystem;

    public static string tokenReward;
    public static string dailyReward;

    static EntityQuery UserQuery;

    readonly IgnorePhysicsDebugSystem tokenMonoBehaviour;

    public SanguisService()
    {
        UserQuery = Core.EntityManager.CreateEntityQuery(UserComponent);

        tokenReward = Core.Localization.GetPrefabName(new(Plugin.TokenReward));
        dailyReward = Core.Localization.GetPrefabName(new(Plugin.DailyReward));
        
        tokenMonoBehaviour = (new GameObject("SanguisService")).AddComponent<IgnorePhysicsDebugSystem>();
        if (TokenSystem) tokenMonoBehaviour.StartCoroutine(UpdateLoop().WrapToIl2Cpp());
    }

    // Iterate through each entity in the query
    static IEnumerator UpdateLoop()
    {
        WaitForSeconds waitForSeconds = new(intervalMinutes * 60); // Convert minutes to seconds for update loop
        while (true)
        {
            NativeArray<Entity> userEntities = UserQuery.ToEntityArray(Allocator.TempJob);
            DateTime now = DateTime.Now;
            try
            {
                Dictionary<ulong, (int Tokens, (DateTime Start, DateTime End) TimeData)> updatedTokens = [];
                foreach (Entity userEntity in userEntities)
                {
                    User user = userEntity.Read<User>();
                    if (!user.IsConnected) continue;
                    ulong steamId = user.PlatformId;
                    if (Core.DataStructures.PlayerTokens.TryGetValue(steamId, out var tokenData))
                    {
                        TimeSpan timeOnline = now - tokenData.TimeData.Start;
                        int newTokens = tokenData.Tokens + timeOnline.Minutes * tokensPerMinute;
                        updatedTokens[steamId] = (newTokens, (now, tokenData.TimeData.DailyLogin));
                    }
                    yield return null;
                }
                foreach (var tokenData in updatedTokens)
                {
                    Core.DataStructures.PlayerTokens[tokenData.Key] = tokenData.Value;
                }
                Core.DataStructures.SavePlayerTokens();
            }
            finally
            {
                userEntities.Dispose();
            }
            ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, $"<color=red>Sanguis</color> have been updated, don't forget to redeem them! (.sanguis r)");
            yield return waitForSeconds;
        }
    }
}