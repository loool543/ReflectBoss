# 물리 쿼리 결과 배열 재사용 — 측정 기록

[← 프로젝트 소개](../README.md#성능-개선)

## 측정 목적

Stage 2 후반에서 투사체 수와 함께 증가하는 `BossProjectile.LateUpdate`의 반복 할당을 확인하고, 쿼리 결과 배열 재사용 전후를 비교했습니다. 목표는 **이동·충돌 검사 과정의 GC 할당 감소**입니다. 기존 플레이에서 GC로 인한 끊김이 확인됐다는 의미는 아닙니다.

## 환경과 자료의 범위

이 기록은 배열 재사용 변경 당시의 개발 빌드 자료입니다. README의 플레이 영상과 스크린샷은 이후 UI·연출을 추가한 버전이므로, 아래 수치를 최신 영상 빌드의 전체 성능 측정값으로 해석하지 않습니다.

| 항목 | 기록 |
| :--- | :--- |
| 측정일 | 2026-10-22 |
| Unity / 렌더링 | 6000.2.15f1 / URP 17.2.0 |
| 측정 대상 | Windows Player에 연결한 Unity Profiler |
| 수집 절차 | Windows Development Build, Deep Profile 비활성 기준으로 진행 |
| 게임 상황 | Stage 1 일반 플레이, Stage 2 중간·후반 플레이 |
| 변경 | 쿼리 결과 배열 재사용 — 커밋 `2aa6109` |
| 대표 지표 | BossProjectile.LateUpdate 하위 GC Alloc, Calls, Time ms |
| 자료 형태 | 개발자가 제공한 선택 프레임 스크린샷 6장 |

CPU/GPU 모델, 빌드 해상도, 프레임 제한 및 Script Debugging 설정의 동일 여부는 이 기록에서 확인하지 못했습니다. 수정 전후 사이 PC 재부팅도 있었습니다. 따라서 엄밀한 통제 벤치마크나 구간 평균으로 해석하지 않습니다. 캡처의 GPU 시간은 수집되지 않았습니다.

탄환을 별도 도구로 강제 생성한 테스트가 아니라 실제 Stage 2 플레이 중 도달한 고부하 상황입니다. 경과 시간은 추정값이어서 비교표에는 포함하지 않았습니다.

## 변경 내용

- `OverlapSphere` → `OverlapSphereNonAlloc`: 타겟과 벽 검사에서 Collider 배열 재사용.
- `SphereCastAll` → `SphereCastNonAlloc`: 이동 경로 검사에서 RaycastHit 배열 재사용.
- 배열은 투사체별로 16칸에서 시작하며, 가득 차면 두 배로 확장 후 같은 조건으로 재조회.
- 반환 개수만 순회하고, 스윕의 가장 가까운 유효 충돌 선택 유지.
- 탄환 수명·발사량·동기화 호출·레이어 조건은 이번 변경에서 유지.

## 선택 프레임 결과

GC Alloc과 Time은 투사체 메서드 및 하위 호출의 합계입니다. `SyncTransforms` 시간은 이 합계에 이미 포함됩니다.

| 상황 | 버전 | Frame | Calls | GC Alloc | Time ms | 캡처 |
| :--- | :--- | ---: | ---: | ---: | ---: | :--- |
| Stage 1 | Before | 40292 | 4 | 약 0.7KB | 0.10 | [원본](images/profiling/stage1-before.png) |
| Stage 1 | After | 125824 | 4 | 176B | 0.08 | [원본](images/profiling/stage1-after.png) |
| Stage 2 중간 | Before | 73418 | 60 | 11.4KB | 1.01 | [원본](images/profiling/stage2-60-before.png) |
| Stage 2 중간 | After | 144560 | 61 | 2.6KB | 0.87 | [원본](images/profiling/stage2-61-after.png) |
| Stage 2 후반 | Before | 80166 | 103 | 19.5KB | 2.15 | [원본](images/profiling/stage2-103-before.png) |
| Stage 2 후반 | After | 87524 | 103 | 4.4KB | 1.44 | [원본](images/profiling/stage2-103-after.png) |

103회 처리 시 GC.Alloc 발생 횟수는 **412회 → 103회**로 감소했습니다. 할당량 감소율은 표시된 반올림 값을 사용한 `(19.5 - 4.4) / 19.5 × 100 ≈ 77%`입니다.

### 해석

여러 부하 수준의 선택 프레임에서 할당 감소가 관측됐습니다. 남아 있는 할당의 정확한 발생 위치는 호출 스택으로 확정하지 않았으므로, 전체 할당 제거 또는 특정 API만의 할당이라고 단정하지 않습니다.

CPU 시간도 선택 프레임에서는 줄었으나, 변경하지 않은 SyncTransforms 시간 역시 103회 기준 1.14ms에서 0.85ms로 달랐습니다. 실행 환경·탄환 배치·충돌 상황의 차이가 포함되므로 CPU 감소분 전체를 배열 재사용의 효과로 귀속하지 않습니다.

초기 배열 생성과 용량 확장에는 할당이 발생합니다. 이 자료는 생성·확장 순간의 최악 비용이나 GC 수집 지연을 검증하지 않습니다. 버퍼는 투사체 생존 동안 유지되므로 전체 메모리 사용량 감소를 의미하지도 않습니다.

## 다시 측정할 때

1. 동일 PC·해상도·품질·프레임 제한·빌드 설정을 기록합니다.
2. Windows Player를 새로 실행하고, 같은 준비 과정을 거쳐 각 Stage에 진입합니다.
3. Profiler에서 Windows Player 연결과 Deep Profile 비활성을 확인합니다.
4. 유사한 투사체 처리 수의 구간을 수집하고, 생성·실패 전환·창 포커스 변경 프레임은 일반 플레이와 구분합니다.
5. 기록을 저장하고 여러 프레임의 분포를 비교합니다. 한 프레임의 낮은 수치를 평균 성능으로 제시하지 않습니다.
6. 할당 호출 스택 수집은 별도로 진행하고, 계측 조건이 다른 CPU 시간은 직접 비교하지 않습니다.

## 동작 검증의 범위

변경본의 컴파일 통과와 새 Windows 빌드의 플레이 캡처를 확보했습니다. 다음 항목에 대한 체계적인 회귀 테스트 통과 결과는 아직 기록하지 않았습니다.

- 이동 중 피격과 BasketBall 반사, 일반 Ball 피격.
- 벽 반사만으로 보스 피해가 발생하지 않는지.
- 벽 안 생성 복구와 모서리 연속 반사.
- 16칸보다 많은 결과로 버퍼 확장을 유도한 경우의 충돌 누락 여부.
- 실패·성공 화면과 Retry 후 상태 초기화.

## 참고 자료

- [Unity — Physics.OverlapSphereNonAlloc](https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Physics.OverlapSphereNonAlloc.html)
- [Unity — Physics.SphereCastNonAlloc](https://docs.unity3d.com/6000.2/Documentation/ScriptReference/Physics.SphereCastNonAlloc.html)

NonAlloc 쿼리는 버퍼를 자동 확장하지 않으며, SphereCastNonAlloc은 결과 순서와 가까운 결과만 포함함을 보장하지 않습니다. 따라서 버퍼 포화 시 재조회와 가장 가까운 유효 결과 선택을 함께 유지했습니다.
