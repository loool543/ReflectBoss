Reflect Boss
Unity로 제작한 3D One-Room Boss Battle 프로젝트입니다.

보스가 발사하는 투사체 중 BasketBall을 정확한 타이밍에 반사하여 보스에게 되돌려 보내는 것이 핵심 전투 메커니즘입니다.

단순히 공격 버튼을 눌러 보스를 쓰러뜨리는 방식이 아니라,
보스의 공격을 이용해 다시 보스를 공격하는 Reflect 중심의 전투를 구현했습니다.

🎮 Gameplay
Stage 1
Boss가 일정한 간격으로 Projectile을 발사합니다.
Ball과 BasketBall이 랜덤하게 등장합니다.
일반 Ball은 방어할 수 없으며 Player에게 Damage를 줍니다.
BasketBall은 Player가 Block 상태일 때 Reflect할 수 있습니다.
Player가 Reflect한 BasketBall만 Boss에게 Damage를 줄 수 있습니다.
Stage 2
Boss HP가 50% 이하가 되면 Stage 2로 전환됩니다.

Boss가 한 번에 3개의 Projectile을 발사합니다.
중앙은 Player 방향으로 발사됩니다.
나머지 두 Projectile은 좌우 약 30도 방향으로 발사됩니다.
각 Projectile은 Ball / BasketBall 중 독립적으로 랜덤 선택됩니다.
Projectile이 Env Layer의 벽과 충돌하면 제거되지 않고 반사됩니다.
벽의 충돌 Normal을 이용하여 입사 방향에 따라 자연스럽게 반사됩니다.
Stage 2 Projectile은 Player 또는 Boss와 충돌하기 전까지 Arena에 유지됩니다.
벽에 의해 튕겨진 BasketBall과
Player가 직접 Reflect한 BasketBall을 별도의 상태로 구분하여,

Player가 직접 Reflect한 BasketBall에 맞았을 때만 Boss HP가 감소합니다.

🕹 Controls
Input	Action
WASD	Move
Shift	Run
Space	Jump
Right Mouse Button	Block / Reflect
⚔ Combat Flow
Boss
 ↓
Projectile Fire
 ↓
Ball / BasketBall
 ↓
Player
 ├─ Ball → Damage
 │
 └─ BasketBall
      ├─ Block 실패 → Damage
      │
      └─ Block 성공 → Reflect
                       ↓
                      Boss
                       ↓
                    Damage
