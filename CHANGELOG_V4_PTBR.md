# DeepwaterEngagementSuite v4.0 — resumo

## v4.0.2 — proteção do limite de entidades

- Corrigido o spam `Unable to process ... entity candidate addresses due to the configured limit of 10000` no encerramento de Voyages.
- O modo Voyage agora permanece travado até outro chart Deepwater válido carregar; zerar `MaxLanternCount` no fim não reativa o Bubble Planner.
- `Collect Sleeping Entities` é pausado automaticamente durante Voyages e restaurado ao entrar em chart normal ou descarregar o plugin.
- A proteção é configurável em `Planner Settings -> Proteção de entidades em Voyages` e vem ativada por padrão.

## Bubble Planner

- desativado em Voyages (20+ lanternas por padrão);
- sugestões fixas nos charts normais;
- recálculo somente para nova entidade descoberta fora das bolhas;
- abrir loot, colocar lanternas ou descarregar entidade não invalida o plano.

## Voyage Planner

- máximo padrão de um foco;
- checkboxes independentes para S/$, compacto fechado e linhas retas;
- exceção configurável para boards premium;
- topologias desmarcadas são filtradas antes do assignment no fast solver;
- sem foco forte, nenhuma estratégia mediana ocupa o board e S/$ recebe prioridade.

## Estratégias

- Rare currency: um tile, Sea Pillars, Strongboxes adjacentes e rare monsters globais;
- Brine King: rare/pack-size border, Adjacent Rare/Starfish e rares globais, sem Strongboxes;
- Messages: providers guardados e gastos juntos;
- Sulphur: implicits globais guardados até existir border compatível;
- Ground loot: só ativa com No Equipment + Possessed + Golden/Starfish + Sea Pillars.

## Segurança

- Pantheon + Brine King é inválido;
- Pantheon + Possessed Rares é inválido;
- Strongboxes + Brine King é inválido quando o foco de Brine está ativo;
- fallback não relaxa segurança nem formatos marcados.
