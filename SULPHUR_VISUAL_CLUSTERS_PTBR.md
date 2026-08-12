# Marcadores compactos de cristais de Sulphur

Esta versão separa a lógica visual dos cristais da lógica econômica do Bubble Planner.

## Visual

- Cristais próximos são agrupados em um único marcador.
- O tamanho do marcador cresce pela quantidade e pelo porte dos cristais no aglomerado.
- Não existe texto/nome abaixo do marcador.
- Não existe frame roxo de captura planejada.
- O ícone usa transparência configurável.
- No mapa grande é desenhado apenas o marcador do mapa; o ícone de mundo não é duplicado por baixo.

## Bolhas colocadas

Cada cristal coberto por uma bolha real é retirado imediatamente do agrupamento visual. Se todo o aglomerado estiver coberto, o marcador desaparece. Se somente parte estiver coberta, o marcador muda de posição e tamanho para representar apenas os cristais restantes.

## Trail

Por padrão, cristais de Sulphur não entram no Trail. Isso remove as linhas e os nomes repetidos sem impedir que o Bubble Planner use os cristais na pontuação.

## Opções

| Opção | Padrão |
|---|---:|
| Agrupar cristais | Ligado |
| Distância do agrupamento | 24 unidades de grid |
| Opacidade | 55% |
| Tamanho máximo | 150% |
| Esconder cristais cobertos | Ligado |
| Ignorar no Trail | Ligado |

A distância pode ser reduzida se dois depósitos distintos estiverem sendo combinados, ou aumentada se um mesmo depósito estiver aparecendo como dois marcadores.
