# Jelly Physics

## 개요

Jelly시스템의 구성과 연결 구조를 설명하는 문서입니다.
지형에는 `JellyGround`Meterial과 `JellySurfaceWave`를 적용하여 Inspector 값을 조정하여 충돌 파도를 생성합니다.
캐릭터에는 `JellyVisual`과 `JellySurfaceFollower2D`를 적용하여 외형 변경과 표면 추종 기능을 사용합니다.

## 목차
1. [JellySurfaceWave](#jellysurfacewave)
   - [Inspector 설정](#jellysurfacewave-inspector-설정)
   - [관련 클래스](#jellysurfacewave-관련-클래스)
   - [구현 기능](#jellysurfacewave-구현-기능)
2. [JellyVisual](#jellyvisual)
   - [Inspector 설정](#jellyvisual-inspector-설정)
   - [관련 클래스](#jellyvisual-관련-클래스)
   - [구현 기능](#jellyvisual-구현-기능)
3. [JellySurfaceFollower2D](#jellysurfacefollower2d)
   - [Inspector 설정](#jellysurfacefollower2d-inspector-설정)
   - [관련 클래스](#jellysurfacefollower2d-관련-클래스)
   - [구현 기능](#jellysurfacefollower2d-구현-기능)
4. [Material과 Shader](#material과-shader)
5. [오브젝트 구성](#오브젝트-구성)
6. [파일 위치](#파일-위치)

---

# JellySurfaceWave

`JellySurfaceWave`는 지형의 충돌을 감지하고 화면과 Collider에 파동을 생성하는 클래스입니다.
하나의 Renderer에서 최대 4개의 파동을 동시에 관리할 수 있습니다.

---

## JellySurfaceWave Inspector 설정

### 참조 설정
| 항목 | 기본값 | 설명 |
|------|------|------|
| `Surface Renderer` | 자동 탐색 | JellyGround Shader가 적용된 Renderer |
| `Reacting Layers` | Everything | 충돌 파동을 발생시킬 Layer |
| `Surface Tilemap` | 자동 탐색 | Collider 윤곽을 생성할 때 사용하는 Tilemap |
| `Tilemap Collider` | 자동 탐색 | 파동 Collider의 원본이 되는 TilemapCollider2D |

### 충돌 설정
| 항목 | 기본값 | 설명 |
|------|------|------|
| `Minimum Impact Speed` | 0.8 | 파동을 생성하기 위한 최소 충돌 속도 |
| `Impact Response` | 0.08 | 충돌 속도를 파동 세기로 변환하는 비율 |
| `Max Impact Strength` | 0.5 | 하나의 충돌에 적용할 최대 파동 세기 |
| `Impact Cooldown` | 0.4초 | 같은 오브젝트의 중복 충돌을 제한하는 시간 |
| `Visual Wave Duration` | 3초 | 하나의 파동이 유지되는 시간 |
| `Concurrent Wave Limit` | 4 | 동시에 유지할 수 있는 파동 개수 |
| `Minimum Contact Separation` | 0.75 | 서로 다른 충돌 지점으로 판단하는 최소 거리 |

### 공통 파동 설정
| 항목 | 기본값 | 설명 |
|------|------|------|
| `Impact Frequency` | 1.2 | 파동이 반복되는 촘촘한 정도 |
| `Impact Speed` | 1.5 | 파동이 시간에 따라 진행되는 속도 |
| `Impact Falloff` | 0.25 | 거리에 따라 파동이 약해지는 정도 |
| `Impact Decay` | 0.6 | 시간에 따라 파동이 약해지는 정도 |
| `Visual Wave Height` | 1.6 | 화면과 Collider에 적용되는 파동 높이 |
| `Max Combined Visual Offset` | 1.25 | 여러 파동이 겹칠 때 적용할 최대 이동 거리 |

### 파동 Collider 설정
| 항목 | 기본값 | 설명 |
|------|------|------|
| `Animate Tile Collider` | 활성화 | 화면의 파동에 맞춰 Collider를 변형 |
| `Maximum Collider Point Spacing` | 0.5 | Collider 선분을 나누는 최대 간격 |
| `Surface Tilemap` | 자동 탐색 | Tilemap 데이터를 읽을 때 사용 |
| `Tilemap Collider` | 자동 탐색 | 원본 Collider 윤곽을 생성할 때 사용 |

> **성능 참고**
>
> `Maximum Collider Point Spacing`이 작을수록 파동을 정확히 따라가지만 Collider 점과 물리 연산량이 증가합니다.

---

## JellySurfaceWave 관련 클래스

| 클래스 및 컴포넌트 | 설명 |
|--------------------|------|
| `Tilemap` | 젤리 지형의 타일 정보를 저장 |
| `TilemapRenderer` | JellyGround Material을 이용하여 지형 출력 |
| `TilemapCollider2D` | 지형의 원본 충돌 영역 제공 |
| `CompositeCollider2D` | 여러 Tile Collider를 하나의 윤곽으로 결합 |
| `Rigidbody2D` | CompositeCollider2D에 필요한 물리 컴포넌트 |
| `EdgeCollider2D` | 실행 중 화면 파동을 따라가는 Collider로 생성 |
| `JellySurfaceFollower2D` | JellySurfaceWave의 표면 이동량을 이용하여 캐릭터 이동 |
| `JellyGround` | 지형에 파동을 출력하는 Material |
| `JellyGround.shader` | 충돌 위치와 파동 이동량을 화면에 적용 |

---

## JellySurfaceWave 구현 기능

### 주요 함수
| 함수 | 설명 |
|------|------|
| `Awake()` | Renderer와 Tilemap 참조를 초기화하고 파동 Collider를 준비 |
| `OnEnable()` | Shader와 MaterialPropertyBlock 설정을 다시 적용 |
| `FixedUpdate()` | 종료된 파동을 정리하고 Collider 위치를 갱신 |
| `OnCollisionEnter2D()` | 충돌 조건을 확인하고 새로운 파동 생성 |
| `PlayRipple()` | 지정한 위치와 방향에 파동 생성 |
| `GetSurfaceDeltaAtWorldPoint()` | 특정 위치에서 표면이 움직인 양을 반환 |
| `FindVisualWaveSlot()` | 새로운 파동에 사용할 슬롯을 선택 |
| `UpdateVisualWaves()` | 유지 시간이 끝난 파동을 제거 |
| `InitializeRuntimeWaveCollider()` | 변형 가능한 Runtime Collider 생성 |
| `UpdateRuntimeWaveCollider()` | 화면 파동에 맞춰 Collider 점을 이동 |
| `ApplyWaveSettings()` | Inspector 값을 Shader에 전달 |
| `OnDisable()` | 파동과 Runtime Collider를 제거하고 원본 Collider 복구 |

### 충돌 파동 생성

```text
오브젝트 충돌
→ Reacting Layers 확인
→ 충돌 속도 확인
→ Impact Cooldown 확인
→ 접촉점 거리 확인
→ 파동 슬롯 선택
→ Shader에 충돌 정보 전달
→ 화면과 Collider에 파동 적용
```
파동 세기는 다음과 같이 계산합니다.
```text
파동 세기
= 충돌 속도 × Impact Response
```
계산된 값은 `Max Impact Strength`를 넘지 않도록 제한합니다.

### 파동 슬롯

- 최대 4개의 파동을 동시에 표시합니다.
- 모든 슬롯이 사용 중이면 가장 오래된 파동을 교체합니다.
- 파동은 `Visual Wave Duration` 동안 유지됩니다.
- 유지 시간의 마지막 25% 구간에서 자연스럽게 사라집니다.

### 파동 Collider

화면에 표시되는 파동과 실제 충돌 영역을 일치시키기 위해 실행 중 `EdgeCollider2D`를 생성합니다.
```text
TilemapCollider2D 윤곽 확인
→ Collider 선분 분할
→ Runtime EdgeCollider2D 생성
→ 화면 파동 이동량 계산
→ Collider 점에 같은 이동량 적용
```
파동 Collider를 생성하지 못하면 화면 파동만 사용하고 원본 Collider를 유지합니다.

---

# JellyVisual

`JellyVisual`은 Rigidbody2D 오브젝트의 외형을 충돌 방향으로 압축하거나 점프 방향으로 늘리는 클래스입니다.

---

## JellyVisual Inspector 설정

### 참조 설정
| 항목 | 기본값 | 설명 |
|------|------|------|
| `Deformation Pivot` | 자동 생성 | 충돌 방향으로 회전하고 Scale 변형을 적용할 Transform |
| `Visual` | 직접 등록 | 실제로 변형할 SpriteRenderer가 있는 Transform |

### 스프링 설정
| 항목 | 기본값 | 설명 |
|------|------|------|
| `Stiffness` | 100 | 변형된 외형이 원래 형태로 돌아가려는 힘 |
| `Damping` | 7 | 반복되는 스프링 움직임이 줄어드는 정도 |\

### 변형 설정
| 항목 | 기본값 | 설명 |
|------|------|------|
| `Max Deformation` | 0.5 | 적용할 수 있는 최대 변형량 |
| `Impact Response` | 0.045 | 충돌 힘을 변형량으로 변환하는 비율 |
| `Stretch Multiplier` | 2 | 압축 후 반동으로 늘어나는 배율 |
| `Side Expansion` | 1.15 | 눌릴 때 양옆으로 퍼지는 정도 |
| `Visual Half Size` | (0.5, 0.5) | 접촉면 고정 계산에 사용할 Visual 크기의 절반 |
| `Anchor Strength` | 1 | 압축될 때 접촉 지점을 고정하는 정도 |

> **Player 프리팹 참고**
>
> 현재 기본 Player 프리팹의 `Side Expansion`은 `1.5`로 설정되어 있습니다.

---

## JellyVisual 관련 클래스

| 클래스 및 컴포넌트 | 설명 |
|--------------------|------|
| `Rigidbody2D` | JellyVisual 사용에 필요한 물리 컴포넌트 |
| `Transform` | Pivot과 Visual의 위치, 회전 및 크기 변경 |
| `SpriteRenderer` | 실제 변형되어 화면에 표시되는 Sprite |
| `PlayerController` | 점프와 충돌 시 JellyVisual 함수 호출 |

---

## JellyVisual 구현 기능

### 주요 함수
| 함수 | 설명 |
|------|------|
| `Awake()` | Visual과 DeformationPivot 초기화 |
| `LateUpdate()` | 스프링 값을 갱신하고 외형 변형 적용 |
| `Initialize()` | 참조를 확인하고 원래 위치, 크기 및 회전 저장 |
| `CreateDeformationPivotIfNeeded()` | Pivot이 없으면 자동으로 생성 |
| `UpdateSpring()` | 변형된 외형을 원래 형태로 복구 |
| `ApplyDeformation()` | Pivot의 크기, 위치 및 회전을 변경 |
| `ReactToImpact()` | 충돌 방향과 힘을 이용하여 압축 변형 추가 |
| `Stretch()` | 캐릭터의 위쪽 방향으로 늘어나는 변형 추가 |
| `OnDisable()` | Visual과 Pivot을 원래 상태로 복구 |

### 충돌 변형

충돌 힘을 이용하여 변형량을 계산합니다.
```text
변형량
= 충돌 힘 × Impact Response
```
계산된 변형량은 `Max Deformation` 범위로 제한합니다.\
```text
캐릭터 충돌
→ 충돌 방향을 로컬 방향으로 변환
→ 변형량 계산
→ 충돌 방향으로 압축
→ 수직 방향으로 확대
→ 스프링을 이용하여 원래 형태로 복구
```

### 점프 변형

Player가 점프하면 `Stretch()`를 호출합니다.
```text
점프
→ Stretch(Jump Stretch)
→ 캐릭터 위쪽 방향으로 늘어남
→ 스프링 반동
→ 원래 형태로 복구
```

### DeformationPivot 자동 생성

```text
Deformation Pivot 확인
→ 비어 있으면 Visual의 부모 확인
→ DeformationPivot 생성
→ Visual을 DeformationPivot의 자식으로 이동
```

---

# JellySurfaceFollower2D

`JellySurfaceFollower2D`는 Rigidbody2D 오브젝트가 움직이는 젤리 표면을 따라가게 하는 클래스입니다.

---

## JellySurfaceFollower2D Inspector 설정

| 항목 | 기본값 | 설명 |
|------|------|------|
| `Minimum Ground Normal Y` | 0.5 | 파동 바닥으로 인정할 최소 접촉 법선 Y 값 |
| `Cancel Ground Normal Velocity` | 활성화 | 바닥 법선 방향의 상대 속도를 제거할지 설정 |

---

## JellySurfaceFollower2D 관련 클래스

| 클래스 및 컴포넌트 | 설명 |
|--------------------|------|
| `Rigidbody2D` | 계산된 표면 이동량을 캐릭터 위치에 적용 |
| `Collider2D` | 젤리 표면과의 물리 접촉 감지 |
| `Collision2D` | 표면 접촉점과 접촉 법선 정보 제공 |
| `JellySurfaceWave` | 현재 표면의 이동량 제공 |
| `PlayerController` | 점프, 착지 및 사망 시 표면 추종 제어 |
| `JumpEnemy` | 착지 시 JellySurfaceFollower2D를 이용하여 표면 추종 |

### 클래스 관계
```text
PlayerController 또는 JumpEnemy
└ JellySurfaceFollower2D
    ├ Rigidbody2D
    ├ Collider2D
    └ JellySurfaceWave
```

---

## JellySurfaceFollower2D 구현 기능

### 공개 상태
| 항목 | 설명 |
|------|------|
| `IsFollowingSurface` | 현재 파동 표면을 추종하고 있는지 반환 |
| `CurrentSurface` | 현재 추종하고 있는 JellySurfaceWave 반환 |

### 주요 함수
| 함수 | 설명 |
|------|------|
| `Awake()` | Rigidbody2D 참조 저장 |
| `FixedUpdate()` | 표면 이동량을 Rigidbody2D 위치에 적용 |
| `IsSurfaceCollision()` | 충돌 대상에 JellySurfaceWave가 있는지 확인 |
| `OnCollisionEnter2D()` | 표면과 처음 충돌했을 때 접촉 정보 저장 |
| `OnCollisionStay2D()` | 표면과 접촉하는 동안 접촉 정보 갱신 |
| `OnCollisionExit2D()` | 현재 표면에서 벗어나면 추종 정보 제거 |
| `SetFollowingEnabled()` | 표면 추종 기능 활성화 또는 비활성화 |
| `TryStartFollowing()` | 충돌 정보를 이용하여 표면 추종 시작 |
| `ClearSurface()` | 현재 표면과 접촉 정보 초기화 |
| `UpdateSurfaceContact()` | 가장 위쪽을 향하는 접촉점 저장 |
| `RemoveGroundNormalVelocity()` | 표면 법선 방향의 불필요한 속도 제거 |
| `OnDisable()` | 현재 추종 중인 표면 정보 제거 |

### 표면 추종

```text
젤리 표면과 충돌
→ JellySurfaceWave 확인
→ 접촉 법선 Y 값 확인
→ 가장 위쪽을 향하는 접촉점 저장
→ 표면 이동량 요청
→ Rigidbody2D 위치에 이동량 적용
→ 바닥 법선 방향 속도 제거
```
표면 이동량은 다음 함수를 통해 가져옵니다.
```csharp
_surfaceWave.GetSurfaceDeltaAtWorldPoint(_contactPoint);
```
```text
캐릭터 이동량
= 현재 표면 위치 - 이전 표면 위치
```

### 표면 추종 해제

```text
점프 또는 사망
→ SetFollowingEnabled(false)
→ 현재 JellySurfaceWave 참조 제거
→ 접촉점과 접촉 법선 초기화
```

---

# Material과 Shader

## JellyGround Material

젤리 지형의 `TilemapRenderer`에 적용하는 Material입니다.

```text
JellyGround.mat
└ JellyMario/2D/JellyGround Shader
```

## JellyGround Shader 기능
- 최대 4개의 충돌 위치와 방향을 전달받습니다.
- 충돌 지점에서 파동과 눌림 효과를 계산합니다.
- 거리와 시간에 따라 파동을 감소시킵니다.
- 여러 파동의 이동량을 합산합니다.
- 합산된 이동량이 최대 범위를 넘지 않도록 제한합니다.
- URP 2D Renderer에서 화면을 출력합니다.

## Shader 주요 속성
```text
Impact Frequency
Impact Speed
Impact Falloff
Impact Decay
Wave Height Multiplier
Max Combined Wave Offset
```

## MaterialPropertyBlock

`JellySurfaceWave`는 `MaterialPropertyBlock`을 이용하여 지형별 파동 정보를 Shader에 전달합니다.
```text
충돌 위치
충돌 방향
충돌 시간
파동 세기
```
실행 중에는 `JellySurfaceWave` Inspector의 값이 MaterialPropertyBlock을 통해 적용됩니다.
따라서 지형별 파동 설정은 Material보다 `JellySurfaceWave` Inspector에서 조정하는 것을 권장합니다.

---

# 오브젝트 구성

## 지형 구성
젤리 지형은 `JellySerfaceWave`를 통해 Tielmap화면과 Collider에 파동을 적용합니다.
```text
Grid
└ Ground
    ├ Tilemap
    ├ TilemapRenderer
    │   └ JellyGround Material
    ├ TilemapCollider2D
    ├ Rigidbody2D
    ├ CompositeCollider2D
    └ JellySurfaceWave
```

### 필수 컴포넌트
- `Grid`
- `Tilemap`
- `TilemapRenderer`
- `TilemapCollider2D`
- `JellySurfaceWave`
- `JellyGround` Material

### 파동 Collider 사용 시 필요한 컴포넌트
- `Rigidbody2D`
- `CompositeCollider2D`

> **참고**
>
> - `JellyGround` Material은 `TilemapRenderer`에 등록합니다.
> - `JellySurfaceWave`는 Ground 오브젝트에 추가합니다.
> - `Animate Tile Collider`가 활성화되어 있으면 필요한 `Rigidbody2D`와 `CompositeCollider2D`를 실행 중 자동으로 생성할 수 있습니다.
> - 안정적인 구성을 위해 물리 컴포넌트를 씬에 미리 추가하는 것을 권장합니다.

## 캐릭터 구성
캐릭터가 외형 변경을 위한`JellyVisual`와 추종을 위한`JellySurfaceFollower2D`로 구성합니다.
```text
Character
├ Rigidbody2D
├ Collider2D
├ CharacterController
├ JellyVisual
├ JellySurfaceFollower2D
└ DeformationPivot
    └ Visual
        └ SpriteRenderer
```

### Jelly 외형 사용 시 필요한 컴포넌트
- `Rigidbody2D`
- `JellyVisual`
- `Visual` 자식 오브젝트
- `SpriteRenderer`

### 주총 기능 사용 시 필요한 컴포넌트
- `JellySurfaceFollower2D`
- `Rigidbody2D`
- `Collider2D`
- `JellySurfaceWave`

> **주의**
>
> - `JellyVisual.Visual`은 반드시 등록해야 합니다.
> - Visual에는 부모 Transform이 필요합니다.
> - `Collider2D`는 Trigger가 아닌 일반 충돌로 설정해야 합니다.
> - 점프할 때 표면 추종을 해제하고 착지할 때 다시 활성화해야 합니다.

---

# 파일 위치

## 스크립트 위치
```text
Assets
└ Scripts
    └ Jelly
        ├ JellySurfaceWave.cs
        ├ JellyVisual.cs
        └ JellySurfaceFollower2D.cs
```

## Material 및 Shader 위치
```text
Assets
└ Materials
    ├ JellyGround.mat
    └ JellyGround.shader
```