# Enemy System Guide

## 개요

적 시스템 구현 완료.

맵 배치 담당자는 생성된 프리팹을 씬에 배치한 후 Inspector 값만 조정하여 사용할 수 있습니다.



---

# 적 목록
| 프리팹 | 클래스 | 설명 |
|--------|--------|------|
| [`BeeEnemy`](#beeenemy-flyenemy) | `FlyEnemy` | 좌우로 비행하는 적 |
| [`SnailEnemy`](#snailenemy-crawlenemy) | `CrawlEnemy` | 지형을 따라 이동하는 달팽이 적 |
| [`FrogEnemy`](#frogenemy-jumpenemy) | `JumpEnemy` | 일정 시간마다 점프하는 적 |
| [`BlockEnemy`](#blockenemy-fallingenemy) | `FallingEnemy` | 플레이어가 밟으면 떨어지는 블록 |
| [`RollingEnemy`](#rollingenemy-surfaceenemy) | `SurfaceEnemy` | 지정된 경로를 순환하는 적 |
| [`SpawnEnemy`](#spawnenemy) | `SpawnEnemy` | 지형 아래에서 나타났다 사라지는 적 |
| [`BossSlime`](#bossslime) | `BossSlimeController` | 벽에 반사되며 이동하는 보스 소환 슬라임 |

---

# 공동 시스템

일반 Enemy는 `EnemyBase`를 상속받아 상태와 애니메이션을 관리합니다.

## 공통 기능
- Idle, Move, Die 상태 관리
- 상태에 맞는 Sprite 애니메이션 재생
- 다른 Enemy와의 충돌 무시
- 비활성화 시 애니메이션 정지
- 다시 활성화될 때 애니메이션 재시작

## 공통 Inspector 설정
| 항목 | 기본값 | 설명 |
|------|--------|------|
| `Idle Frames` | 프리팹별 설정 | 대기 상태에서 재생할 Sprite 목록 |
| `Idle Frame Time` | 0.5초 | Idle 애니메이션의 프레임 간격 |
| `Move Frames` | 프리팹별 설정 | 이동 상태에서 재생할 Sprite 목록 |
| `Move Frame Time` | 0.5초 | Move 애니메이션의 프레임 간격 |
| `Die Frames` | 프리팹별 설정 | 사망 상태에서 재생할 Sprite 목록 |
| `Die Frame Time` | 0.5초 | Die 애니메이션의 프레임 간격 |

> **참고**
>
> `EnemyBase.Die()`는 상태와 애니메이션만 변경합니다.
> 일반 Enemy를 제거하는 별도의 사망 로직은 현재 구현되어 있지 않습니다.

---

## BeeEnemy (FlyEnemy)

비행 적

### 기능
- 좌우 왕복 이동
- 지형과 충도돌 시 이동 방향 전환
- Jelly 지형에 충돌 파동 생성
- 방향에 따른 스프라이트 자동 반전

### Inspector
| 항목 | 현재값 | 설명 |
|------|--------|------|
| `Move Speed` | 2 | 비행 이동 속도 |
| `Direction` | -1 | 초기 이동 방향 |

### Direction

```text
1   왼쪽 이동
-1  오른쪽 이동
```
현재 프리팹은 오른쪽으로 이동을 시작합니다.

---

## SnailEnemy (CrawlEnemy)

달팽이 적

### 기능

- 일정 범위를 왕복 이동
- Transform 기반 Move Point로 경로 관리
- Jelly 지형의 경사면을 따라 이동 및 회전
- Jelly 지형에 충돌 파동 생성
- 방향 전환 시 스프라이트 반전

### Inspector
| 항목 | 현재값 | 설명 |
|------|--------|------|
| `Move Points` | 비어 있음 | 이동에 사용할 지점 목록 |
| `Move Speed` | 2 | 이동 속도 |
| `Move Range` | 5 | 시작 위치를 기준으로 이동할 좌우 거리 |
| `Point Arrival Distance` | 0.1 | 이동 지점에 도착했다고 판단할 거리 |
| `Slope Rotation Speed` | 360 | 경사면에 맞춰 회전하는 속도 |
| `Ground Probe Distance` | 1 | 아래쪽 지형을 탐색하는 거리 |

> **참고**
>
> 경사면 추적 기능을 사용하려면 지형에 `JellySurfaceWave`가 있어야 합니다.

---

## FrogEnemy (JumpEnemy)

개구리 적

### 기능

- 일정 시간마다 점프
- X축 기반 MovePoint로 경로 관리
- `JellySurfaceFollower2D`를 이용하여 Jelly 지형 착지 판정
- Jelly 지형에 충돌 파동 생성
- GroundCheck 기반 바닥 판정

### Inspector
| 항목 | 현재값 | 설명 |
|------|--------|------|
| `Jump Power` | 11 | 수직 점프 힘 |
| `Jump Delay` | 0초 | 착지 후 다음 점프까지 기다리는 시간 |
| `Move Point Xs` | 비어 있음 | 부모 기준 점프 목표 X 좌표 목록 |
| `Point Arrival Distance` | 0.35 | 목표 지점 도착 판정 거리 |
| `Move Power` | -3 | 수평 점프 힘 |
| `Direction` | 1 | 점프 방향 계산에 사용하는 값 |

### Direction

```text
1   왼쪽 점프
-1  오른쪽 점프
```
이동 지점이 없을 때 수평 점프 힘은 다음과 같이 계산합니다.
```text
수평 점프 힘 = Move Power × Direction
```

## 필수 구성
- `Rigidbody2D`
- `Collider2D`
- `JumpEnemy`
- `JellySurfaceFollower2D`

> **참고**
>
> 프리팹에 `GroundCheck` 오브젝트가 남아 있지만 현재 `JumpEnemy`에서는 사용하지 않습니다.
> 착지 판정은 `JellySurfaceFollower2D`와 충돌 정보를 이용합니다.

---

## BlockEnemy (FallingEnemy)

낙하 블록

## 기능
- 플레이어가 블록 위에 올라오면 자동 활성화
- 설정된 시간만큼 기다린 후 아래로 낙하
- 일정한 속도로 계속 아래쪽으로 이동
- 활성화될 때 Move 상태로 변경

###  Inspector 설정
| 항목 | 현재값 | 설명 |
|------|--------|------|
| `Fall Speed` | 3 | 블록이 떨어지는 속도 |
| `Fall Delay` | 0.2초 | 플레이어가 밟은 후 낙하하기까지의 시간 |

## 활성화 조건
```text
PlayerController가 있는 오브젝트와 충돌
→ 플레이어가 BlockEnemy보다 위에 있음
→ 블록 윗면과 접촉
→ Fall Delay 후 낙하
```

> **참고**
>
> 현재 `BlockEnemy` 프리팹의 Root Layer는 `Default`입니다.

---

## RollingEnemy (SurfaceEnemy)

순찰 적

### 기능
- Transform축 기반 MovePoint로 경로 관리
- 방향에 따른 스프라이트 자동 반전
- Jelly 지형 Collider와의 물리 충돌 무시

### Inspector
| 항목 | 현재값 | 설명 |
|------|--------|------|
| `Move Points` | 미등록 | 이동할 지점 목록 |
| `Move Speed` | 3 | 지점 사이를 이동하는 속도 |
| `Direction` | 1 | 이동 지점을 순회하는 방향 |
| `Arrival Distance` | 0.02 | 지점 도착 판정 거리 |

### Direction

```text
1   Move Points를 앞에서부터 순회
-1  Move Points를 뒤에서부터 순회
```

## 이동 지점 구성
```text
RollingEnemy
├ Visual
├ Point1
├ Point2
└ Point3
```
`Move Points`에 지점을 직접 등록할 수 있습니다.
등록된 지점이 없으면 이름이 `Point`로 시작하는 자식 오브젝트를 자동으로 탐색합니다.

> **주의**
>
> 이동 지점이 하나도 없으면 RollingEnemy는 이동하지 않습니다.

---

## SpawnEnemy

땅속에서 등장하는 슬라임

### 기능

- 배치된 위치를 숨김 위치로 사용
- 일정 시간 대기
- 설정된 높이까지 상승
- 다시 대기한 후 원래 위치로 하강
- 상승과 하강 반복

### Inspector
| 항목 | 현재값 | 설명 |
|------|--------|------|
| `Rise Height` | 0.9 | 숨김 위치에서 상승할 높이 |
| `Move Speed` | 1 | 상승 및 하강 속도 |
| `Wait Time` | 2초 | 상승 또는 하강 후 기다리는 시간 |


## 동작 순서
```text
숨김 위치에서 대기
→ 상승
→ 표시 위치에서 대기
→ 하강
→ 반복
```

> **참고**
>
> 현재 Tutorial과 Hard Scene의 일부 SpawnEnemy는 Inspector Override를 통해 다른 값을 사용합니다.

---

## BossSlimeEnemy

보스 패턴용 슬라임

### 기능

- 포물선으로 발사
- 착지 후 이동

### Inspector

```text
Move Speed
Direction
Launch Force
```

### Direction

```text
1   왼쪽 이동
-1  오른쪽 이동
```

### Launch Force

```text
X  수평 발사 속도
Y  수직 발사 속도
```

### 예시

```text
(-3, 6)
```

왼쪽 포물선

```text
(3, 6)
```

오른쪽 포물선

---

# 애니메이션

EnemyBase에서 관리

### 등록 가능 항목

```text
Idle Frames
Move Frames
Hit Frames
Die Frames
```

### 예시

```text
Idle Frames
  frog_idle_0

Move Frames
  frog_jump_0
```

---

# 프리팹 위치

```text
Assets
└ Prefabs
    └ Enemy
```

생성된 프리팹

```text
BeeEnemy
SnailEnemy
FrogEnemy
BlockEnemy
RollingEnemy
SpawnEnemy
MiniSlime
```