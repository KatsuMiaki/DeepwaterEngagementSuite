# DeepwaterEngagementSuite — Unified Focus Planner v4

Esta versão mantém a leitura das duas abas, o auto-placement com validação de rotação, o scorer por
tags, os modifiers `per connection` e os fallbacks de conectividade.

As regras atuais estão documentadas em:

- `VOYAGE_STRATEGIES_V4_PTBR.md`: foco único, S/$, compacto, linhas retas, reservas e segurança;
- `RARE_CURRENCY_STRONGBOX_PTBR.md`: Sea Pillars + Strongboxes para moeda por rare monster;
- `BUBBLE_PLANNER_LIVE_VOYAGE_PTBR.md`: Bubble Planner fixo somente nos charts normais;
- `SULPHUR_VISUAL_CLUSTERS_PTBR.md`: visual compacto dos cristais.

## Princípios atuais

- Uma Voyage usa um foco por padrão.
- Strongboxes e rare monsters globais são guardados para Divine/Exalted/Annulment/Ancient.
- Brine King usa rare monsters globais e Adjacent Rare/Starfish, nunca Strongboxes.
- Sulphur global aguarda border de Sulphur.
- Messages in a Bottle aguardam quantidade suficiente e são gastos juntos.
- Ground loot isolado não cria estratégia; exige o combo completo configurado.
- Pantheon com Brine King ou Possessed Rares é uma combinação inválida até nos fallbacks.
- Sem foco forte, o solver prefere S/$ para completar a Voyage rapidamente.

## Origem técnica preservada

- Base anexada: scorer econômico, tiers, perfis e recuperação do solver.
- `purplecofe`: fundação do Voyage Planner e pesos configuráveis.
- `deafwave`: duas abas, validação de rotação, retry/foco da janela e atraso entre cliques.
- Revisão atual: famílias de layout como restrição real, filtro antecipado no fast solver, focos
  mutuamente exclusivos e Bubble Planner sem custo dentro das Voyages.
