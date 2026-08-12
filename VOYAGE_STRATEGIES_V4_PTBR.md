# Voyage Planner v4 — estratégias exclusivas

O padrão agora é **um foco por Voyage**. Sem recompensa direta forte, o planner guarda charts
premium e prioriza um percurso S/$ curto.

## Formatos

- **S/$**: percurso rápido para boards fracos.
- **Compacto (+ fechado)**: liga todo o miolo e os cantos em uma malha fechada.
- **Linhas retas**: candelabro aberto para estratégias que exigem vários suportes.
- As três famílias possuem caixas independentes e podem ser combinadas.
- A exceção premium libera todos os formatos quando a nota da Voyage alcança o limite configurado.

## Focos

1. **Moeda por rare monster**: Sea Pillars no único tile premiado, Strongboxes adjacentes e rares
   globais nos demais slots. Divine, Exalted, Annulment e Ancient sempre forçam foco único.
2. **Brine King**: Brine em border de pack size/raros; Adjacent Rare ou Starfish ao redor; rares
   globais nos outros slots. Nunca recebe Strongboxes.
3. **Messages in a Bottle**: guarda os providers até atingir o mínimo e então usa o conjunto em
   uma única Voyage, concentrando os quatro melhores ao redor do centro.
4. **Dead Man's Sulphur**: guarda os implicits globais e só os libera com border de Sulphur;
   Chart Effect recebe os melhores providers.
5. **Ground loot**: só vira foco quando existe o combo completo No Equipment + Possessed Rares +
   Golden/Starfish + Sea Pillars. Partes isoladas permanecem guardadas.

## Segurança

Pantheon-touched com Brine King ou Possessed Rares é rejeitado como solução inválida, inclusive nos
fallbacks. Em foco de Brine, Strongboxes também são uma combinação inválida. O fallback pode
devolver charts reservados quando faltarem formatos compatíveis, mas não relaxa essas regras nem
os formatos marcados.
