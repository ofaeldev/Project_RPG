# RPG Project - Arquitetura Inicial

## Objetivo

Este projeto sera construido em pequenas funcionalidades completas. Cada etapa deve ficar compreensivel, testavel e polida antes de seguir para a proxima.

## Principios

- SRP primeiro: cada script deve ter um motivo claro para mudar.
- Composicao sobre heranca: personagens sao montados com componentes pequenos.
- Dados editaveis fora do codigo: valores de balanceamento devem ir para ScriptableObjects quando fizer sentido.
- Comentarios explicam intencao, nao repetem cada linha.
- Uma funcionalidade por vez: implementar, testar no editor, revisar e so entao expandir.

## Estrutura de Pastas

- `Assets/Game/Scripts/Inputs`: leitura de input e adaptadores de controle.
- `Assets/Game/Scripts/Character`: componentes reutilizaveis por jogador, NPCs e monstros.
- `Assets/Game/Scripts/Player`: componentes especificos do jogador local.
- `Assets/Game/Scripts/Shared`: utilitarios pequenos compartilhados.
- `Assets/Game/ScriptableObjects`: configuracoes e dados editaveis.
- `Assets/Game/Prefabs`: prefabs do jogo.
- `Assets/Game/Art`: sprites, tilesets, animacoes e materiais.
- `Assets/Game/Scenes`: cenas do projeto.
- `Assets/Game/Tests`: testes automatizados.

## Funcionalidade 1: Movimento Top-Down

Scripts envolvidos:

- `PlayerInputReader`: le teclado, mouse/gamepad e expoe estado bruto de input do jogador.
- `CharacterMotor2D`: recebe uma direcao e aplica velocidade no `Rigidbody2D`.
- `PlayerMovementController`: conecta input local ao motor.
- `CharacterMovementSettings`: guarda dados de movimento ajustaveis no Inspector.

Essa divisao permite que o mesmo `CharacterMotor2D` seja usado depois por inimigos, NPCs, cutscenes ou rede, sem depender de teclado ou gamepad.

## Proximo Passo Recomendado

Criar o prefab `Player` com:

- `SpriteRenderer`
- `Rigidbody2D`
- `CapsuleCollider2D` ou `BoxCollider2D`
- `PlayerInputReader`
- `CharacterMotor2D`
- `PlayerMovementController`

Depois disso, validamos movimento, colisao basica e camera antes de entrar em animacao.
