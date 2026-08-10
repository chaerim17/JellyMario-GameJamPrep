# Player System Guide

## 개요

프로젝트에서 사용하는 플레이어 시스템의 구성과 사용 방법을 설명합니다.

플레이어는 좌우 회전 이동, 점프, 젤리 충돌 반응, 사망 및 스테이지 클리어 기능을 지원합니다.

캐릭터 종류에 따라 사용하는 스프라이트는 다르지만 이동과 점프 기능은 동일합니다.

맵 배치 담당자는 씬에 `PlayerSpawner`를 배치한 후 Inspector에 생성 위치와 플레이어 프리팹을 등록하여 사용할 수 있습니다.

## 목차
1. [빠른 사용 방법](#빠른-사용-방법)
2. [캐릭터 목록](#캐릭터-목록)
3. [플레이어 기능](#플레이어-기능)
4. [조작 방법](#조작-방법)
5. [프리팹 구성](#프리팹-구성)
6. [Inspector 설정](#inspector-설정)
7. [플레이어 상태](#플레이어-상태)
8. [애니메이션](#애니메이션)
9. [충돌 및 Layer](#충돌-및-layer)
10. [캐릭터 선택과 생성](#캐릭터-선택과-생성)
11. [관련 클래스](#관련-클래스)
12. [PlayerBase](#playerbase)
13. [PlayerController](#playercontroller)
14. [파일 위치](#파일-위치)
15. [현재 구현 시 주의사항](#현재-구현-시-주의사항)

---

# 빠른 사용 방법

맵 배치 담당자가 플레이어를 씬에 추가하는 방법을 먼저 설명합니다.

## 씬 구성

```text
Playerpoint
└ PlayerSpawner
```

## 설정 순서
1. 빈 GameObject를 생성합니다.
2. 이름을 Playerpoint로 설정합니다.
3. 플레이어가 생성될 위치로 이동합니다.
4. PlayerSpawner를 추가합니다.
5. Player Point를 연결합니다.
6. Player Prefabs를 순서대로 등록합니다.
7. 씬에 InputManager가 있는지 확인합니다.

## 필수 확인 사항

- PlayerSpawner가 배치되어 있는가
- Player Point가 연결되어 있는가
- 모든 플레이어 프리팹이 등록되어 있는가
- InputManager가 존재하는가
- 플레이어 프리팹이 Player Layer를 사용하는가

---

# 캐릭터 목록

| 항목 | 설명 |
|------|------|
| PlayerBeige | 베이지색 플레이어 캐릭터 |
| PlayerGreen | 초록색 플레이어 캐릭터 |
| PlayerPink | 분홍색 플레이어 캐릭터 |
| PlayerPurple | 보라색 플레이어 캐릭터 |
| PlayerYellow | 노란색 플레이어 캐릭터 |

> **참고**
>
> - 모든 캐릭터는 같은 `PlayerController`를 사용합니다.
> - 캐릭터별 차이는 상태별 스프라이트입니다.
> - 이동 속도와 점프력은 각 프리팹의 Inspector에서 변경할 수 있습니다.

---

# 플레이어 기능

- 좌우 방향키를 이용한 회전 이동
- 위쪽 방향키를 이용한 점프
- 상태별 스프라이트 애니메이션
- 충돌 방향에 따른 젤리 변형
- 움직이는 젤리 표면 추종
- 위험 요소 충돌 시 사망
- 사망 후 현재 씬 재시작
- 골인 깃발 도착 시 다음 씬 이동
- 선택한 캐릭터 프리팹 생성

---

# 조작 방법

| 입력 | 동작 |
|------|------|
| `←` | 왼쪽 이동 |
| `→` | 오른쪽 이동 |
| `↑` | 점프 |

플레이어는 위치를 직접 변경하지 않고 `Rigidbody2D`의 moveSpeed를 이용하여 굴러갑니다.

---

# 프리팹 구성

## 오브젝트 구조

```text
Player
├── Rigidbody2D
├── CapsuleCollider2D
├── PlayerController
├── JellySurfaceFollower2D
├── JellyVisual
├── PixelShatterEffect
└── Visual
    └── SpriteRenderer
```

## 필수 컴포넌트

- `Rigidbody2D`
- `CapsuleCollider2D`
- `PlayerController`
- `JellySurfaceFollower2D`
- `SpriteRenderer`

## 선택 컴포넌트

- `JellyVisual`
- `PixelShatterEffect`

> **참고**
>
> - `JellyVisual`이 없어도 이동과 점프는 실행됩니다.
> - `PixelShatterEffect`가 없어도 사망 후 씬 재시작은 실행됩니다.
> - `SpriteRenderer`가 자식 오브젝트에 없으면 애니메이션이 표시되지 않습니다.

---

# Inspector 설정

## 애니메이션 설정

```text
Idle Frames
Idle Frame Time

Move Frames
Move Frame Time

Jump Frames
Jump Frame Time

Die Frames
Die Frame Time
```

| 항목 | 설명 |
|------|------|
| `Idle Frames` | 대기 상태에서 재생할 스프라이트 배열 |
| `Idle Frame Time` | 대기 애니메이션의 프레임 간격 |
| `Move Frames` | 이동 상태에서 재생할 스프라이트 배열 |
| `Move Frame Time` | 이동 애니메이션의 프레임 간격 |
| `Jump Frames` | 점프 상태에서 재생할 스프라이트 배열 |
| `Jump Frame Time` | 점프 애니메이션의 프레임 간격 |
| `Die Frames` | 사망 상태에서 재생할 스프라이트 배열 |
| `Die Frame Time` | 사망 애니메이션의 프레임 간격 |

## Move 설정

```text
Move Speed
Rotation Acceleration
```

| 항목 | 기본값 | 설명 |
|------|--------|------|
| `Move Speed` | 30 | 플레이어의 목표 회전 속도 |
| `Rotation Acceleration` | 720 | 현재 회전 속도가 목표 속도에 도달하는 정도 |

- `Move Speed`가 클수록 플레이어가 빠르게 굴러갑니다.
- `Rotation Acceleration`이 클수록 빠르게 회전하고 빠르게 멈춥니다.

## Jump 설정

```text
Jump Power
Jump Direction
```

| 항목 | 기본값 | 설명 |
|------|--------|------|
| `Jump Power` | 5 | 점프할 때 적용되는 속도 |
| `Jump Direction` | Player Transform | 점프 방향을 결정하는 Transform |

- `Jump Direction`이 비어 있으면 플레이어 자신의 위쪽 방향을 사용합니다.
- `Jump Direction`이 회전하면 실제 점프 방향도 변경됩니다.

## Jelly 설정

```text
Jelly Visual
Jelly Surface Follower
Jump Stretch
```

| 항목 | 기본값 | 설명 |
|------|--------|------|
| `Jelly Visual` | 자동 탐색 | 점프 및 충돌에 따른 젤리 변형 처리 |
| `Jelly Surface Follower` | 자동 탐색 | 움직이는 젤리 표면 추종 |
| `Jump Stretch` | 0.1 | 점프할 때 플레이어가 늘어나는 정도 |

## Die 설정

```text
Pixel Shatter Effect
Death Delay
```

| 항목 | 기본값 | 설명 |
|------|--------|------|
| `Pixel Shatter Effect` | 자동 탐색 | 사망 시 픽셀 분해 효과 |
| `Death Delay` | 0.8초 | 사망 후 현재 씬을 다시 불러오기까지 기다리는 시간 |

픽셀 분해 효과의 재생 시간이 `Death Delay`보다 길면 효과가 끝날 때까지 기다립니다.

## JellySurfaceFollower2D 설정

```text
Minimum Ground Normal Y
Cancel Ground Normal Velocity
```

| 항목 | 기본값 | 설명 |
|------|--------|------|
| `Minimum Ground Normal Y` | 0.5 | 착지로 인정할 최소 접촉 법선 Y 값 |
| `Cancel Ground Normal Velocity` | 활성화 | 표면 방향의 불필요한 속도를 제거할지 결정 |

## JellyVisual 설정

```text
Deformation Pivot
Visual
Stiffness
Damping
Max Deformation
Impact Response
Stretch Multiplier
Side Expansion
Visual Half Size
Anchor Strength
```

| 항목 | 기본값 | 설명 |
|------|--------|------|
| `Deformation Pivot` | 자동 생성 가능 | 젤리 변형의 중심 Transform |
| `Visual` | 직접 등록 | 실제로 변형할 스프라이트 Transform |
| `Stiffness` | 100 | 원래 모양으로 돌아가려는 힘 |
| `Damping` | 7 | 젤리 흔들림이 감소하는 정도 |
| `Max Deformation` | 0.5 | 적용할 수 있는 최대 변형량 |
| `Impact Response` | 0.045 | 충격 속도가 변형량에 미치는 정도 |
| `Stretch Multiplier` | 2 | 늘어나는 효과의 강조 정도 |
| `Side Expansion` | 1.15 | 눌릴 때 옆으로 퍼지는 정도 |
| `Visual Half Size` | (0.5, 0.5) | Visual 크기 계산에 사용하는 값 |
| `Anchor Strength` | 1 | 충돌 지점을 고정하는 정도 |

## PixelShatterEffect 설정

```text
Columns
Rows
Duration
Min Scatter Force
Max Scatter Force
Random Force
Upward Force
Gravity
Max Angular Speed
Fade Start
```

| 항목 | 기본값 | 설명 |
|------|--------|------|
| `Columns` | 8 | 스프라이트를 가로로 나눌 개수 |
| `Rows` | 8 | 스프라이트를 세로로 나눌 개수 |
| `Duration` | 0.75초 | 픽셀 분해 효과의 재생 시간 |
| `Min Scatter Force` | 1.5 | 조각에 적용할 최소 분산 힘 |
| `Max Scatter Force` | 4.5 | 조각에 적용할 최대 분산 힘 |
| `Random Force` | 1.2 | 조각마다 추가되는 무작위 힘 |
| `Upward Force` | 2 | 조각에 적용되는 위쪽 힘 |
| `Gravity` | 7 | 조각에 적용되는 중력 |
| `Max Angular Speed` | 720 | 조각의 최대 회전 속도 |
| `Fade Start` | 0.3초 | 조각이 투명해지기 시작하는 시간 |

---

# 플레이어 상태

`PlayerBase`에서 플레이어의 상태를 관리합니다.

## 상태 목록

| 상태 | 설명 |
|------|------|
| `Idle` | 이동 입력이 없는 대기 상태 |
| `Move` | 좌우 이동 입력이 있는 상태 |
| `Jump` | 플레이어가 점프한 상태 |
| `Die` | 플레이어가 사망한 상태 |

## 상태 전환

```text
Idle ── 좌우 입력 ──> Move
Move ── 입력 해제 ──> Idle

Idle 또는 Move ── 점프 입력 ──> Jump
Jump ── 착지 또는 애니메이션 종료 ──> Idle 또는 Move

모든 상태 ── 사망 판정 ──> Die
```

---

# 애니메이션

`PlayerBase`에서 상태별 애니메이션을 관리합니다.

Animator Controller를 사용하지 않고 등록된 스프라이트 배열을 순서대로 재생합니다.

## 애니메이션 반복

| 상태 | 재생 방식 |
|------|-----------|
| `Idle` | 반복 재생 |
| `Move` | 반복 재생 |
| `Jump` | 한 번 재생 |
| `Die` | 한 번 재생 |

## PlayerBeige 예시

```text
Idle Frames
  character_beige_idle

Move Frames
  character_beige_walk_a
  character_beige_walk_b

Jump Frames
  character_beige_jump

Die Frames
  character_beige_hit
```

> **참고**
>
> - 스프라이트 배열이 비어 있으면 경고 메시지가 출력됩니다.
> - 배열 안에 비어 있는 스프라이트가 있으면 해당 프레임을 건너뜁니다.
> - 캐릭터별로 같은 상태에 해당하는 스프라이트를 등록합니다.

---

# 충돌 및 Layer

## 착지

다음 대상과 플레이어 아래쪽이 충돌하면 착지로 처리합니다.

- `Ground` Layer
- `JellySurfaceWave`가 적용된 표면
- 접촉 법선의 Y 값이 `0.5`보다 큰 충돌

착지하면 표면 추종을 활성화하고 입력에 따라 `Idle` 또는 `Move` 상태로 변경합니다.

## 사망

다음 Layer와 충돌하거나 Trigger에 진입하면 사망합니다.

- `Monster`
- `Hazard`
- `DeathZone`

보스와 투사체는 각 공격 스크립트에서 플레이어의 `Die()`를 직접 호출합니다.

- `Boss`
- `Fireball`
- `Missile`

## 사망 처리 과정

```text
Die 상태 전환
→ 플레이어 입력 차단
→ Rigidbody2D 물리 정지
→ Collider 비활성화
→ 사망 애니메이션 재생
→ 픽셀 분해 효과 실행
→ 현재 씬 다시 로드
```

## 스테이지 클리어

`Goal Flag` Layer의 Trigger에 진입하면 다음 씬으로 이동합니다.

```text
Goal Flag 진입
→ 다음 Build Index 확인
→ 다음 씬으로 이동
```

> **주의**
>
> - 실제 Layer 이름은 공백이 포함된 `Goal Flag`입니다.
> - `GoalFlag`를 사용하면 클리어 판정이 작동하지 않습니다.
> - `MainMenu` 씬에서는 클리어를 처리하지 않습니다.

---

# 캐릭터 선택과 생성

선택한 캐릭터 인덱스는 다음 값으로 저장합니다.

```text
PlayerPrefs["SelectedCharacter"]
```

## 프리팹 배열 순서

| 인덱스 | 프리팹 |
|--------|--------|
| 0 | `PlayerYellow` |
| 1 | `PlayerBeige` |
| 2 | `PlayerGreen` |
| 3 | `PlayerPink` |
| 4 | `PlayerPurple` |

`LoginUI`의 캐릭터 버튼 순서와 `PlayerSpawner`의 프리팹 배열 순서는 같아야 합니다.

## PlayerSpawner Inspector

```text
Player Point
Player Prefabs
```

### Player Point

플레이어를 생성할 위치와 회전값입니다.

등록하지 않으면 `PlayerSpawner` 오브젝트의 위치를 사용합니다.

### Player Prefabs

선택할 수 있는 플레이어 프리팹 배열입니다.

```text
PlayerYellow
PlayerBeige
PlayerGreen
PlayerPink
PlayerPurple
```

---

# 관련 클래스

| 클래스 | 설명 |
|--------|------|
| `PlayerBase` | 플레이어 상태와 애니메이션 관리 |
| `PlayerController` | 입력, 이동, 점프, 충돌 및 사망 처리 |
| `PlayerSpawner` | 선택한 플레이어 프리팹 생성 |
| `PlayerManager` | 현재 플레이어 참조 관리 |
| `InputManager` | 키보드 입력 제공 |
| `JellyVisual` | 젤리 변형 처리 |
| `JellySurfaceFollower2D` | 젤리 표면 추종 |
| `PixelShatterEffect` | 사망 시 픽셀 분해 효과 처리 |

---

# PlayerBase

`PlayerBase`는 플레이어의 상태와 애니메이션을 관리하는 부모 클래스입니다.

## 주요 변수

| 변수 | 설명 |
|------|------|
| `idleFrames` | 대기 애니메이션 스프라이트 배열 |
| `moveFrames` | 이동 애니메이션 스프라이트 배열 |
| `jumpFrames` | 점프 애니메이션 스프라이트 배열 |
| `dieFrames` | 사망 애니메이션 스프라이트 배열 |
| `CurrentState` | 플레이어의 현재 상태 |
| `_spriteRenderer` | 스프라이트를 출력하는 컴포넌트 |
| `_animationCoroutine` | 현재 실행 중인 애니메이션 코루틴 |

## 주요 함수

| 함수 | 설명 |
|------|------|
| `Initialize()` | 플레이어를 초기화합니다. |
| `HandleInput()` | 입력 처리를 위한 함수입니다. |
| `HandleMovement()` | 이동 처리를 위한 함수입니다. |
| `ChangeState()` | 플레이어 상태를 변경합니다. |
| `Idle()` | `Idle` 상태로 변경합니다. |
| `Move()` | `Move` 상태로 변경합니다. |
| `Jump()` | `Jump` 상태로 변경합니다. |
| `Die()` | `Die` 상태로 변경합니다. |
| `SetAnimation()` | 상태에 맞는 애니메이션을 선택합니다. |
| `PlayAnimation()` | 스프라이트 애니메이션을 재생합니다. |
| `OnAnimationFinished()` | 애니메이션 종료 후 처리를 실행합니다. |

---

# PlayerController

`PlayerController`는 입력, 이동, 점프, 충돌, 사망 및 스테이지 클리어를 처리합니다.

## 주요 변수

| 변수 | 설명 |
|------|------|
| `moveSpeed` | 목표 회전 속도 |
| `rotationAcceleration` | 회전 가속도 |
| `jumpPower` | 점프 속도 |
| `jumpDirection` | 점프 방향 기준 |
| `jellyVisual` | 젤리 변형 컴포넌트 |
| `jellySurfaceFollower` | 젤리 표면 추종 컴포넌트 |
| `jumpStretch` | 점프 변형 정도 |
| `pixelShatterEffect` | 픽셀 분해 효과 |
| `deathDelay` | 사망 후 씬 재시작 대기시간 |
| `_rigidbody` | 플레이어의 물리 컴포넌트 |
| `_collider` | 플레이어의 충돌 컴포넌트 |
| `_moveInput` | 현재 이동 입력 |
| `_isDead` | 플레이어 사망 여부 |

## 주요 함수

| 함수 | 설명 |
|------|------|
| `Initialize()` | 필요한 컴포넌트를 탐색하고 등록합니다. |
| `HandleInput()` | 이동 및 점프 입력을 처리합니다. |
| `HandleMovement()` | 현재 입력에 따라 이동 상태를 처리합니다. |
| `Move()` | 이동 상태와 회전 이동을 적용합니다. |
| `UpdateRotation()` | 플레이어의 각속도를 변경합니다. |
| `Jump()` | 점프 속도와 젤리 효과를 적용합니다. |
| `OnCollisionEnter2D()` | 일반 충돌, 착지 및 사망을 처리합니다. |
| `IsDeathLayer()` | 사망 판정 Layer인지 확인합니다. |
| `OnTriggerEnter2D()` | Trigger 사망 및 클리어를 처리합니다. |
| `Die()` | 플레이어 사망 처리를 실행합니다. |
| `PlayDeathSequence()` | 사망 효과 후 현재 씬을 다시 불러옵니다. |
| `StageClear()` | 다음 Build Index의 씬으로 이동합니다. |

---

# 파일 위치

## 프리팹 위치

```text
Assets
└ Prefabs
    └ Player
        ├ Player
        ├ PlayerBeige
        ├ PlayerGreen
        ├ PlayerPink
        ├ PlayerPurple
        └ PlayerYellow
```

## 스크립트 위치

```text
Assets
└ Scripts
    ├ Player
    │   ├ PlayerBase.cs
    │   ├ PlayerController.cs
    │   └ PlayerSpawner.cs
    └ Managers
        ├ PlayerManager.cs
        └ InputManager.cs
```

---

# 구현 시 주의사항

- 공중에서도 다시 점프할 수 있습니다.
- 사망하면 체크포인트가 아닌 씬 전체를 다시 시작합니다.
- 스테이지 순서는 Build Settings의 씬 순서에 의존합니다.
- `Default` Layer는 일반적인 착지 대상으로 처리하지 않습니다.
- 골인 깃발은 정확히 `Goal Flag` Layer를 사용해야 합니다.
- 캐릭터 선택 순서와 프리팹 배열 순서가 다르면 다른 캐릭터가 생성됩니다.
- `SpriteRenderer`가 자식 오브젝트에 없으면 애니메이션이 표시되지 않습니다.