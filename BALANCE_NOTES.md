# Loot Optimized Voyage profile

This build includes a `Loot Optimized` profile based on the supplied Allflame Voyage sheet.
It is installed without overwriting existing user profiles.

## Priority model

- Premium adjacent rewards: Operative, Arcanist and Diviner Strongboxes, Messages in Bottles,
  Giant Starfish, Golden Lanterns and additional Strongboxes.
- Premium voyage-wide rewards: no equipment drops, quantity, rare/magic monster density,
  possessed rares and pack size.
- Strategy borders: Divine/Annulment/Ancient rare rewards, Scarabs, Currency, Treasure Anchors,
  Golden/Infinite Lanterns and chart effect.
- Low weights: ordinary monster packs, barrels, equipment-to-gold, common unique conversions,
  flask quality, Soul Eater, Friendly Jellyfish and other sheet-marked low-value rewards.

Operative Strongboxes use `Strongboxes,Scarabs`; Arcanist Strongboxes use
`Strongboxes,Currency`. This lets Scarab and Currency borders amplify the correct chart family.

All Additional/Diviner/Arcanist/Operative Strongbox sources also carry `RareMonsters` for scoring.
This represents rerolling Voyage strongboxes to `Guarded by 3 Rare Monsters`. With a rare-currency
border, Sea Pillars is locked on the rewarded tile, the highest-Value1 strongbox charts are locked
orthogonally around it, and global increased-rare charts fill remaining free strategy slots.
Treasure Anchors and Golden/Infinite Lanterns have intrinsic flat utility instead of pretending
to be generic reward multipliers.

## Composite Quantity per connection

The old solver used `1 + (multiplier - 1) * connections`, which incorrectly made the supplied
Quantity border stronger with more connections.

The corrected model is:

`effective multiplier = max(0, base multiplier + connection change * chart connections)`

Configured tiers:

| Tier | Base | Per connection | 1 conn | 2 conn | 3 conn | 4 conn |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | 2.20 | -0.50 | 1.70 | 1.20 | 0.70 | 0.20 |
| 2 | 2.80 | -0.75 | 2.05 | 1.30 | 0.55 | 0.00 |
| 3 | 3.40 | -1.00 | 2.40 | 1.40 | 0.40 | 0.00 |

Because these destination multipliers couple neighboring chart choices, a board containing any
per-connection border automatically uses the exact solver. The fast solver is retained for boards
where its assignment model is exact enough.

## Score display

- `Loot score`: chart weights, voyage-wide weights, border multipliers and intrinsic border value.
- `Layout`: lantern-efficiency quality for S/$, straight and compact/closed layouts.
- `Optimized score`: loot score plus the configured layout preference bonus.

The weights are utility values for comparing arrangements, not a prediction of exact currency
drops. Change the profile rather than the source code when personal market prices change.
