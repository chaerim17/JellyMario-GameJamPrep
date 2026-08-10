# DB

## 개요

Jelly Mario의 랭킹 시스템은 Supabase(PostgreSQL)를 사용합니다.

Unity는 `WebManager`를 통해 Supabase REST API와 직접 통신하며, 별도의 Spring Boot 서버는 사용하지 않습니다.

```text
Unity
  ↓
Supabase REST API
  ↓
PostgreSQL
```

---

## Supabase 프로젝트

| 항목 | 내용 |
| --- | --- |
| Project Name | `jelly-mario-ranking` |
| Service | Supabase |
| Database | PostgreSQL |
| --- | --- |
| Project Name | `jelly-mario-player-info` |
| Service | Supabase |
| Database | PostgreSQL |


---

## 테이블 구조

#### `player`

```sql
CREATE TABLE player (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "playerName" VARCHAR(20) NOT NULL,
    "characterId" INT NOT NULL,
    "createdAt" TIMESTAMP DEFAULT NOW()
);
```

| 컬럼 | 설명 |
|--------|--------|
| id | 기본 키(PK) |
| playerName | 플레이어 닉네임 |
| characterId | 선택한 캐릭터 번호 |
| createdAt | 데이터 생성 시각 |

### `ranking`

```sql
CREATE TABLE ranking (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "playerName" VARCHAR(20) NOT NULL,
    "clearTime" FLOAT NOT NULL,
    "createdAt" TIMESTAMP DEFAULT NOW()
);
```

| 컬럼 | 설명 |
| --- | --- |
| `id` | 기본 키(PK) |
| `playerName` | 플레이어 이름 |
| `clearTime` | 클리어 시간 |
| `createdAt` | 데이터 생성 시각 |

---

## API

### 플레이어 프로필 저장

**Method:** `POST`
**Endpoint:** `/rest/v1/player`
**Status:** 구현 완료

```json
{
  "playerName": "Chaerim",
  "characterId": 2
}
```

### 랭킹 저장

**Method:** `POST`  
**Endpoint:** `/rest/v1/ranking`
**Status:** 구현 완료

```json
{
  "playerName": "Chaerim",
  "clearTime": 42.53
}
```

### 랭킹 조회

**Method:** `GET`  
**Endpoint:** `/rest/v1/ranking?select=*`
**Status:** 구현 완료

현재 게임은 닉네임 및 캐릭터 선택 기능을 사용하며,
Supabase를 통해 플레이어 프로필 및 랭킹 정보를 저장합니다.

---

## Unity 연동

### `WebManager`

`UnityWebRequest` 기반으로 Supabase REST API에 직접 접근합니다.

### 사용 헤더

```csharp
request.SetRequestHeader("apikey", API_KEY);

request.SetRequestHeader(
    "Authorization",
    $"Bearer {API_KEY}"
);

request.SetRequestHeader(
    "Content-Type",
    "application/json"
);
```

---

## 권한

anon 권한 정책:

| 작업 | 허용 여부 |
| --- | --- |
| `SELECT` | 허용 |
| `INSERT` | 허용 |
| `UPDATE` | 비허용 |
| `DELETE` | 비허용 |

---

## Known Issues

### PostgreSQL 대소문자

PostgreSQL은 큰따옴표 없이 컬럼을 생성하면 컬럼명을 자동으로 소문자로 변환합니다.

```text
playerName
   ↓
playername
```

따라서 Unity DTO와 동일한 이름을 유지하려면 컬럼 생성 시 반드시 큰따옴표를 사용해야 합니다.

```sql
"playerName"
"clearTime"
"createdAt"
```

> 이 규칙을 지키지 않으면 Unity JSON 필드명과 DB 컬럼명이 달라져 요청 또는 응답 처리에서 문제가 생길 수 있습니다.

---
