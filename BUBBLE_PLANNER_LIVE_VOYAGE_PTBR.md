# Bubble Planner fixo — charts normais

O Bubble Planner foi desativado dentro de Voyages. Ao detectar uma área com 20 ou mais lanternas
(limite configurável), o plugin encerra o cálculo, remove a sugestão e não abre a janela do planner.
Ícones de loot, clusters de Sulphur e Trail continuam independentes.

Nos charts normais, clique em **Start search** uma vez. A sugestão permanece fixa quando:

- uma lanterna é colocada;
- um baú é aberto;
- uma entidade sai da memória;
- a quantidade de lanternas muda.

Ela só é recalculada quando uma nova entidade de recompensa, ainda fora das bolhas existentes, é
carregada. Nesse momento o cálculo usa as lanternas realmente restantes e todas as bolhas já
colocadas como âncoras. O debounce agrupa entidades carregadas no mesmo instante.

Isto evita a oscilação visual e elimina o custo contínuo que o antigo horizonte móvel provocava
nas Voyages grandes.
