# JellyAlien

A physics-based jelly platformer game inspired by the unique movement and challenge of jelly-like characters.

## Overview

JellyAlien은 젤리 특유의 탄성과 불안정한 움직임을 중심으로 한 2D 플랫폼 게임입니다.
플레이어는 말랑한 외계 생명체를 조작하여 다양한 적과 장애물을 극복하고 최종 보스전까지 생존해야 합니다.

게임 내 캐릭터, 몬스터, 보스는 모두 젤리 컨셉을 기반으로 설계되었으며, 젤리 물리 효과와 무중력 시스템을 통해 기존 플랫폼 게임에서는 경험하기 어려운 독특한 조작감을 제공합니다.

### [플레이 시연 영상](https://youtu.be/mp5-O3RvsLg?si=9jmo4daUHQP_YSb7)

## Features

**Physics Based Movement**
- 젤리 물리 기반 캐릭터 이동
- 탄성과 기울기를 활용한 독특한 조작 방식
- 수학적 계산 기반 Jelly Deformation 구현

**Various Stages**
- Tutorial Stage
- Hard Stage
- Boss Stage

**Enemy System**
- 6종의 일반 몬스터
- 다양한 이동 및 공격 패턴
- 대형 슬라임 보스

**Ranking System**
- 닉네임 등록 기능
- 클리어 타임 측정
- 데이터베이스 연동
- 실시간 랭킹 확인

**Boss Battle**
- 4단계 패턴 기반 보스전
- 무중력 환경이 적용된 공격 패턴
- 플레이어 추적 장애물 시스템

## Controls

| Key | Action |
|-----|--------|
| ↑   | Jump |
| ←   | Tilt Left |
| →   | Tilt Right |

캐릭터는 머리가 바라보는 방향으로 이동하며, 좌우 기울기와 점프를 조합하여 맵을 공략합니다.

## Player System

게임 시작 전 플레이어는:
1. 캐릭터 선택 (5종)
2. 닉네임 입력

을 수행해야 합니다. 선택된 닉네임은 클리어 기록과 함께 저장되어 랭킹 시스템에 반영됩니다.

## Boss Patterns

| Phase | Pattern | Description |
|-------|---------|-------------|
| 1 | Charge Attack | 플레이어 방향으로 고속 돌진합니다. |
| 2 | Fire Breath | 무중력 환경에서 불꽃을 분사합니다. |
| 3 | Mini Slime Throw | 소형 슬라임을 지속적으로 생성합니다. |
| 4 | Tracking Obstacle | 플레이어를 추적하는 특수 장애물을 생성합니다. |

## Ranking

게임 시작과 동시에 타이머가 시작됩니다. 보스 스테이지 클리어 후 기록된 시간은 데이터베이스에 저장되며, 랭킹 메뉴에서 확인할 수 있습니다. 랭킹은 클리어 시간이 빠른 순으로 정렬됩니다.

## Tech Stack

**Client**
- Unity
- C#

**Graphics**
- Shader Programming
- Particle Effects
- Jelly Physics

**Backend**
- Unity WebGL
- supabase

**Development Tools**
- Git
- Visual Studio

## Documentation

세부 구현 문서는 `/docs` 폴더를 참고하세요.
- [Framework](Docs/Framework.md)
- [Physics System](Docs/.md)
- [Network & Ranking Server](Docs/DB.md)

## Team Jellien

**김채림** — Team Lead / Network / Database
- DB Managemet
- Network Architecture
- Web Depoloyment
- Basic Monster Logic

**김서진** — Gameplay Programmer
- Physics System
- Mathematical Logic
- Particle Effects

**이수빈** — Framework Programmer
- Framework Architecture
- Level Design
- Object Interaction System
- Content Integration
- Sound
- Boss Monster Logic
