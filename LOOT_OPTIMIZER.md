# Voyage Loot Optimizer

This build adds an economic border evaluator and layout-aware solver on top of PR #11.

## What the score means

The border score is a 0–100 **potential** score. It is not a guaranteed market-value estimate.
Each border starts with a reward-family score and receives synergy when the current chart pool can
exploit it. The final board score is weighted toward the best two or three borders because the most
successful board strategies concentrate compatible charts around a small number of strong edges.

Default tiers:

| Score | Color | Meaning |
|---:|---|---|
| 80–100 | Orange | Premium/Divine-tier potential |
| 55–79 | Blue | Rare and good |
| 30–54 | Green | Moderate or situational |
| 0–29 | White | Weak, unsupported, or no direct loot |

Recognized premium combinations include rare-monster charts with rare-orb borders, Operative or
strongbox charts with scarab borders, currency/chest combinations, Stacked Deck conversion with
minimum-magic plus no-equipment support, rarity with unique rewards, and chart-effect borders with
high-value adjacency charts.

## KEEP / REROLL model

The next reroll cost is:

`3,000 × 2^(rerolls already used)` sulphur.

At the supplied exchange rate of 130 sulphur per chaos, the first costs 23.08c, then 46.15c,
92.31c, 184.62c, and so on. The recommendation compares that guarded cost with:

`max(0, expected reroll score − current score) × chaos per loot point`.

Defaults are an expected reroll score of 50, 1 chaos per potential point, and a 10% safety margin.
These are editable under **Border loot and reroll economy** because published roll probabilities
and a stable chaos value for every possible board are not available. The plugin tracks border
changes while the window remains open and provides a reset button/manual offset.

## Quantity per connection

This border is no longer treated as a positive multiplier repeated for every connection. The two
stats are read from game memory and evaluated additively:

`effective quantity factor = 1 + fixed increased quantity − reduced quantity × connections`.

For the observed `120% increased / 50% reduced per connection` roll, the factor is:

| Connections | Effective factor |
|---:|---:|
| 1 | 1.70× |
| 2 | 1.20× |
| 3 | 0.70× |
| 4 | 0.20× |

The solver uses the complete rotated chart connection count and re-scores candidates with the full
model before displaying them.

## Layout policy

- Without a compatible premium border/chart combo, the solver prefers S, $, or compact templates
  to reduce branches and make the default lantern budget easier to use.
- Straight/candelabra templates are selected only for detected premium combinations, or an
  exceptional orange-tier board with a compatible premium implicit.
- Layout preference is a bonus, not a hard lock. This lets a materially stronger loot placement
  beat a visually perfect but weaker route. Its strength is configurable.

## Solver recovery

Having more charts expands the slow solver's search space. A short time limit can therefore expire
before it reaches the first complete grid even when a valid Voyage exists. This build recovers in
ordered stages instead of reporting a false "no solution":

1. Use the selected solver with all configured strategies and reservations.
2. If the slow solver returns no grid, run the exact fast topology solver on the same chart pool.
3. If the selected shapes are still infeasible, restore reserved charts while keeping strategic locks.
4. As a final feasibility fallback, restore reservations and relax strategy locks.

The optimizer window shows the active fallback, total/search/reserved chart counts, and the number
of locked placements. Straight/S/$ remains a score preference throughout every stage.

## Default weight rebalance

The default profile now follows the supplied modifier table more closely: voyage pack size 5/7,
quantity 8/10, rarity 7/9, increased rare/magic monsters 25, and Dead Man's Sulphur 15/20/25.
Friendly Jellyfish, Soul Eater, and flask quality are deliberately low without a specific combo.

## Community basis and limits

The model reflects reports that concentrated 2–3-edge setups and coherent rare-orb/strongbox
combinations outperform a board filled with unrelated generic rewards. It also avoids assuming that
player quantity/rarity scales strongbox contents. Sources used during implementation:

- https://www.reddit.com/r/pathofexile/comments/1v74hcz/ive_solved_voyages_theyre_good_but_maybe_theres/
- https://www.reddit.com/r/pathofexile/comments/1v8d1g3/my_feedback_on_voyages/
- https://www.reddit.com/r/pathofexile/comments/1v7b6ox/psa_check_your_voyage_modifiers_carefully/
- https://www.reddit.com/r/pathofexile/comments/1v6wo1j/loot_from_60_strongbox_voyage/
- https://www.pathofexile.com/forum/view-thread/3991665

Community observations are not controlled drop-rate tests. The score is intentionally configurable
and should be recalibrated if later patch notes or a reliable roll-frequency dataset become available.
