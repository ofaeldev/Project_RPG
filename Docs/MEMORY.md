# RPG Project - Memoria Mestra

Last updated: 2026-05-16

Este e o documento mestre de memoria, arquitetura, sistemas e direcao do projeto. `Docs/Architecture.md` e `Assets/Game/Docs/SYSTEMS_NOTES.md` ficam apenas como ponte historica para este arquivo. Em caso de divergencia, seguir este `Docs/MEMORY.md`.

## Papel do Codex

- Codex atua como arquiteto tecnico, revisor e agente de desenvolvimento para um RPG 2D top-down estilo Tibia feito em Unity/C#.
- Ser direto, tecnico, opinativo e criterioso. Nao concordar automaticamente com decisoes que possam gerar acoplamento, retrabalho ou dificuldade futura.
- Pensar como arquiteto de software e game designer ao mesmo tempo: sugerir melhorias de design, arquitetura, jogabilidade, balanceamento e qualidade de codigo mesmo quando nao forem pedidas explicitamente.
- Antes de propor ou implementar sistemas, ler esta memoria e respeitar os limites de responsabilidade ja documentados.
- Quando houver duvida real de design ou regra de sistema, perguntar antes de codar.
- So implementar codigo quando o sistema estiver bem definido, com aproximadamente 95% de confianca sobre a solucao.
- Preferir passos pequenos, completos, testaveis e polidos antes de expandir a proxima camada.
- Priorizar arquitetura solida antes de velocidade. Nao criar solucao rapida se ela prejudicar modularidade, testes ou evolucao.

## Objetivo do Projeto

- Construir e manter um projeto escalavel, limpo, modular e facil de evoluir.
- Foco em arquitetura, boas praticas, separacao de responsabilidades e padronizacao de gameplay inspirada em Tibia.
- Engine: Unity.
- Linguagem: C#.
- Estilo: RPG 2D top-down / tile-based.
- Ferramentas: Codex + MCP Unity.
- Prioridade: arquitetura solida antes de velocidade.
- Evitar scripts espalhados, excesso de `MonoBehaviour` e logica acoplada em GameObjects/prefabs.

## Regras de Arquitetura Permanentes

- Aplicar SRP com rigor: cada classe deve ter uma responsabilidade clara.
- Manter baixo acoplamento e alta coesao.
- Preferir classes puras de C# para regras de dominio, calculos, resolvers, modelos, DTOs, value objects e servicos testaveis.
- Usar `MonoBehaviour` como camada de integracao com Unity: input, scene references, chamadas a servicos/sistemas e apresentacao visual.
- Evitar regra de gameplay complexa em `Update`, `OnTrigger`, UI ou prefabs.
- Evitar singletons globais sem justificativa clara. Quando existirem por legado ou conveniencia, nao espalhar novas dependencias diretas sem avaliar migracao.
- Evitar God Classes, managers gigantes, scripts que fazem tudo e scripts especificos por criatura/item quando dados ou politicas genericas resolvem.
- Preferir composicao a heranca.
- Separar dominio, aplicacao, infraestrutura, apresentacao e dados quando isso reduzir acoplamento real.
- Centralizar sistemas de inventario, combate, movimento, skills, vocacoes, criaturas, loot, quests, NPCs, dialogo e economia.
- Refatorar ou recomendar refatoracao ao encontrar duplicacao, responsabilidade misturada ou regra acoplada a view/prefab.
- Prefabs devem consumir dados e adaptar Unity; nao devem ser donos das regras do jogo.
- Dados editaveis devem ficar fora do codigo quando fizer sentido, preferencialmente em ScriptableObjects.
- Comentarios devem explicar intencao/decisao, nao repetir cada linha.

## Estrutura de Camadas Desejada

Direcao futura, migrando gradualmente sem quebrar o projeto atual:

- `Assets/Game/Domain`: entidades puras, regras de negocio, calculos, modelos e value objects.
- `Assets/Game/Application`: casos de uso, servicos de gameplay e orquestracao entre sistemas.
- `Assets/Game/Infrastructure`: persistencia, carregamento de dados, adapters e integracao com arquivos/assets.
- `Assets/Game/Presentation`: MonoBehaviours, UI, animacoes, inputs, views e prefabs.
- `Assets/Game/Data`: ScriptableObjects, configs, tabelas de itens, criaturas, spells e vocacoes.
- `Assets/Editor`: ferramentas de editor.
- `Assets/Game/Tests`: testes unitarios e de integracao.

Estrutura atual a preservar enquanto migramos por etapas:

- `Assets/Game/Scripts/Inputs`: leitura de input e adaptadores de controle.
- `Assets/Game/Scripts/Character`: componentes reutilizaveis por jogador, NPCs e monstros.
- `Assets/Game/Scripts/Gameplay`: dados/regras de gameplay e contratos proximos ao dominio.
- `Assets/Game/Scripts/Systems`: orquestradores, controllers e integracoes de sistema.
- `Assets/Game/Scripts/Shared`: utilitarios pequenos compartilhados.
- `Assets/Game/ScriptableObjects`: configuracoes e dados editaveis.
- `Assets/Game/Prefabs`: prefabs do jogo.
- `Assets/Game/Art`: sprites, tilesets, animacoes e materiais.
- `Assets/Game/Scenes`: cenas do projeto.
- `Assets/Game/Tests`: testes automatizados.

## Fluxo Antes de Codar

Antes de implementar qualquer mudanca relevante, responder ou registrar:

- Diagnostico rapido do problema.
- Melhor abordagem arquitetural.
- Arquivos/classes que serao criados ou alterados.
- Riscos tecnicos.
- Alternativas possiveis.
- Recomendacao final.

## Durante a Implementacao

- Criar codigo limpo, legivel e testavel.
- Usar nomes claros e consistentes.
- Evitar overengineering, mas nao sacrificar arquitetura.
- Preferir enums, value objects, interfaces e DTOs quando fizer sentido.
- Centralizar configuracoes em ScriptableObjects ou arquivos de dados quando apropriado.
- Separar dados de comportamento.
- Uma funcionalidade por vez: implementar, testar no editor, revisar e so entao expandir.
- Com MCP Unity: inspecionar cena, prefabs, scripts e dependencias antes de editar; evitar modificar assets/prefabs sem necessidade; validar referencias, compilacao, console e testes depois.
- Preservar compatibilidade com sistemas existentes, a menos que haja uma migracao clara e segura.

## Depois de Implementar

Entregar sempre que houver mudanca de codigo/sistema:

- Resumo do que foi alterado.
- Justificativa arquitetural.
- Como testar no Unity.
- Possiveis melhorias futuras.
- Dividas tecnicas percebidas.
- Aviso claro se algo violar o padrao desejado do projeto.

## Referencias Recorrentes

- Tibia Wiki BR: https://www.tibiawiki.com.br/
- TibiaWiki Fandom - Hit Point: https://tibia.fandom.com/wiki/Hit_Point
- TibiaWiki Fandom - Formulae: https://tibia.fandom.com/wiki/Formulae
- TibiaWiki Fandom - Creatures: https://tibia.fandom.com/wiki/Creatures
- Unity - Three ways to architect your game with ScriptableObjects: https://unity.com/how-to/architect-game-code-scriptable-objects
- Unity - Advanced programming and code architecture: https://unity.com/how-to/advanced-programming-and-code-architecture
- Unity Learn - Introduction to Optimization in Unity: https://learn.unity.com/tutorial/introduction-to-optimization-in-unity

## Direcao Inspirada em Tibia

- Ao trabalhar em sistemas inspirados por Tibia, consultar primeiro o Tibia Wiki BR e usar o TibiaWiki Fandom em ingles como apoio quando necessario.
- Usar wikis como referencia de design, nomes, conceitos, vocacoes, monstros, itens, spells, quests, NPCs, loot, skills e progressao, sem copiar conteudo protegido literalmente.
- Quando uma decisao usar informacao da wiki, informar a fonte consultada e o que foi adaptado.
- Se houver diferenca entre "igual ao Tibia" e "melhor para nosso projeto", apontar o trade-off e recomendar claramente uma opcao.
- Separar pontos de vida, atributos de combate, formulas de dano, mitigacao, loot e IA.
- Criaturas devem ter caracteristicas claras e configuraveis: HP, dano/ataque, defesa/mitigacao, comportamento, loot e recompensa.
- Dano fisico pode evoluir em camadas: ataque base, skill/equipamento, defesa, armadura/mitigacao, tipos de dano e resistencias.
- Loot deve ser consultavel e previsivel para design, mas rolado uma vez por fonte de loot no runtime.
- A experiencia de combate deve priorizar leitura clara: alvo selecionado, alcance, follow on/off, dano recebido, cura, morte e corpo lootavel.

## Principios de Game Design

- Progressao de personagem clara.
- Combate simples, legivel e tatico.
- Vocacoes com identidade forte.
- Skills evoluindo por uso ou experiencia, conforme a regra escolhida para o nosso jogo.
- Loot com raridade, previsibilidade suficiente para design e economia controlada.
- Monstros com comportamento previsivel, mas interessante.
- NPCs com dialogo por palavras-chave quando isso combinar com o sistema.
- Mundo tile-based com exploracao, risco e recompensa.
- Morte, penalidades e recuperacao balanceadas.
- Evitar power creep e sistemas dificeis de manter.

## Estado Validado Atual

- Git estava limpo antes desta consolidacao.
- Unity: `6000.3.10f1`.
- Cena ativa: `Assets/Game/Scenes/TutorialScene.unity`.
- `TutorialScene` validada com 0 missing scripts, 0 broken prefabs e 0 issues.
- Builds sequenciais validados com 0 erros e 0 avisos:
  - `RPGProject.Runtime.csproj`
  - `Assembly-CSharp-Editor.csproj`
  - `RPGProject.EditModeTests.csproj`
- Unity EditMode tests passando: 54/54.
- Unity PlayMode tests passando: 6/6, cobrindo fluxo real da `TutorialScene` com quest dos ratos, combate, morte, corpse loot, claim de loot, aggro/chase, retorno andando para home/leash, regressao contra teleport visual de ataque, pacing de dano e indicadores visuais principais.
- Console pos-teste sem erros de gameplay; apenas warnings esperados do Unity Test Framework e o warning intencional de recompensa com inventario cheio.

## Cena Tutorial Atual

- Cena: `Assets/Game/Scenes/TutorialScene.unity`.
- `SampleScene` foi renomeada para `TutorialScene` e e a primeira cena jogavel/tutorial.
- Gerada por `Assets/Editor/TutorialQuestSceneBuilder.cs` via menu `RPG Project > Build Tutorial Quest Scene`.
- Historia inicial: jogador acorda em Vale de Lumen apos uma nevoa estranha chegar aos campos. Lio envia o jogador ate Mira, a guarda da vila.
- Primeira quest: `O primeiro sinal`, falar com Mira.
- Segunda quest: `Ratos na nevoa`, derrotar 10 mist rats.
- Retorno a Mira apos a quest dos ratos concede `OldChapelKey` / `Chave da Capela Antiga`.
- Tutorial desativa load/save automatico do `GameProgressSaveManager` para evitar que PlayerPrefs antigos pulem etapas de teste.
- Tutorial conecta player stats, rat stats, movement, attack settings, behavior settings, enemy AI, loot/corpse, feedback UI e follow toggle.

Conexoes de cena:

- `Managers/Dialogue System`: `DialogueManager`, `QuestManager`.
- `Managers/Inventory System`: `InventoryManager`, `InventoryWorldDropper`.
- `Managers/Loot System`: `LootService`.
- `Managers/Gameplay System`: `GameplayInputBlocker`.
- `Managers/Save System`: `GameProgressSaveManager`.
- `UIManager/Gameplay UI System`: `GameplayUIManager`, `GlobalFeedbackUIController`.
- `UIManager/Dialogue UI System`: `DialogueUIController`.
- `UIManager/Quest Log UI System`: `QuestLogUIController`.
- `UIManager/Inventory UI System`: `InventoryUIController`.
- `UIManager/Loot UI System`: `LootUIController`.
- `UIManager/Combat UI System`: `CombatFollowToggleUIController`, `CombatWorldUIController`.
- `Canvas`: dialogue panel, choices, `GlobalFeedback`, `QuestLogToggleButton`, `QuestLogPanel`, `InventoryToggleButton`, `InventoryPanel`, `LootPanel`, `CombatFollowToggle`.
- NPCs existentes possuem `NPCQuestIndicator`.

## Sistemas Implementados e Donos

### Input, Movimento e Interacao

Scripts principais:

- `PlayerInputReader`, `PlayerInputState`, `KeyboardShortcutUtility`.
- `PlayerMovementController`, `PlayerActionController`.
- `CharacterMotor2D`, `CharacterMovementSettings`.
- `RightClickActionTarget` e targets especificos de NPC, door, container, enemy, item pickup e key pickup.

Regras:

- `PlayerInputReader` e a unica camada de captura de input do jogador.
- `PlayerInputReader` publica eventos e mantem estado em `PlayerInputState`; movement/action controllers consomem intencoes.
- `CharacterMotor2D` e o unico componente que escreve velocidade/posicao fisica em `Rigidbody2D`.
- Click-to-move deve usar `PlayerMovementController.SetMoveTarget()` e `CharacterMotor2D.SetMovementTarget()`.
- Nao escrever diretamente no `Rigidbody2D` a partir de input/action controllers.
- Nao adicionar input readers por feature enquanto eventos/estado existentes forem suficientes.
- Doors e containers consultam inventario central; nao possuem ownership local de itens.

### Dialogue, Quest e Quest Log

Scripts principais:

- `DialogueDefinition`, `QuestDefinition`, `QuestKillTarget`.
- `DialogueManager`, `QuestManager`, `DialogueUIController`.
- `DialogueTypewriter`, `DialogueKeyboardInput`.
- `QuestRewardService`, `QuestLogProjection`.
- `QuestLogUIController`, `QuestLogPanelView`, `QuestLogContentFormatter`, `QuestLogKeyboardInput`.
- `NPCInteractionTarget`, `NPCQuestIndicator`.

Regras:

- Dialogue ScriptableObjects possuem linhas, speaker side, retrato por linha, choices, branch/next dialogue e quest link.
- Choices podem aceitar, recusar, continuar, fechar e ramificar.
- Conditions de dialogue/choice suportam `QuestState` e `InventoryRequirement`.
- `DialogueManager` e dono de estado ativo, progresso visto, filtro de choices, chaining e snapshots.
- `QuestManager` e dono de estado de quests, progresso de objetivos, quest log, reward claiming e snapshots.
- Nao criar outro store de progresso de dialogo ou quest. Usar `CreateProgressSnapshot()` e `LoadProgressSnapshot()`.
- Quests concedem rewards por `QuestDefinition`; reward claiming passa por `QuestManager.TryClaimReward()` e falha sem mudar estado se o inventario nao couber.
- `QuestKillTarget` reporta kills ao `QuestManager`.
- UI de quest deve ler `QuestManager.GetQuestLogEntries()`, nao dicionarios internos.
- Formatacao do quest log fica em `QuestLogContentFormatter`.
- `NPCQuestIndicator` e dono da apresentacao do marcador de quest, nao `NPCInteractionTarget`.

### Global Feedback UI e Bloqueio de Gameplay

Scripts principais:

- `GameplayUIEvents`, `GameplayUIManager`, `GameplayInputBlocker`, `GlobalFeedbackUIController`.
- `GameplayFeedbackMessage`, `FeedbackRateLimiter`.

Regras:

- Painel/UI que bloqueia movimento/acao deve chamar `GameplayInputBlocker.Instance.SetBlocker(this, visible)`.
- Movement/action controllers consultam `GameplayInputBlocker`, nao paineis individuais.
- Gameplay pede feedback por `GameplayUIEvents`; nao chamar `GlobalFeedbackUIController.Instance` diretamente.
- Passar `source: gameObject` em feedback de interacao para cooldown/replacement funcionar.
- Mensagens possuem tipos info, success, warning, error, quest e loot.
- Rate limiting evita spam do mesmo objeto; fontes diferentes podem substituir a mensagem atual com pequeno delay.

### Inventory, Items e Requirements

Scripts principais:

- `ItemDefinition`, `ItemStackDefinition`, `ItemUseEffect`, `FeedbackItemUseEffect`.
- `LootTableDefinition`, `LootDropEntry`, `LootRarity`, `ILootSource`.
- `InventoryManager`, `InventoryModel`, `IInventoryService`.
- `InventoryRequirement`, `InventoryRequirementService`.
- `InventoryUIController`, `InventoryPanelView`, `InventoryPresenter`, `InventorySlotUI`.
- `InventoryInteractionFlow`, `InventorySlotTransferService`, `InventoryDropFlow`, `InventoryWorldDropper`.
- `InventoryDetailsFormatter`, `InventoryDragIconPresenter`, `InventoryKeyboardInput`.

Regras:

- `ItemDefinition` e a fonte preferencial para itens reais: stable `itemId`, display name, description, category, icon e stacking.
- String ids sao fallback/migracao.
- `InventoryManager` e dono de ownership, amounts, snapshots e uso de itens.
- Nao criar item ownership em doors, containers, quests ou pickups.
- `InventoryRequirement` centraliza requirement data; `InventoryRequirementService` satisfaz/consome contra `IInventoryService`.
- Nao duplicar lock checks em doors/containers/conditions.
- Item-specific use logic nao entra em `InventoryManager`; criar/atribuir `ItemUseEffect`.
- UI de inventario suporta abrir por botao/tecla `I`, selection por numero/clique, use/drop/cancel, drag/drop reorder e details.
- Drag para fora do inventario passa por `InventoryWorldDropper`; UI slots nao criam drops diretamente.
- Futuro equipment/hotbar/container swap deve reutilizar `InventorySlotUI` e criar regras acima de `InventoryManager`.

### Loot e Corpse Loot

Scripts principais:

- `LootService`, `LootUIController`, `LootPanelView`, `LootContentFormatter`, `LootKeyboardInput`.
- `LootClaimService`.
- `CorpseLootSource`, `ContainerInteractionTarget`, `ILootSource`, `LootTableDefinition`.

Regras:

- `LootService` centraliza fluxo de abrir UI, fallback claim-all e feedback.
- Interaction targets chamam `LootService.OpenOrClaimAll()`.
- UI pede claim por `ILootSource.ClaimAllLoot()`, nao adiciona itens diretamente.
- `LootClaimService` isola transferencia de stacks para inventario.
- Containers e corpses possuem conteudo/estado/claim rules, mas nao decidem fluxo de UI.
- Corpse loot vive em `CorpseLootSource`; death/decay nao concedem itens diretamente.
- Preferir `LootTableDefinition` para loot de inimigo; direct `ItemStackDefinition[]` em corpse e fallback.
- Loot table rola uma vez por fonte de loot no runtime.

### Combat, Health e Enemy AI

Scripts principais:

- `HealthComponent`, `HealthChange`, `HealthChangeType`.
- `CombatStatsDefinition`, `ICombatStatsProvider`, `CharacterCombatStats` legacy.
- `CombatAttackSettings`, `DamageResolver`, `BasicDamageResolver`, `DamageContext`, `DamageResult`.
- `CombatActor`, `AutoAttackController`, `EnemyCombatController`.
- `EnemyCombatBehaviorSettings`, `EnemyCombatBehaviorResolver`, `EnemyCombatContext`, `EnemyCombatIntent`.
- `CombatTarget` legacy/compatibility.
- `CombatFollowToggleUIController`, `CombatWorldUIController`, `FloatingCombatText`, `HitFlashPresenter`, `EnemyCombatVisualPresenter`, `CorpseLootIndicatorPresenter`, `CorpseDecayController`.

Regras:

- `HealthComponent` so possui HP/death state: dano, cura, max-health, revive explicito e eventos `HealthChanged`, `Died`, `Revived`.
- Nao colocar attack timing, AI, armor, mitigation, damage types, loot, quest progress ou UI em `HealthComponent`.
- Atributos de combate ficam em `CombatActor` para combatants, ou outro `ICombatStatsProvider` quando nao houver actor.
- `CombatActor` centraliza identidade de combate e execucao de ataque: stats, target, targetability, range, cooldown, damage resolution/application e eventos.
- Controllers decidem quando agir; formulas ficam em `DamageResolver`/`BasicDamageResolver`.
- Nao duplicar cooldown/range/damage application em player/enemy controllers.
- `AutoAttackController` e player-specific: follow target, chase target selecionado e pedir ataques ao `CombatActor`.
- `EnemyCombatController` e AI integration: resolve target, monta contexto, recebe intent de `EnemyCombatBehaviorResolver`, executa movimento/ataque.
- `EnemyCombatController` tambem guarda `HomePosition` e leash basico: ao perder alvo fora do alcance de perseguicao, o inimigo deve retornar andando para a origem/home, nunca teleportar. Respawn completo deve ser um sistema separado futuro em cima de spawn/home anchors.
- `EnemyCombatBehaviorResolver` e a camada pura de decisao de enemy AI. Nao crescer branches por inimigo no controller.
- `EnemyCombatBehaviorSettings` separa engagement de movement:
  - Engagement: `AggressiveOnSight`, `RetaliateWhenTargeted`, `RetaliateWhenDamaged`, `Passive`.
  - Movement: `HoldPosition`, `ChaseTarget`, `FleeWhenDamaged`, `FleeAtLowHealth`, `KeepDistance`.
- Novos inimigos devem nascer configurando stats, attack settings, loot e behavior asset. Evitar novo script especifico ate faltar uma politica generica.
- `CombatWorldUIController` centraliza selection frames, enemy health bars e floating combat text a partir de eventos de `HealthComponent`/`CombatActor`.
- Visual presenters exibem resultados ja calculados; nao recalculam dano/regra.
- `CorpseDecayController` mantem mortos lootaveis e depois desativa corpo; nao desativar imediatamente inimigo lootavel.
- `EnemyInteractionTarget` pode reagir a morte para quest/deactivation, mas nao deve conter cadence/damage logic.
- Chase/flee edge cases devem ser resolvidos por distancia de abordagem, range tolerance e speed tuning, nao movendo dano para movement.
- Player follow usa buffer pequeno para entrar um pouco no range; `CombatActor` tem tolerancia de borda; fleeing enemies possuem `fleeSpeedMultiplier`.
- Feedback visual de ataque/death nao pode animar o transform raiz do inimigo quando o `SpriteRenderer` esta no proprio root; nesses casos deve animar apenas cor/indicador ou um child renderer dedicado. Esse contrato evita teleport/snap de posicao.

Estado de combate atual:

- Player tem 100 HP, starter combat stats, 1.5 attack range e 1 attack per second.
- Mist rats tem 20 HP, stats/attack fracos, `AggressiveOnSight + ChaseTarget`, detection range 3.25 e `EnemyCombatVisualPresenter`.
- Mist rats usam home/leash basico: quando a perseguicao quebra, entram em `EnemyCombatState.Returning` e voltam andando para a origem capturada.
- `BasicDamageResolver` calcula `baseDamage + attacker attack - target defense`, clampado em zero.
- `CombatTarget` e `CharacterCombatStats` permanecem para compatibilidade/testes/legacy, mas novos combatants devem preferir `CombatActor`.

### World State Persistence e Save/Load

Scripts principais:

- `PersistentWorldState`, `WorldStateSnapshotService`.
- `GameProgressSaveManager`.
- `ISaveStorage`, `PlayerPrefsSaveStorage`, `GameProgressSaveData`.

Regras:

- `PersistentWorldObject` guarda stable `worldObjectId`.
- `IPersistentWorldState` permite snapshot/restore local e pequeno por componente.
- Doors, containers e item pickups implementam persistencia de mundo.
- `GameProgressSaveManager` salva/restaura quest, dialogue, inventory e world object snapshots.
- Save/load atual e ponte temporaria em PlayerPrefs.
- Futuro save global deve substituir ou envolver `GameProgressSaveManager`, reaproveitando snapshot APIs; nao duplicar quest/dialogue save data em formato paralelo sem migracao.

## Refatoracoes Recentes

- `PlayerInputState` puro e handoff event-driven de `PlayerInputReader` para movement/action controllers.
- Removidos input readers/controllers duplicados: `PlayerMovementInputReader`, `PlayerActionInputReader`, `PlayerInputMovementController`.
- `GameplayUIEvents` e `GameplayUIManager` centralizam feedback de gameplay.
- Quest feedback e item-use feedback foram consolidados em `GameplayUIManager`.
- `UIManager` dedicado separa UI controllers de gameplay/service `Managers`.
- `CombatActor` centraliza ataque para player e inimigos.
- `AutoAttackController` e `EnemyCombatController` delegam range/cooldown/target/damage ao `CombatActor`.
- `CombatWorldUIController` centraliza selection frames, health bars e floating combat text.
- `EnemyCombatVisualPresenter` apresenta leitura de estado do inimigo com cores sutis, indicador de aggro, lunge de ataque e pose de morte.
- `EnemyCombatController` ganhou home/leash return com estado `Returning`, preparando a base para um sistema futuro de respawn inspirado em Tibia sem acoplar respawn diretamente na IA.
- `EnemyCombatVisualPresenter` foi protegido contra teleport visual: lunge/death pose so mexem em renderer filho, nao no transform raiz do inimigo.
- `CorpseLootIndicatorPresenter` apresenta marker de corpse lootavel enquanto `CorpseLootSource` ainda tem loot disponivel.
- `FeedbackRateLimiter`, `GameplayFeedbackMessage`, save storage/snapshot helpers, quest reward/log projection, inventory details/drag/key/view/presenter/flow/transfer/drop e loot view/formatter/input foram extraidos.
- `LootClaimService` e `InventoryRequirementService` isolam regras puras de transferencia/requisitos.
- `EnemyCombatContext`, `EnemyCombatIntent` e `EnemyCombatBehaviorResolver` extraem decisao de AI para camada testavel.
- `EnemyCombatVisualPresenter` conectado aos mist rats para leitura de estado.
- Testes PlayMode criados em `Assets/Game/Tests/PlayMode/TutorialScenePlayModeTests.cs`, validando a cena tutorial de ponta a ponta sem input manual e garantindo que mist rats engajam o player dentro do detection range, retornam andando para home quando quebram leash e nao teleportam por feedback visual de ataque.

## Proximo Passo Recomendado

Recomendacao clara: fazer uma rodada de polimento de combate em Play Mode antes de adicionar sistemas novos.

Escopo sugerido:

- Testar feel de detection, chase, flee, attack range e attack pacing na `TutorialScene`.
- Ajustar dados existentes de player e mist rats primeiro, sem criar codigo.
- Criar o primeiro arquetipo alternativo somente por configuracao, preferencialmente um inimigo `HoldPosition` ranged/caster simples ou um creature timido `FleeWhenDamaged`.
- So adicionar nova politica em `EnemyCombatBehaviorSettings` se a configuracao atual nao expressar o comportamento desejado.

Riscos:

- Comecar vocacoes, spells ou economia agora pode esconder problemas basicos de combate.
- Criar scripts especificos por inimigo cedo demais vai contra a direcao data-driven.
- Migrar pastas para `Domain/Application/Presentation` agora teria custo alto e pouco retorno imediato; melhor migrar junto de refatoracoes reais.

Depois do feel basico:

- Adicionar animations/Animator states reais para chase/attack/death.
- Avaliar corpse loot model/service se regras de transferencia crescerem.
- Planejar atributos mais ricos: armor/mitigation, damage types, resistencias, skills e vocacoes.
