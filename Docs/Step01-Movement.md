# Step 01 - Movimento Top-Down

## Objetivo

Criar um jogador que se move em 2D, estilo RPG top-down, com responsabilidades separadas.

## Componentes

- `PlayerInputReader`: captura teclado, mouse/gamepad e expoe input bruto do jogador.
- `CharacterMotor2D`: aplica velocidade no `Rigidbody2D`.
- `PlayerMovementController`: liga input ao motor.
- `CharacterMovementSettings`: guarda a velocidade em um asset editavel.

## Como Montar no Unity

1. Crie um asset em `Assets/Game/ScriptableObjects`:
   - Botao direito no Project.
   - `Create > RPG Project > Character > Movement Settings`.
   - Nome sugerido: `PlayerMovementSettings`.
   - Comece com `Move Speed = 4`.

2. Crie um GameObject chamado `Player`.

3. Adicione estes componentes:
   - `SpriteRenderer`
   - `Rigidbody2D`
   - `BoxCollider2D`
   - `PlayerInputReader`
   - `CharacterMotor2D`
   - `PlayerMovementController`

4. No `Rigidbody2D`, confira:
   - `Gravity Scale = 0`
   - `Freeze Rotation Z = true`
   - `Interpolation = Interpolate`

5. No `CharacterMotor2D`, arraste o asset `PlayerMovementSettings` para o campo `Movement Settings`.

## Teste Manual

- Aperte Play.
- Use `WASD` ou setas para mover.
- O jogador deve andar na diagonal sem ganhar velocidade extra.
- Ao soltar as teclas, o jogador deve parar imediatamente.

## Por Que Esta Divisao Existe

O input nao sabe como mover o personagem. O motor nao sabe se a direcao veio de teclado, gamepad, IA ou rede. O controlador do jogador apenas conecta as duas pontas. Isso mantem o projeto flexivel para quando adicionarmos monstros, NPCs, pathfinding, combate e multiplayer.
