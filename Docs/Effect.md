# Effects

## 개요

프로젝트에서 사용되는 Effects 시스템의 구성을 설명합니다.
현재 구현된 Effect는 플레이어 사망 시 스프라이트를 작은 조각으로 나누어 흩뿌리는 `PixelShatterEffect`입니다.

## 목차
1. [PixelShatterEffects](#playerbase)
 - [Inspector 설정](#inspector-설정)
 - [관련 클래스](#관련-클래스)
 - [구현 효과](#구현-효과)
2. [파일 위치](#파일-위치)

---

# PixelShatterEffects

`PixelShatterEffect`는 현재 SpriteRenderer에 표시된 스프라이트를 작은 조각으로 나누어 흩뿌리는 효과를 처리합니다.
각 조각에는 이동, 회전, 중력 및 Fade 효과가 적용됩니다.
같은 GameObject에 여러 개의 `PixelShatterEffect`가 추가되지 않도록 `DisallowMultipleComponent`가 적용되어 있습니다.

```csharp
[DisallowMultipleComponent]
public class PixelShatterEffect : MonoBehaviour
```

---

# Inspector 설정

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

## 조각 설정

| 항목 | 기본값 | 설명 |
|------|------|------|
| `Columns` | 8 | 원본 스프라이트를 가로로 나눌 개수 |
| `Rows` | 8 | 원본 스프라이트를 세로로 나눌 개수 |

생성되는 최대 조각 수는 다음과 같이 계산합니다.
```text
조각 수 = Columns × Rows
```
현재 Player 프리팹에서는 최대 64개의 픽셀 조각을 생성합니다.

## 움직임 설정

| 항목 | 기본값 | 설명 |
|------|------|------|
| `Duration` | 0.75초 | 픽셀 분해 효과가 유지되는 시간 |
| `Min Scatter Force` | 1.5 | 조각에 적용할 최소 분산 힘 |
| `Max Scatter Force` | 4.5 | 조각에 적용할 최대 분산 힘 |
| `Random Force` | 1.2 | 조각마다 추가되는 무작위 힘 |
| `Upward Force` | 2 | 모든 조각에 적용되는 위쪽 힘 |
| `Gravity` | 7 | 조각에 적용되는 아래쪽 중력 |
| `Max Angular Speed` | 720 | 조각에 적용되는 최대 회전 속도 |
| `Fade Start` | 0.3초 | 조각이 투명해지기 시작하는 시간 |

### Duration과 Fade Start

효과는 다음 순서로 진행됩니다.
```text
0초         픽셀 분해 시작
0~0.3초     조각이 완전히 보이는 상태
0.3~0.75초  조각이 점점 투명해짐
0.75초      조각 제거
```

효과가 적용되는 시간은 다음과 같이 계산합니다.
```text
Duration - Fade Start
```
기본값은 0.45초입니다.

---

# 관련 클래스

| 클래스 | 설명 |
|------|------|
| `PixelShatterEffect` | 스프라이트를 작은 조각으로 분해하고 이동, 회전 및 Fade 효과를 적용합니다. |
| `PlayerController` | 플레이어가 사망했을 때 PixelShatterEffect를 실행합니다. |
| `SpriteRenderer` | 분해할 원본 스프라이트와 렌더링 정보를 제공합니다. |
| `Fragment` | 생성된 각 조각의 이동 및 렌더링 정보를 저장하는 내부 클래스입니다. |

## 클래스 관계
```text
PlayerController
└ Die()
    └ PlayDeathSequence()
        └ PixelShatterEffect.Play()
            └ PlayRoutine()
                ├ CreateFragments()
                └ Fragment
```

## Fragment

`Fragment`는 생성된 각 픽셀 조각의 정보를 저장하는 내부 클래스입니다.

| 변수 | 설명 |
|------|------|
| `Transform` | 조각의 위치와 회전을 변경할 Transform |
| `Renderer` | 조각을 화면에 출력하는 SpriteRenderer |
| `RuntimeSprite` | 실행 중 생성한 조각 Sprite |
| `Velocity` | 조각의 현재 이동 속도 |
| `AngularSpeed` | 조각의 회전 속도 |
| `StartColor` | Fade 계산에 사용하는 원래 색상 |

---

# 구현 효과

## 픽셀 분해

원본 스프라이트를 행과 열로 나누어 조각 Sprite를 생성합니다.
```csharp
Sprite.Create(sourceSprite.texture, fragmentRect, pivot, pixelsPerUnit);
```

## 조각 분산

중심에서 바깥쪽으로 향하는 힘에 무작위 힘과 위쪽 힘을 추가합니다.
```text
조각 속도 = 바깥 방향 × 분산 힘 + 무작위 힘 + 위쪽 힘
```

## 조각 회전

각 조각에 서로 다른 회전 속도를 적용합니다.
```text
회전 속도 = Random(-Max Angular Speed, Max Angular Speed)
```

## 중력 효과

매 프레임 조각의 속도에 아래쪽 중력을 적용하고 위치를 변경합니다.
```csharp
fragment.Velocity += Vector2.down * gravity * deltaTime;
fragment.Transform.localPosition += (Vector3)(fragment.Velocity * deltaTime);
```

## Fade 효과

효과 종료 시간에 가까워질수록 Alpha 값을 감소시킵니다.
```text
Alpha = 1 - Fade 진행률
```

---

# 파일 위치

## Effect 스크립트
```text
Assets
└ Scripts
    └ Effects
        └ PixelShatterEffect.cs
```