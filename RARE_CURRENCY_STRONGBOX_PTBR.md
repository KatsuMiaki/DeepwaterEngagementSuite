# Rare Currency Strongbox Engine

Esta estratégia é ativada quando uma borda faz monstros raros soltarem currency valiosa, como Divine, Exalted, Annulment ou Ancient Orb.

## Montagem priorizada

1. **Sea Pillars** ocupa o tile tocado pela borda de recompensa por rare monster.
2. Todos os vizinhos ortogonais disponíveis recebem os melhores charts de:
   - `Additional Strongboxes in Adjacent Areas`;
   - Diviner's Strongboxes;
   - Arcanist's Strongboxes;
   - Operative's Strongboxes.
3. Os slots restantes priorizam `Increased number of Rare Monsters in all Voyage Areas`.
4. Sem Sea Pillars, o fallback usa Pelagic Abyss ou o melhor tile disponível. Brine King's Domain
   não é mais usado como fallback do motor de Strongboxes.

O solver ordena os fornecedores de strongbox pelo `Value1`, portanto charts de 5 strongboxes vencem os de 4, que vencem os tiers menores.

## Preparação dentro da Voyage

Leve Chaos Orbs e, conforme o estado inicial da caixa, Alchemy e Scouring Orbs. Nas strongboxes que caíram dentro do tile premiado, procure principalmente:

- `Guarded by 3 Rare Monsters`;
- `Stream of Monsters` como complemento/segunda melhor opção.

O primeiro mod é o núcleo econômico: cada caixa passa a fornecer vários monstros raros que recebem a recompensa da borda.

## Alterações no scorer

Strongboxes agora possuem simultaneamente as tags `Strongboxes` e `RareMonsters`. Isso não afirma que o conteúdo normal da caixa seja uma recompensa de rare monster; representa o potencial de reroll da própria caixa para gerar raros. Assim, o Voyage Planner reconhece a cadeia completa:

`chart fornecedor -> strongboxes no Sea Pillars -> rares rerollados -> currency da borda`

Perfis antigos são atualizados em memória quando carregados, preservando pesos personalizados e acrescentando apenas as tags necessárias.

## Separação do Brine King

Brine King's Domain usa uma estratégia independente: `Increased Rare Monsters in all Voyage
Areas` nos slots globais e `Adjacent Increased Rare Monsters`/`Giant Starfish` ao redor do Brine.
Strongboxes são proibidas nesse plano porque os raros gerados por elas não amplificam de forma
confiável a população própria do Brine King.

## Referências de calibração

- https://www.reddit.com/r/pathofexile/comments/1vgavh3/divine_voyage_w_friends/
- https://www.reddit.com/r/pathofexile/comments/1vcdzpe/loots_from_rare_drops_an_additional_divine_orb/
- https://www.reddit.com/r/pathofexile/comments/1v9qoh7/optimizing_divine_voyage/
