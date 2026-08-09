# Client Framework

## 1. 프레임워크 설명

Jelly Alien 프로젝트에서 사용하는 공통 시스템을 관리하기 위한
클라이언트 프레임워크입니다.

Manager 기반 구조를 사용하며,
씬 관리, 플레이어 관리, 입력 처리, 사운드 관리,
웹 API 통신 및 UI 시스템을 포함합니다.

프로젝트 전반에서 사용하는 Core, Manager, Player, Enemy, Boss, UI, Network 등의 시스템을 역할별로 분리하여 관리할 수 있도록 구조를 구성했습니다.

또한 ManagersHub를 통해 전역 Manager에 일관된 방식으로 접근할 수 있도록 하였으며, 씬 구성과 리소스 관리, 플레이어 및 적 오브젝트, 보스 시스템 등 프로젝트에서 공통으로 사용하는 기능을 쉽게 확장하고 유지보수할 수 있는 기반을 마련했습니다.

---

## 2. 프로젝트 구조

### 에셋 구조
```text
Assets
├── Audio
│   ├── BGM (Background Music)
│   │   └── mixkit-deep-urban-623
|   |
│   └── SFX (Sound Effects)
│       ├── mixkit-technological-futuristic-hum-2133
│       ├── sfx_bump
│       ├── sfx_coin
│       ├── sfx_disappear
│       ├── sfx_gem
│       ├── sfx_hurt
│       ├── sfx_jump
│       ├── sfx_magic
│       ├── sfx_select
│       └── sfx_throw
|
├── Materials
│   ├── JellyGround.mat
│   └── JellyGround.shader
|
├── Prefabs
│   ├── BossPattern
│   │   ├── BossSlime
│   │   ├── Fireball
│   │   └── Missile
|   |
│   └── Enemy
│       ├── BeeEnemy
│       ├── BlockEnemy
│       ├── FrogEnemy
│       ├── RollingEnemy
│       ├── SnailEnemy
│       ├── SpawnEnemy
│       └── Square
|
├── Scenes
│   ├── Boss.unity
│   ├── Hard.unity
│   ├── Init.unity
│   ├── MainMenu.unity
│   └── Tutorial.unity
│
├── Scripts
│   ├── Boss
│   │   ├── BossBase.cs
│   │   ├── BossController.cs
│   │   ├── BossSlimeController.cs
│   │   ├── FireballController.cs
│   │   └── MissileController.cs
│   │
│   ├── Camera
│   │   └── FollowCamera.cs
│   │
│   ├── Core
│   │   ├── Define.cs
│   │   ├── ManagersHub.cs
│   │   └── Singleton.cs
│   │
│   ├── Enemy
│   │   ├── CrawlEnemy.cs
│   │   ├── EnemyBase.cs
│   │   ├── EnemyController.cs
│   │   ├── FallingEnemy.cs
│   │   ├── FlyEnemy.cs
│   │   ├── JumpEnemy.cs
│   │   ├── SpawnEnemy.cs
│   │   └── SurfaceEnemy.cs
|   |
│   ├── Jelly
│   │   ├── JellySurfaceWave.cs
│   │   └── JellyVisual.cs
│   │
│   ├── Managers
│   │   ├── GameManager.cs
│   │   ├── InputManager.cs
│   │   ├── PlayerManager.cs
│   │   ├── SceneManagerEx.cs
│   │   ├── SoundManager.cs
│   │   ├── TimerManager.cs
│   │   └── WebManager.cs
│   │
│   ├── Network
│   │   ├── Request
│   │   │   ├── SubmitProfileRequest.cs
│   │   │   └── SubmitScoreRequest.cs
|   |   |
│   │   └── Response
│   │        ├── RankingData.cs
│   │        └── RankingResponse.cs
│   │
│   ├── Player
│   │   ├── PlayerBase.cs
│   │   └── PlayerController.cs
│   │
│   └── UI
│       ├── BossUI.cs
│       ├── InGameUI.cs
│       ├── LoginUI.cs
│       ├── MainMenuUI.cs
│       ├── RankingUI.cs
│       ├── ResultUI.cs
│       ├── TimerUI.cs
│       └── UIBase.cs
│
├── Settings
│   ├── Build Profiles
│   └── Scenes
│
├── Sprites
│   ├── Backgrounds
│   │   └── Parts
│   │
│   ├── Character
│   │
│   ├── Enemies
│   │
│   ├── Tiles
│   │   └── Palette
│   │
│   └── UI
│       ├── Blue
│       ├── Extra
│       ├── Green
│       ├── Grey
│       ├── Red
│       └── Yellow
|
├── TextMesh Pro
│   ├── Fonts
│   │   ├── LiberationSans
│   │   └── LiberationSans - OFL
│   │
│   ├── Resources
│   │   ├── Fonts & Materials
│   │   │   ├── BMKkubulim
│   │   │   ├── BMKkubulim SDF
│   │   │   ├── Cloudsofa_namgim-Regular
│   │   │   ├── Cloudsofa_namgim-Regular SDF
│   │   │   ├── HiKR-ExtraBold
│   │   │   ├── HiKR-ExtraBold SDF
│   │   │   ├── LiberationSans SDF
│   │   │   ├── LiberationSans SDF - Drop Shadow
│   │   │   ├── LiberationSans SDF - Fallback
│   │   │   ├── LiberationSans SDF - Outline
│   │   │   ├── Mona12
│   │   │   ├── Mona12 SDF
│   │   │   ├── Mona12-Bold
│   │   │   ├── Mona12-Bold SDF
│   │   │   ├── x10y12pxDenkiChipHangul
│   │   │   └── x10y12pxDenkiChipHangul SDF
│   │   │
│   │   ├── Style Sheets
│   │   │   └── Default Style Sheet
│   │   │
│   │   ├── LineBreaking Following Characters
│   │   ├── LineBreaking Leading Characters
│   │   └── TMP Settings
│   │
│   └── Shaders
|
└── UI
    ├── BossUI
    ├── LoginPanel
    ├── RankingPanel
    └── TimerPanel
```

### 씬 구조
```text
Scenes
├── Boss
│   ├── Main Camera
│   ├── Global Light 2D
│   ├── Canvas
│   │   ├── BossUI
│   │   └── TimerUI
|   |
│   ├── Grid
│   │   ├── Ground
│   │   ├── Wall
│   │   ├── Hazard
│   │   └── Background
│   │       ├── Background_sky
│   │       ├── Background_under
│   │       └── Background_middle
│   │
│   ├── PlayerSpawn
│   │   └── Player
│   │
│   ├── InputManager
│   ├── Boss
|   └── SpawnPoint
│       ├── CenterPoint
│       ├── BreathSpawnPoint
│       ├── MissileSpawnPoint
│       ├── BossStartPoint
│       ├── MonsterSpawnPoint1
│       ├── MonsterSpawnPoint2
│       ├── MonsterSpawnPoint3
│       ├── MonsterSpawnPoint4
│       ├── MonsterSpawnPoint5
│       ├── MonsterSpawnPoint6
│       └── PlayerSpawnPoint
|
├── Hard
│   ├── Main Camera
│   ├── Global Light 2D
│   ├── Canvas
│   │   └── TimerUI
|   |
│   ├── Grid
│   │   ├── Ground
│   │   ├── Hazard
│   │   ├── Goal Flag
│   │   └── Background
│   │       ├── Background_middle
│   │       ├── Background_sky
│   │       └── Background_under
│   │
│   ├── InputManager
│   ├── DeathZone
│   └── PlayerSpawn
│       └── Player
│   
├── Init
│   ├── Main Camera
│   ├── Global Light 2D
│   └── Managers
|
├── MainMenu
│   ├── Main Camera
│   ├── Global Light 2D
│   ├── Canvas
|   |   ├── RankingButtonUI
|   |   ├── LoginPanel
│   │   └── RankingPanel
|   |
│   ├── Grid
|   |   ├── Ground
|   |   ├── Goal Flag
│   │   |    └── Trigger
|   |   |
│   │   ├── Title
│   │   |    ├── Jelly Alien
│   │   |    └── Notice
|   |   |
│   │   ├── Background
│   │   |    ├── Background_middle
│   │   |    ├── Background_sky
│   │   |    └── Background_under
│   |   |
│   │   └── InvisibleWall
│   |        ├── InvisibleWall_left
│   |        ├── InvisibleWall_right
│   │        └── InvisibleWall_top
|   |
|   ├── PlayerSpawn
|   │    └── Player
|   |
|   └── Manager
|
└── Tutorial
    ├── Main Camera
    ├── Global Light 2D
    ├── Canvas
    │    └── TimerUI
    |
    ├── Grid
    │   ├── Ground
    │   ├── Hazard
    │   ├── Goal Flag
    │   └── Background
    │       ├── Background_middle
    │       ├── Background_sky
    │       └── Background_under
    │
    ├── PlayerSpawn
    │   └── Player
    │
    ├── DeathZone
    └── InputManager
```
---

## 3. 프로젝트 폴더 역할

### Audio

게임에서 사용하는 BGM 및 효과음 리소스를 관리합니다.

---

### Materials

게임에서 사용하는 Material을 관리합니다.

---

### Prefabs

게임에서 사용하는 프리팹을 관리합니다.

- BossPattern : 보스 패턴(화염구, 미사일, 소환 몬스터) 프리팹
- Enemy : 일반 몬스터 프리팹

---

### Scenes

게임에서 사용하는 씬을 관리합니다.

Init, MainMenu, Tutorial, Hard, Boss 씬을 포함합니다.

---

### Scripts

프로젝트의 모든 C# 스크립트를 관리합니다.

---

### Boss

보스 AI와 패턴을 담당하는 클래스를 관리합니다.

---

### Camera

카메라 동작을 담당하는 클래스를 관리합니다.

---

### Core

프로젝트 전반에서 사용하는 공통 기능을 관리합니다.

Singleton, ManagersHub, 공통 Enum 등을 포함합니다.

---

### Enemy

일반 몬스터의 AI 및 동작을 담당하는 클래스를 관리합니다.

---

### Jelly

게임의 젤리 동작을 담당하는 클래스를 관리합니다.

---

### Managers

게임의 전역 기능을 관리하는 Manager 클래스를 관리합니다.

---

### Network

DB 데이터 클래스를 관리합니다.

Request와 Response 데이터를 포함합니다.

---

### Player

플레이어 관련 클래스를 관리합니다.

---

### UI

게임 UI 관련 클래스를 관리합니다.

---

### Settings

프로젝트 설정 파일을 관리합니다.

Build Profile 및 Scene 설정을 포함합니다.

---

### Sprites

게임에서 사용하는 스프라이트 이미지를 관리합니다.

배경, 캐릭터, 적, 타일, UI 리소스를 포함합니다.

---

### TextMesh Pro

TextMesh Pro에서 사용하는 폰트, 셰이더 및 설정 파일을 관리합니다.

---

### UI (Assets)

UI 프리팹 및 UI 리소스를 관리합니다.

---

## 4. 클래스 역할

### Define

프로젝트에서 사용하는 공통 Enum을 관리합니다.

- GameState
- SceneType

---

### Singleton<T>

모든 Manager의 부모 클래스입니다.

프로젝트 내에서 하나의 객체만 생성되도록 보장하며, 씬이 변경되어도 유지됩니다.

---

### ManagersHub

모든 Manager 접근을 한 곳으로 통합합니다.

- ManagersHub.Game
- ManagersHub.Scene
- ManagersHub.Web
- ManagersHub.Player
- ManagersHub.Input
- ManagersHub.Sound

---

### GameManager

게임의 상태와 플레이어 정보를 관리합니다.

---

### SceneManagerEx

씬 전환 및 현재 씬을 관리합니다.

---

### WebManager

웹 API 통신을 담당합니다.

- GetRanking()
- SubmitScore()

---

### PlayerManager

현재 플레이어를 관리합니다.

---

### InputManager

플레이어 입력을 관리합니다.

---

### SoundManager

BGM 및 효과음을 관리합니다.

---

### TimerManager

게임 플레이 시간을 측정하고 관리합니다.

---

### BossBase

모든 보스가 공통으로 사용하는 기본 클래스입니다.

---

### BossController

보스의 상태 및 패턴을 관리합니다.

---

### FireballController

보스의 화염구 패턴을 관리합니다.

---

### MissileController

보스의 미사일 패턴을 관리합니다.

---

### BossSlimeController

보스가 소환하는 몬스터를 관리합니다.

---

### FollowCamera

플레이어를 따라 이동하는 카메라를 관리합니다.

---

### EnemyBase

모든 몬스터가 공통으로 사용하는 기본 클래스입니다.

---

### EnemyController

몬스터의 공통 동작을 관리합니다.

---

### CrawlEnemy

기어가는 몬스터를 구현합니다.

---

### FallingEnemy

낙하하는 몬스터를 구현합니다.

---

### FlyEnemy

비행 몬스터를 구현합니다.

---

### JumpEnemy

점프하는 몬스터를 구현합니다.

---

### SpawnEnemy

몬스터를 생성하는 오브젝트를 구현합니다.

---

### SurfaceEnemy

벽이나 천장을 이동하는 몬스터를 구현합니다.

---

### JellySurfaceWave

젤리 표면에 충돌이 발생했을 때 파동 효과를 생성하고, 표면의 비주얼과 Collider를 함께 변형합니다.

---

### JellyVisual

충돌 및 이동에 반응하여 젤리 캐릭터가 눌리거나 늘어나는 변형 효과를 관리합니다.

---

### PlayerBase

플레이어의 공통 기능을 정의한 부모 클래스입니다.

---

### PlayerController

실제 플레이어를 제어하는 클래스입니다.

생성 시 PlayerManager에 등록됩니다.

---

### UIBase

모든 UI의 부모 클래스입니다.

---

### MainMenuUI

메인 메뉴 UI를 관리합니다.

---

### InGameUI

게임 플레이 UI를 관리합니다.

---

### ResultUI

게임 결과 UI를 관리합니다.

---

### RankingUI

랭킹 UI를 관리합니다.

---

### BossUI

보스 UI를 관리합니다.

---

### LoginUI

로그인 UI를 관리합니다.

---

### MainMenuUI

메인메뉴 UI를 관리합니다.

---

### TimerUI

타이머 UI를 관리합니다.

---

### RankingData

랭킹 한 명의 정보를 저장합니다.

---

### RankingResponse

랭킹 조회 결과를 저장합니다.

---

## 5. 씬 역할

### Init

게임 실행 시 가장 먼저 실행되는 씬입니다.

Managers를 생성 및 초기화한 후 MainMenu 씬으로 이동합니다.

---

### MainMenu

게임의 시작 화면입니다.

닉네임 입력, 게임 시작 및 랭킹 조회 기능을 제공합니다.

---

### Tutorial

게임의 기본 조작을 익히기 위한 튜토리얼 스테이지입니다.

플레이어 이동, 점프, 함정 및 골인 시스템을 학습할 수 있습니다.

---

### Hard

고난이도 스테이지입니다.

다양한 함정과 지형을 활용하여 플레이어의 조작 능력을 요구하는 스테이지입니다.

---

### Boss

보스전 전용 스테이지입니다.

보스 패턴과 전투 시스템을 담당하며, 보스 전용 스폰 포인트와 패턴 오브젝트를 포함합니다.

주요 구성 요소
- Boss
- BossStartPoint
- CenterPoint
- BreathSpawnPoint
- MissileSpawnPoint
- MonsterSpawnPoint (1~6)
- PlayerSpawnPoint

---

## 6. 클래스 관계

```text
Singleton<T>
├── GameManager
├── SceneManagerEx
├── WebManager
├── PlayerManager
├── InputManager
└── SoundManager

ManagersHub
├── Game
├── Scene
├── Web
├── Player
├── Input
└── Sound

Boss
├── BossBase
├── BossController
├── FireballController
├── MissileController
└── BossSlimeController

Enemy
├── EnemyBase
├── EnemyController
├── CrawlEnemy
├── FallingEnemy
├── FlyEnemy
├── JumpEnemy
├── SpawnEnemy
└── SurfaceEnemy

Player
├── PlayerBase
└── PlayerController

Camera
└── FollowCamera

UI
├── BossUI
├── UIBase
├── LoginUI
├── MainMenuUI
├── InGameUI
├── TimerUI
├── ResultUI
└── RankingUI

Network
├── Request
│   └── SubmitScoreRequest
└── Response
    ├── RankingData
    └── RankingResponse
```
