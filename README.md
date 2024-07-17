## Table of Contents

- [Features](#features)
- [Commands](#commands)
- [Configuration](#configuration)

## Features

- **Sanguis:** Reward players with Sanguis for being online. Can be redeemed in-game for a configurable item, Sanguis per minute online and Sanguis per item reward can be configured as well as the update interval.
- **Daily Login:** Reward players with a congfigurable item/quantity for logging in once per day.

## Commands

### Sanguis Commands
- `.sanguis redeem`
  - Redeem earned Sanguis.
  - Shortcut: *.sanguis r*
- `.sanguis get`
  - Display total Sanguis.
  - Shortcut: *.sanguis g*
- `.sanguis daily`
  - Display time until daily login bonus or redeem it if available.
  - Shortcut: *.sanguis d*
 
## Configuration

### Reward Systems
- **Sanguis**: `Sanguis` (bool, default: false)  
  Enable or disable Sanguis.
- **Daily Logins**: `DailyLogin` (bool, default: false)  
  Enable or disable daily logins. Sanguis must be enabled as well.
- **Daily Item Reward**: `DailyReward` (int, default: -257494203)  
  Item prefab for daily reward.
- **Daily Item Quantity**: `DailyQuantity` (int, default: 50)
  Item quantity for daily reward.
- **Sanguis Item Reward**: `SanguisItemReward` (int, default: -257494203)  
  Item prefab for redeeming Sanguis.
- **Sanguis Reward Factor**: `SanguisRewardFactor` (int, default: 50)  
  Sanguis required per item reward.
- **Sanguis Per Minute**: `SanguisPerMinute` (int, default: 5)  
  Factor by which rates are increased in expertise/legacy per increment of prestige in leveling.
- **Sanguis Update Interval**: `SanguisUpdateInterval` (int, default: 30)  
  Interval in minutes to update player Sanguis.

