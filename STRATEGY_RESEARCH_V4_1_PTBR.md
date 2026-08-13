# Estratégias de Voyage v4.1 — critérios e evidências

Pesquisa revisada em 12/08/2026. Relatos de loot são amostras da comunidade, não garantias de
retorno; por isso o planner usa os padrões repetidos entre relatos e não os maiores números
publicados.

## Regras adotadas

| Plano | Gatilho mínimo | O que fica reservado |
|---|---|---|
| Rare currency | border Divine/Annulment/Ancient/Exalted + Sea Pillars + 3 Strongboxes adjacentes + 5 rares globais | Sea Pillars, até 9 Strongboxes genéricas, Operative/Diviner em pools próprios, até 9 rares globais e adjacentes |
| Operative | 9 charts Operative | até 9 Operative |
| Diviner | 9 charts Diviner | até 9 Diviner |
| Sulphur | border de Sulphur + 9 charts com pelo menos 25% | até 9 Sulphur 25% |
| Message | 8 providers + 1 alvo | até 8 Message in a Bottle |
| Brine King | border de rare/pack size + 3 Adjacent Rare/Starfish + 5 rares globais | Brine, Adjacent Rare/Starfish e rares globais |
| Fast Voyage | nenhum pacote completo | usa Barrels, Imprisoned, Soul Eater, Possessed e Tormented Spirits como filler |

## Por que os limites são rígidos

- Relatos repetidos de Divine Voyage usam concentração: Sea Pillars no tile premiado, cerca de
  três charts de Strongbox e cinco implicits globais de rare monsters.
- Strongbox spam sem uma recompensa/border que converta a densidade apresentou resultado fraco;
  portanto uma Strongbox isolada não abre mais uma estratégia.
- Para Strongboxes no tile premiado, a comunidade usa `Guarded by 3 Rare Monsters` ou
  `Stream of Monsters`; o segundo aparece como preferência de alguns jogadores. Isso é uma dica
  operacional exibida pelo plugin, não um peso oculto do solver.
- Sulphur aproveita tanto o implicit do chart quanto a Voyage; guardar nove charts de 25% para uma
  border compatível é mais coerente do que espalhá-los por Voyages comuns.
- Conectividade continua sendo uma restrição: se os charts não fecharem o formato selecionado, o
  planner não restaura estoques premium para fabricar uma solução qualquer.

## Formato

- Fast Voyage respeita literalmente as famílias marcadas; com somente S/$, não aceita compacto ou
  linhas retas.
- A exceção premium não significa `All`. Ela seleciona somente linhas retas para rare currency e
  somente compacto para Message/Brine, pois esses planos precisam de uma topologia de suporte.

## Fontes comunitárias

- https://www.reddit.com/r/pathofexile/comments/1vgavh3/divine_voyage_w_friends/
- https://www.reddit.com/r/pathofexile/comments/1v9qoh7/optimizing_divine_voyage/
- https://www.reddit.com/r/pathofexile/comments/1v74hcz/ive_solved_voyages_theyre_good_but_maybe_theres/
- https://www.reddit.com/r/pathofexile/comments/1v6wo1j/loot_from_60_strongbox_voyage/
- https://www.reddit.com/r/PathOfExileSSF/comments/1vdj9ii/best_dead_mans_sulphur_farm_charts_or_voyages/
- https://www.reddit.com/r/PathOfExileBuilds/comments/1v90lxh/voyage_strategies_discussion/
- https://www.reddit.com/r/PathOfExileBuilds/comments/1vi3le5/how_are_you_guys_farming_chartsvoyages/
