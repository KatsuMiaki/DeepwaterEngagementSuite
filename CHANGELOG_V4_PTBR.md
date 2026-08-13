# DeepwaterEngagementSuite v4.0 — resumo

## v4.1.0 — estratégias completas e estoques premium

- O planejador automático escolhe exatamente um plano econômico por Voyage.
- Rare currency só ativa com borda de Divine/Annulment/Ancient/Exalted, Sea Pillars, 3 charts livres de Strongbox e 5 globais de rare monsters.
- Operative e Diviner agora mantêm estoques separados e são usados em Voyages dedicadas de 9 charts.
- Dead Man's Sulphur só ativa com a borda compatível e 9 charts de pelo menos 25%.
- Message in a Bottle usa o conjunto de 8 providers ao redor de um único chart-alvo.
- Global/adjacent rare monsters, Strongboxes, Sea Pillars, Message e Sulphur 25% viraram reservas rígidas; os fallbacks não os gastam em Voyage de descarte.
- Barrels, Imprisoned Monsters, Soul Eater, Possessed e Tormented Spirits passam a ser preferidos como filler de Fast Voyage.
- A aba de estratégias foi reduzida aos limites que realmente alteram a decisão; os toggles isolados de Pantheon/Soul Eater/Possessed foram removidos da interface.
- A exceção premium de formato não libera mais todos os layouts: moeda rara pede somente linhas retas; Message/Brine pedem somente compacto. Fora disso, a seleção S/$ é respeitada literalmente.
- O fallback final que removia locks e restaurava charts premium foi eliminado.

## v4.0.4 — compatibilidade com a nova UI de fronteiras

- Restaurada a leitura dos 12 modificadores de fronteira após a atualização da interface de Voyage.
- O texto agora é lido primeiro em `3 -> 10 -> slot -> 1 -> 0`, conforme a nova árvore da UI.
- Mantidos fallbacks para a estrutura antiga, tooltip e texto direto do slot.
- Textos visíveis são convertidos para IDs `DeepwaterBorder...`, permitindo que solver, estratégias, pontuação econômica e cores continuem funcionando quando `Data.BorderMods` estiver vazio.
- Fingerprints do board e das fronteiras também usam o texto da UI como fallback, voltando a detectar rerolls e mudanças de borda.
- Valores numéricos ainda fornecidos pela API são preservados; fórmulas especiais de quantidade usam defaults conhecidos quando os valores não estiverem disponíveis.
- Textos novos ainda não mapeados são registrados uma única vez no log, facilitando diagnosticar futuras alterações sem spam por frame.
- Frases numéricas específicas têm prioridade sobre fallbacks genéricos, evitando classificar `180%/75% por conexão` como tier 1.

## v4.0.3 — estabilidade do overlay de bolhas

- Corrigida a exceção `ArgumentOutOfRangeException` em `PolygonClipper.Run()` ao renderizar bolhas colocadas.
- Eventos de fechamento obsoletos após subdivisão de segmentos não tentam mais remover o índice `-1` da status line.
- Corrigido o contrato do comparador para eventos geométricos equivalentes e referências idênticas.
- A união das bolhas agora é balanceada em pares, reduzindo o tamanho dos polígonos intermediários e o custo do clipping.
- Círculos inválidos ou de raio zero são ignorados.
- Se uma geometria degenerada ainda não puder ser unida, o overlay desenha os contornos individuais sem derrubar o `PerformanceRender`.
- Círculos do overlay usam 64 segmentos em vez de 100, mantendo boa aparência com menos trabalho de CPU.

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
