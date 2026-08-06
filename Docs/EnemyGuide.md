# Enemy System Guide

## 개요

적 시스템 구현 완료.

맵 배치 담당자는 생성된 프리팹을 씬에 배치한 후 Inspector 값만 조정하여 사용할 수 있습니다.

---

# 적 목록

## BeeEnemy (FlyEnemy)

비행 적

### 기능

- 일정 방향으로 지속 이동
- 방향에 따른 스프라이트 자동 반전

### Inspector

```text
Move Speed
Direction
```

### Direction

```text
1   왼쪽 이동
-1  오른쪽 이동
```

---

## SnailEnemy (CrawlEnemy)

달팽이 적

### 기능

- 일정 범위를 왕복 이동
- 방향 전환 시 스프라이트 반전

### Inspector

```text
Move Speed
Move Range
```

---

## FrogEnemy (JumpEnemy)

개구리 적

### 기능

- 일정 시간마다 점프
- 착지 시 Idle 상태
- GroundCheck 기반 바닥 판정

### Inspector

```text
Jump Power
Jump Delay
Move Power
Direction
Check Radius
Ground Check
```

### Direction

```text
1   왼쪽 점프
-1  오른쪽 점프
```

### 주의

GroundCheck 오브젝트가 반드시 필요함.

```text
FrogEnemy
└ GroundCheck
```

---

## BlockEnemy (FallingEnemy)

밟으면 떨어지는 적

### 기능

- Hit() 호출 시 낙하

### 주의

직접 떨어지지 않으므로 밟기 판정 구현 시

```csharp
enemy.Hit();
```

호출 필요

---

## RollingEnemy (SurfaceEnemy)

순찰 적

### 기능
- Bee와 동일
- 일정 방향으로 지속 이동
- 방향에 따른 스프라이트 자동 반전

### Inspector

```text
Move Speed
Direction
```

### Direction

```text
1   왼쪽 이동
-1  오른쪽 이동
```

---

## SpawnEnemy

땅속에서 등장하는 슬라임

### 기능

- 일정 높이까지 상승
- 대기
- 하강
- 반복

### Inspector

```text
Rise Height
Move Speed
Wait Time
```

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

---

# 참고

- 적 스탯 시스템 없음
- 공격 및 피격 처리 미구현
- 사망 처리 미구현
- 맵 배치 및 애니메이션 연결 가능 상태
- 행동 패턴 구현 완료