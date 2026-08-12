# Bubble Planner v3 — notas da otimização

## Recompensas adicionadas

| Categoria | Identificação | Peso inicial |
|---|---|---:|
| Dead Man's Sulphur | `ResourceChestSmall/Base/Large/Huge` | 30 |
| Bottled Item | `BottledItemChest` | 40 |
| Sunken Loot | baús Deepwater não classificados e caminhos com `Sunken` | 8 |
| Gold Pile / Gold Treasure | caminhos com `GoldPile` ou `GoldTreasureChest` | 6 |
| Strongbox comum | qualquer `Metadata/Chests/StrongBoxes/...` | 12 |
| Arcanist, Diviner, Scarab e Operative | variantes reconhecidas da Strongbox | 20 |
| Pointer ainda não revelado | alvo de exploração | 8 |

Os pesos continuam configuráveis em **Bubble planner settings**. A migração força
Bottled Item para 40 e os quatro tamanhos de Sulphur para 30 somente uma vez; depois
disso, alterações manuais do usuário são preservadas.

## Nova busca

- Direciona candidatos para recompensas conhecidas em vez de depender apenas de
  pontos aleatórios na borda do polígono.
- Usa Pointer targets para atravessar regiões sem recompensa visível.
- Mantém as recompensas descobertas em cache até trocar de área.
- Pode ler sleeping entities quando `Core -> Debug -> CollectSleepingEntities` estiver
  ativado no ExileApi.
- Mede amostras de terreno dentro de cada bubble e evita posições com menos de 70%
  de área caminhável.
- Relaxa o limite quando um corredor estreito não oferece alternativa, evitando que
  o planner termine sem solução.
- Para a busca quando o resultado fica estável, reduzindo o tempo de espera.

## Ajustes recomendados

- `Cobertura caminhável mínima`: 70%.
- `Penalidade por terreno desperdiçado`: 60.
- `Tempo estável antes de parar`: 550 ms.
- `Maximum generation time`: 3 s como limite de segurança.
- Ativar `Core -> Debug -> CollectSleepingEntities` para ampliar a detecção inicial.

O ExileApi só pode identificar entidades fornecidas pelo cliente. A leitura de sleeping
entities e o cache aumentam bastante o alcance útil, mas não revelam conteúdo que o
jogo ainda não carregou.
