using ProjectM;
using Sanguis.Services;
using Stunlock.Core;
using Unity.Entities;
using VampireCommandFramework;

namespace Sanguis.Commands;

[CommandGroup(name: "sanguis")]
public static class SanguisCommands
{
    static readonly PrefabGUID tokenReward = new(Plugin.TokenReward);
    static readonly int tokenRewardRatio = Plugin.TokenRewardRatio;
    static readonly int tokensPerMinute = Plugin.TokensPerMinute;
    static readonly PrefabGUID dailyReward = new(Plugin.DailyReward);
    static readonly int dailyQuantity = Plugin.DailyQuantity;

    [Command(name: "redeem", shortHand: "r", adminOnly: false, usage: ".sanguis r", description: "Redeems Sanguis.")]
    public static void RedeemSanguisCommand(ChatCommandContext ctx)
    {
        if (!Plugin.TokenSystem)
        {
            ctx.Reply("<color=red>Sanguis</color> are currently disabled.");
            return;
        }

        ulong steamId = ctx.Event.User.PlatformId;

        if (Core.DataStructures.PlayerTokens.TryGetValue(steamId, out var tokenData))
        {
            if (tokenData.Tokens < tokenRewardRatio)
            {
                ctx.Reply($"You don't have enough <color=red>Sanguis</color> to redeem. (<color=#FFC0CB>{tokenRewardRatio}</color> minimum)");
                return;
            }

            int rewards = tokenData.Tokens / tokenRewardRatio;
            int cost = rewards * tokenRewardRatio;
            
            if (Core.ServerGameManager.TryAddInventoryItem(ctx.Event.SenderCharacterEntity, tokenReward, rewards))
            {
                tokenData = new(tokenData.Tokens - cost, tokenData.TimeData);
                Core.DataStructures.PlayerTokens[steamId] = tokenData;
                Core.DataStructures.SavePlayerTokens();
                //ctx.Reply($"You've received <color=#00FFFF>{Core.ExtractName(tokenReward.LookupName())}</color>x<color=white>{rewards}</color> for redeeming <color=#FFC0CB>{cost}</color> <color=red>Sanguis</color>!");
                ctx.Reply($"You've received <color=#00FFFF>{SanguisService.tokenReward}</color>x<color=white>{rewards}</color> for redeeming <color=#FFC0CB>{cost}</color> <color=red>Sanguis</color>!");

            }
            else
            {
                tokenData = new(tokenData.Tokens - cost, tokenData.TimeData);
                Core.DataStructures.PlayerTokens[steamId] = tokenData;
                Core.DataStructures.SavePlayerTokens();
                InventoryUtilitiesServer.CreateDropItem(Core.EntityManager, ctx.Event.SenderCharacterEntity, tokenReward, rewards, new Entity());
                //ctx.Reply($"You've received <color=#00FFFF>{Core.ExtractName(tokenReward.LookupName())}</color>x<color=white>{rewards}</color> for redeeming <color=#FFC0CB>{cost}</color> <color=red>Sanguis</color>! It dropped on the ground because your inventory was full.");
                ctx.Reply($"You've received <color=#00FFFF>{SanguisService.tokenReward}</color>x<color=white>{rewards}</color> for redeeming <color=#FFC0CB>{cost}</color> <color=red>Sanguis</color>! It dropped on the ground because your inventory was full.");
            }
        }
    }

    [Command(name: "get", shortHand: "g", adminOnly: false, usage: ".sanguis g", description: "Shows earned Sanguis, also updates them.")]
    public static void GetSanguisCommand(ChatCommandContext ctx)
    {

        if (!Plugin.TokenSystem)
        {
            ctx.Reply("<color=red>Sanguis</color> are currently disabled.");
            return;
        }

        ulong steamId = ctx.Event.User.PlatformId;

        if (Core.DataStructures.PlayerTokens.TryGetValue(steamId, out var tokenData))
        {
            TimeSpan timeOnline = DateTime.Now - tokenData.TimeData.Start;
            tokenData = new(tokenData.Tokens + timeOnline.Minutes * tokensPerMinute, new(DateTime.Now, tokenData.TimeData.DailyLogin));
            Core.DataStructures.PlayerTokens[steamId] = tokenData;
            Core.DataStructures.SavePlayerTokens();
            ctx.Reply($"You have <color=#FFC0CB>{tokenData.Tokens}</color> <color=red>Sanguis</color>.");
        }

    }
    
    [Command(name: "daily", shortHand: "d", adminOnly: false, usage: ".sanguis d", description: "Time left until eligible for daily login. Awards daily if eligible.")]
    public static void GetDailyCommand(ChatCommandContext ctx)
    {
        if (!Plugin.DailyLogin)
        {
            ctx.Reply("<color=#CBC3E3>Daily</color> reward is currently disabled.");
            return;
        }

        ulong steamId = ctx.Event.User.PlatformId;

        if (Core.DataStructures.PlayerTokens.TryGetValue(steamId, out var tokenData))
        {
            DateTime lastDailyLogin = tokenData.TimeData.DailyLogin;
            DateTime nextEligibleLogin = lastDailyLogin.AddDays(1); // assuming daily login resets every 24 hours
            DateTime currentTime = DateTime.Now;

            if (currentTime >= nextEligibleLogin)
            {
                if (Core.ServerGameManager.TryAddInventoryItem(ctx.Event.SenderCharacterEntity, dailyReward, dailyQuantity))
                {
                    string message = $"You've received <color=#00FFFF>{SanguisService.dailyReward}</color>x<color=white>{dailyQuantity}</color> for logging in today!";

                    ctx.Reply(message);
                }
                else
                {
                    InventoryUtilitiesServer.CreateDropItem(Core.EntityManager, ctx.Event.SenderCharacterEntity, dailyReward, dailyQuantity, new Entity());
                    string message = $"You've received <color=#00FFFF>{SanguisService.dailyReward}</color>x<color=white>{dailyQuantity}</color> for logging in today! It dropped on the ground because your inventory was full.";

                    ctx.Reply(message);
                }
                tokenData = new(tokenData.Tokens, new(tokenData.TimeData.Start, DateTime.Now));
                Core.DataStructures.PlayerTokens[steamId] = tokenData;
                Core.DataStructures.SavePlayerTokens();
            }
            else
            {
                TimeSpan untilNextDaily = nextEligibleLogin - currentTime;
                string timeLeft = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                                untilNextDaily.Hours,
                                                untilNextDaily.Minutes,
                                                untilNextDaily.Seconds);
                ctx.Reply($"Time until daily reward: <color=yellow>{timeLeft}</color>.");
            }
        }    
    }  
}