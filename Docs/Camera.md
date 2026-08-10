# Camera

## 개요

프로젝트에서 쓰이는 Camera시스템의 구성과 기능을 설명합니다.
게임 플레이 씬에서는 `FollowCamera`가 Player를 추적하며, MainMenu와 Init 씬에서는 고정된 Main Camera를 사용합니다.

## 목차
1. [Main Camera](#main-camera)
   - [Camera](#camera)
   - [Audio Listener](#audio-listener)
   - [Universal Additional Camera Data](#universal-additional-camera-data)
2. [Follow Camera](#follow-camera)
   - [Inspector 설정](#inspector-설정)
   - [주요 함수](#주요-함수)
   - [기능](#기능)
3. [Camera Manager](#camera-manager)
4. [Scene별 설정](#Scene별-설정)
5. [관련 클래스](#관련-클래스)
6. [파일 위치](#파일-위치)

---

# Main Camera

Main Camera는 게임 화면을 출력하는 기본 카메라 오브젝트입니다.
```text
Main Camera
├ Transform
├ Camera
├ AudioListener
├ UniversalAdditionalCameraData
└ FollowCamera
```

---

## Camera

Unity에서 게임 화면을 출력하는 기본 컴포넌트입니다.

### 주요 역할
- 게임 월드를 화면에 출력
- 화면에 표시할 Layer 선택
- 배경색 설정
- 카메라 화면 크기 설정
- Orthographic 방식으로 2D 화면 출력

### 주요 Inspector 설정
| 항목 | 설명 |
|------|------|
| `Projection` | 2D 화면 출력을 위해 Orthographic을 사용합니다. |
| `Size` | 카메라에 표시되는 세로 범위를 설정합니다. |
| `Clipping Planes` | 카메라가 렌더링할 최소·최대 거리를 설정합니다. |
| `Viewport Rect` | 카메라 화면이 표시될 영역을 설정합니다. |
| `Depth` | 여러 Camera가 있을 때 출력 순서를 설정합니다. |
| `Culling Mask` | 화면에 표시할 Layer를 선택합니다. |
| `Background` | 배경이 없는 영역에 표시할 색상을 설정합니다. |

### Orthographic Size

`Orthographic Size`가 클수록 화면에 더 넓은 범위가 표시됩니다.
```text
Size 증가
→ 화면에 더 넓은 영역 표시
→ 오브젝트가 작게 보임

Size 감소
→ 화면에 더 좁은 영역 표시
→ 오브젝트가 크게 보임
```

---

## Audio Listener

카메라 위치를 기준으로 게임의 Audio를 듣는 컴포넌트입니다.

### 주요 역할
- AudioSource에서 재생되는 소리를 수신
- 카메라 위치를 기준으로 2D 및 3D Audio 출력
- 플레이어가 듣는 게임 사운드의 기준점 역할
> **주의**
>
> 하나의 씬에는 활성화된 `AudioListener`가 하나만 존재해야 합니다.
> 여러 AudioListener가 동시에 활성화되면 Unity에서 경고 메시지가 출력됩니다.

---

## Universal Additional Camera Data

URP에서 Camera의 추가 렌더링 설정을 관리하는 컴포넌트입니다.

### 주요 역할
- URP Camera 렌더링 설정
- Shadow 렌더링 여부 설정
- Post Processing 사용 여부 설정
- Depth Texture 사용 여부 설정
- Camera Stack 설정
- 2D Renderer 연결
이 컴포넌트는 URP에서 Main Camera를 생성하면 자동으로 추가될 수 있습니다.

### 주요 Inspector 설정
| 항목 | 설명 |
|------|------|
| `Render Type` | Base 또는 Overlay Camera를 설정합니다. |
| `Renderer` | 카메라가 사용할 URP Renderer를 설정합니다. |
| `Post Processing` | 후처리 효과를 적용할지 설정합니다. |
| `Render Shadows` | 그림자를 렌더링할지 설정합니다. |
| `Depth Texture` | 화면의 깊이 정보를 생성할지 설정합니다. |
| `Opaque Texture` | 불투명 오브젝트의 화면 Texture를 생성할지 설정합니다. |
| `Camera Stack` | Overlay Camera를 Base Camera에 연결합니다. |

---

# Follow Camera

`FollowCamera`는 Player의 위치를 추적하는 컴포넌트입니다.
Player가 설정된 화면 여백을 벗어나면 카메라가 부드럽게 Player를 따라갑니다.

같은 GameObject에 여러 개의 `FollowCamera`가 추가되지 않도록 `DisallowMultipleComponent`가 적용되어 있습니다.
```csharp
[DisallowMultipleComponent]
public class FollowCamera : MonoBehaviour
```

---

## Inspector 설정
| 항목 | 기본값 | 설명 |
|------|------|------|
| `Target` | 자동 탐색 | 카메라가 추적할 Transform |
| `Follow Speed` | 8 | 카메라가 목표 위치를 따라가는 속도 |
| `Horizontal Margin` | 1 | 카메라가 움직이지 않는 좌우 여백 |
| `Vertical Margin` | 2 | 카메라가 움직이지 않는 상하 여백 |

---

## 주요 함수

### Start()
```csharp
private void Start()
```
Main Camera의 화면 설정 정보를 Console에 출력합니다.
> 확인을 위한 디버그 함수입니다.

### LateUpdate()
```csharp
private void LateUpdate()
```
Player의 이동이 처리된 후 카메라 위치를 갱신합니다.

#### 역할
- 추적 Target이 있는지 확인합니다.
- Target이 없으면 PlayerController를 자동으로 탐색합니다.
- Player와 카메라 사이의 거리를 계산합니다.
- Player가 화면 여백을 벗어났는지 확인합니다.
- 카메라가 이동할 목표 위치를 계산합니다.
- Follow Speed에 따라 카메라를 부드럽게 이동합니다.
- 카메라의 Z축 위치는 유지합니다.
> Player의 위치가 먼저 변경된 다음 카메라 이동이 실행되도록 LateUpdate()를 사용합니다.

---

## 기능

### Player 자동 탐색

Target이 비어 있으면 다음 함수로 Player를 탐색합니다.
```csharp
PlayerController player =
    FindAnyObjectByType<PlayerController>();
```

Player를 찾으면 Player의 Transform을 Target으로 등록합니다.
```csharp
target = player.transform;
```
PlayerSpawner가 게임 시작 후 Player를 생성하더라도 카메라가 생성된 Player를 자동으로 찾을 수 있습니다.

---

### 화면 여백

Player가 설정한 화면 여백 안에 있으면 카메라는 움직이지 않습니다.
```text
Player가 여백 안에 있음
→ Camera 이동 없음

Player가 여백 밖으로 이동
→ Camera 목표 위치 계산
→ Player 추적
```
좌우 여백은 `Horizontal Margin`, 상하 여백은 `Vertical Margin`을 사용합니다.

---

### 목표 위치 계산

Player가 오른쪽 여백을 벗어나면 Player가 여백 끝에 위치하도록 카메라의 목표 X 좌표를 계산합니다.
```text
목표 X = Player X - Horizontal Margin
```
Player가 왼쪽 여백을 벗어나면 다음 계산을 사용합니다.
```text
목표 X = Player X + Horizontal Margin
```
상하 이동도 `Vertical Margin`을 이용하여 같은 방식으로 계산합니다.

---

### 부드러운 카메라 이동

`Mathf.Lerp()`를 이용하여 현재 위치에서 목표 위치로 부드럽게 이동합니다.
```text
새로운 위치
= Lerp(현재 위치, 목표 위치, Follow Ratio)
```
Follow Ratio는 다음 계산을 사용합니다.
```text
Follow Ratio
= 1 - Exp(-Follow Speed × Delta Time)
```
이 계산을 사용하면 프레임 속도가 달라져도 비슷한 카메라 이동 속도를 유지할 수 있습니다.

> Follow Speed 값이 클수록 카메라가 Player를 빠르게 따라갑니다.

---

# Scene별 설정

| Scene | FollowCamera | Orthographic Size | Horizontal Margin | Vertical Margin |
|----|--------------|-------------------|-------------------|-----------------|
| `Init` | 사용하지 않음 | 5 | - | - |
| `MainMenu` | 사용하지 않음 | 10 | - | - |
| `Tutorial` | 사용 | 10 | 1 | 2 |
| `Hard` | 사용 | 10 | 1 | 2 |
| `Boss` | 사용 | 15 | 4 | 2 |

## Init, MainMenu

화면을 출력하는 고정 Camera입니다.

## Tutorial, Hard

Player를 추적하는 Camera입니다.
```text
Follow Speed       8
Horizontal Margin  1
Vertical Margin    2
```

## Boss

보스 전투의 넓은 화면을 출력하기 위해 다른 씬보다 큰 Orthographic Size와 Horizontal Margin을 사용합니다.
```text
Orthographic Size  15
Follow Speed       8
Horizontal Margin  4
Vertical Margin    2
```

---

# 관련 클래스

| 클래스 및 컴포넌트 | 설명 |
|--------------------|------|
| `FollowCamera` | Player의 위치를 부드럽게 추적합니다. |
| `PlayerController` | FollowCamera가 자동으로 탐색하는 Player 컴포넌트입니다. |
| `Camera` | 게임 월드를 화면에 출력합니다. |
| `AudioListener` | 카메라 위치를 기준으로 게임 Audio를 수신합니다. |
| `UniversalAdditionalCameraData` | URP Camera의 추가 렌더링 설정을 관리합니다. |

---

# 파일 위치

## Camera 스크립트
```text
Assets
└ Scripts
    └ Camera
        └ FollowCamera.cs
```

## 사용 Scene
```text
Assets
└ Scenes
    ├ Init.unity
    ├ MainMenu.unity
    ├ Tutorial.unity
    ├ Hard.unity
    └ Boss.unity
```