# Sistema de UI de Diálogo (Canvas)

Este documento explica como configurar e usar o novo sistema de diálogo baseado em Canvas.

## Arquivos Criados

### Scripts
- `Assets/Game/Scripts/Systems/Dialogue/DialogueUIController.cs` - Controla o painel de diálogo usando elementos de UI Canvas
- `Assets/Game/Scripts/Systems/Dialogue/DialogueManager.cs` - Gerencia o fluxo de diálogo e conecta com quests
- `Assets/Game/Scripts/Gameplay/Interaction/Targets/NPCInteractionTarget.cs` - Inicia o diálogo quando o NPC é clicado

## Como Configurar

### 1. Adicionar à Cena

1. Crie um GameObject vazio chamado `DialogueUI`
2. Adicione um Canvas ao GameObject ou crie um Canvas filho
3. Crie um painel filho para o diálogo e configure o layout como desejar
4. Adicione o script `DialogueUIController` ao GameObject `DialogueUI`
5. Configure os campos do `DialogueUIController` no Inspector:
   - **Dialogue Panel**: o GameObject do painel raiz
   - **Speaker Name Text**: `Text` para o nome do falante
   - **Dialogue Text**: `Text` para o conteúdo do diálogo
   - **Progress Text**: `Text` para mostrar o progresso
   - **Next Button**: `Button` para avançar
   - **Close Button**: `Button` para encerrar no fim

### 2. Garantir Managers na Cena

Certifique-se de que os seguintes objetos estejam na cena:
- `DialogueManager` (com o script `DialogueManager`)
- `QuestManager` (com o script `QuestManager`)

## Como Usar

### Criar um Diálogo

1. No menu Assets > Create > RPG > Dialogue > Dialogue Definition
2. Configure:
   - **Dialogue Id**: identificador único (ex: `welcome_dialogue`)
   - **Display Name**: nome exibido (ex: `Guarda da Vila`)
   - **Is Repeatable**: se pode ser repetido
   - **Lines**: array de linhas de diálogo
   - **Quest To Start**: missão opcional iniciada após o diálogo
   - **Start Quest After Dialogue**: se deve iniciar a missão automaticamente

### Configurar um NPC

1. Adicione o componente `NPCInteractionTarget` a um GameObject NPC
2. Configure:
   - **Dialogue Definition**: o ScriptableObject de diálogo
   - **Quest To Offer**: missão opcional para oferecer
   - **Start Quest After Dialogue**: se inicia automaticamente após o diálogo

### Funcionamento

1. O jogador clica com o botão direito em um NPC.
2. O `PlayerActionController` localiza o alvo e manda o jogador se aproximar.
3. Quando o jogador está dentro do alcance, `NPCInteractionTarget.PerformRightClickAction` chama `DialogueManager`.
4. O `DialogueManager` dispara eventos de linha de diálogo.
5. O `DialogueUIController` escuta os eventos e exibe o painel Canvas.
6. O botão `Próximo` avança o diálogo.
7. O botão `Fechar` aparece na última linha e encerra o diálogo.
8. Se configurado, o `QuestManager` inicia a missão após o término do diálogo.

## Requisitos de UI

- Utilize componentes `Text` e `Button` do Unity UI.
- Configure o painel raiz como `inactive` inicialmente para não aparecer antes do diálogo.
- O `DialogueUIController` precisa receber referências válidas para todos os campos configuráveis.

## Extensões Futuras

- Adicionar animações de fade in/out no painel
- Suporte a opções de resposta
- Integração com áudio de voz
- Sistema de resposta baseado em atribuição de personagem
- Salvar progresso de diálogo online para evitar repetições
