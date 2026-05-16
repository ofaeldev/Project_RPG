# RPG Project - Memoria de Colaboracao

Last updated: 2026-05-16

Este arquivo registra preferencias de direcao, pesquisa e colaboracao para manter a evolucao do jogo consistente entre sessoes.

## Como trabalhar neste projeto

- Antes de propor ou implementar sistemas, ler `Assets/Game/Docs/SYSTEMS_NOTES.md` e respeitar os limites de responsabilidade ja documentados.
- Codex pode e deve opinar sobre a evolucao do jogo, sugerindo melhorias de design, arquitetura, jogabilidade, balanceamento e qualidade de codigo quando fizer sentido.
- Quando houver duvida real de design ou regra de sistema, perguntar antes de codar.
- So implementar codigo quando o sistema estiver bem definido, com aproximadamente 95% de confianca sobre a solucao.
- Preferir passos pequenos, completos, testaveis e polidos antes de expandir a proxima camada.
- Ao trabalhar em sistemas inspirados por Tibia, consultar o Tibia Wiki como referencia antes de fechar a proposta.
- Usar referencias externas como inspiracao, nao como copia literal. O jogo deve manter sua propria identidade e regras.

## Referencias recorrentes

- Tibia Wiki - Hit Point: https://tibia.fandom.com/wiki/Hit_Point
- Tibia Wiki - Formulae: https://tibia.fandom.com/wiki/Formulae
- Tibia Wiki - Creatures: https://tibia.fandom.com/wiki/Creatures
- Unity - Three ways to architect your game with ScriptableObjects: https://unity.com/how-to/architect-game-code-scriptable-objects
- Unity - Advanced programming and code architecture: https://unity.com/how-to/advanced-programming-and-code-architecture
- Unity Learn - Introduction to Optimization in Unity: https://learn.unity.com/tutorial/introduction-to-optimization-in-unity

## Boas praticas que devem guiar sugestoes

- Separar dados editaveis de comportamento: usar ScriptableObjects para valores de balanceamento, configuracoes de criaturas, ataques, loot, quests e efeitos.
- Evitar duplicar estado: inventario, quests, dialogos, mundo persistente e combate devem continuar com donos claros.
- Prefabs e componentes devem funcionar com poucas dependencias rigidas e com referencias configuraveis pelo Inspector.
- Controllers decidem quando agir; modelos, resolvers e managers decidem regras.
- UI deve pedir a managers/sources que executem a regra, nao alterar estado diretamente.
- Performance deve ser medida antes de otimizar demais; quando houver muitos objetos temporarios, considerar pooling.
- Sistemas de feedback visual devem exibir resultados ja calculados, sem recalcular regra de jogo.
- Expandir comportamento por dados primeiro; criar scripts especificos so quando dados nao expressarem mais a diferenca.

## Direcao inspirada em Tibia

- Separar pontos de vida, atributos de combate, formulas de dano, mitigacao, loot e IA.
- Criaturas devem ter caracteristicas claras e configuraveis: HP, dano/ataque, defesa/mitigacao, comportamento, loot e recompensa.
- Dano fisico pode evoluir em camadas: ataque base, skill/equipamento, defesa, armadura/mitigacao, tipos de dano e resistencias.
- Loot deve ser consultavel e previsivel para design, mas rolado uma vez por fonte de loot no runtime.
- A experiencia de combate deve priorizar leitura clara: alvo selecionado, alcance, follow on/off, dano recebido, cura, morte e corpo lootavel.

## Estado atual lembrado

- O jogo esta pausado no polimento de combate apos atributos de ataque/defesa, follow opcional, feedback de alvo fora de alcance com follow desligado, IA simples de inimigos, loot em corpo e a primeira leva de refatoracao arquitetural.
- A direcao de inimigos inspirada em Tibia separa duas perguntas no Inspector: quando o inimigo entra em combate e se ele segue, segura posicao ou foge depois de engajar.
- Os ratos da TutorialScene estao configurados como `AggressiveOnSight + ChaseTarget`, com detection range menor para evitar enxame cedo demais e `EnemyCombatVisualPresenter` para ler chase/attack no Play Mode.
- A arquitetura desejada para inimigos e data-driven: criar inimigo novo deve ser configurar stats, ataque, loot e `EnemyCombatBehaviorSettings`; so tocar codigo quando faltar uma politica generica reaproveitavel.
- Para evitar empate infinito em chase/flee, o player segue um pouco para dentro do alcance de ataque, `CombatActor` tem pequena tolerancia de borda, e fuga tem `fleeSpeedMultiplier` configuravel.
- A decisao de IA inimiga foi extraida para `EnemyCombatContext`, `EnemyCombatIntent` e `EnemyCombatBehaviorResolver`; `EnemyCombatController` deve continuar fino, montando contexto e executando intencoes.
- Politicas atuais de inimigo: `AggressiveOnSight`, `RetaliateWhenTargeted`, `RetaliateWhenDamaged`, `Passive`; movimento: `HoldPosition`, `ChaseTarget`, `FleeWhenDamaged`, `FleeAtLowHealth`, `KeepDistance`.
- Validacao mais recente: runtime/editor/tests builds com 0 erros e Unity EditMode passando 54/54.
- Refatoracoes recentes: visual de selecao saiu de `CombatActor`/`CombatTarget` para `CombatSelectionPresenter`; feedback, save, quest reward/log, typewriter/atalhos de dialogo, presenter/detalhes/drag icon/atalhos/view/fluxo/transfer/drop do inventario e view/formatter/input do loot ganharam classes auxiliares mais testaveis; inimigos/camera aceitam referencias injetadas com lookup antigo como fallback.
- Proximos passos recomendados: testar feel de chase/flee em Play Mode, calibrar alcance/cadencia/velocidade com inspiracao Tibia, depois criar o primeiro arquetipo alternativo por configuracao antes de adicionar novas politicas genericas.
