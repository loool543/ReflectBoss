# 반사 성공 피드백

`BossProjectile.HandleHit()`에서 BasketBall의 플레이어 반사가 확정되고,
`IsReflected`와 보스를 향한 `Direction`을 설정한 직후 기존 `ReflectSuccess` 이벤트가 발생한다.
`ReflectFeedback`은 이 이벤트만 구독한다. 투사체 이동·반사 판정·피해 계산은 변경하지 않았다.

## 실행 흐름

`반사 성공 → ReflectSuccess → PlayFeedback → 문구/Glow 즉시 표시 → 페이드 → 숨김`

`UI_GameScene` 프리팹에 컴포넌트가 연결되어 있다. 기본 설정은 화면 가로 중앙,
아래에서 28% 높이에 `REFLECT!`를 표시한다. 0.12초 유지 후 페이드하여 총 0.45초에 사라진다.
문구 뒤의 부드러운 청록색 UI Glow와 1.12배 크기 팝 효과는 0.16초 동안 원상복구된다.
Glow는 UI 이미지로 표현하며 캐릭터 재질이나 월드 조명을 변경하지 않는다.

## Inspector 설정

| 필드 | 역할 |
| --- | --- |
| Feedback Root | 함께 페이드/확대할 전용 UI 부모. 비워 두면 실행 시 생성한다. HUD 전체를 연결하지 않는다. |
| Label | 사용할 TMP 텍스트. Message와 Text Color가 적용되며 폰트/크기는 지정한 컴포넌트를 따른다. |
| Icon | 문구 대신 또는 문구와 함께 표시할 이미지. Label과 Icon이 모두 없으면 기본 문구를 생성한다. |
| Glow Image | 짧게 빛나는 배경 이미지. 비워 두면 부드러운 원형 이미지를 생성한다. Icon과 별개의 이미지여야 한다. |
| Message | 표시할 문구. 기본값 `REFLECT!`. 초기화 시 적용한다. |
| Screen Anchor | 자동 생성 UI의 화면 기준 위치. `(0.5, 0.28)`은 중앙 하단이다. |
| Offset | 자동 생성 UI의 위치 보정값. Canvas 좌표 기준이다. |
| Text Color | 문구 색상. |
| Duration | 유지와 페이드를 포함한 총 표시 시간. 기본 0.45초. |
| Hold Duration | 페이드 시작 전 유지 시간. 기본 0.12초. Duration 범위 안으로 제한된다. |
| Glow Duration | Glow 및 크기 팝이 원상복구되는 시간. 기본 0.16초. 전체 페이드의 영향도 받는다. |
| Pop Scale | 표시 순간의 확대 배율. 기본 1.12배. 1이면 확대하지 않는다. |
| Glow Intensity | Glow 최대 알파 배율. 기본 0.65. |
| Glow Color | Glow 색상. |

직접 만든 UI를 연결할 때는 이 컴포넌트가 붙은 오브젝트 아래에 **전용 Feedback Root**를 만들고,
Label/Icon/Glow Image를 그 안에 배치한다. 직접 연결한 Root의 위치·크기는 해당 RectTransform에서 조정한다.
잘못된 부모 연결이나 Icon/Glow 중복 연결은 초기화 시 오류를 출력하고 컴포넌트를 비활성화한다.
설정은 Play 시작 전에 변경한다.

## 연속 반사와 종료 처리

- `Update`에서 반사 상태를 확인하지 않는다. 이벤트 수신 시에만 애니메이션을 시작한다.
- 실행 중 다시 반사되면 시작 시간과 밝기/크기만 초기화한다. 코루틴과 UI를 중첩 생성하지 않는다.
- 애니메이션이 끝나면 코루틴도 종료한다. 시간은 `Time.unscaledTime` 기준이다.
- 게임 성공/실패 시 즉시 숨긴다. 컴포넌트가 비활성화될 때 코루틴을 중단하고 이벤트 구독을 해제한다.

## 할당과 재질

기본 UI 오브젝트, 32×32 Glow 텍스처/스프라이트, 고정 문구와 재사용 IEnumerator는 초기화할 때 준비한다.
재생 중에는 알파와 Transform 값만 변경하며, 매번 텍스트를 다시 조립하거나 `WaitForSeconds`를 생성하지 않는다.
직접 생성한 텍스처/스프라이트는 HUD가 파괴될 때 정리한다. Inspector로 받은 에셋은 파괴하지 않는다.

기존 UI/TMP 재질을 사용하며 `new Material`, `Renderer.material`, `TMP_Text.fontMaterial`로
개별 재질을 만들지 않는다. 단, 유휴 상태에서 `StartCoroutine`을 새로 실행할 때 Unity의 Coroutine 핸들
할당은 남으므로 **반사 이벤트 전체가 GC 0 B라고 주장하지 않는다**. 기존 반사 로그·사운드 등의 할당도 별개다.

## 플레이 확인 항목

컴파일 검증과 실제 Unity 화면 검증은 별개다. 다음 항목을 Play/Windows 빌드에서 확인한다.

1. 반사 성공에만 문구가 표시되고, 일반 피격·벽 반사에는 표시되지 않는지.
2. 0.45초 이내 연속 반사 시 밝기가 다시 올라오고 마지막 반사 후 정상적으로 사라지는지.
3. 표시 중 성공/실패하면 즉시 숨겨지고 Retry 후 다시 작동하는지.
4. 해상도를 바꿔도 HP UI 및 플레이어 시야를 가리지 않는지.
5. 게임 진행 중 컴포넌트 메뉴의 `Preview Reflect (Play Mode)`로 외형을 미리 볼 수 있는지.
6. 할당을 확인할 경우 초기화 프레임과 이벤트 재생 프레임을 구분하고, UI 외 기존 로그/사운드 경로도 구분하는지.

## Player 위치의 LineRenderer Ring

`Player` 프리팹의 `PlayerReflectFeedback`도 동일한 `ReflectSuccess` 이벤트를 구독한다.
반사 위치의 발밑에서 청록색 링 하나가 0.24초 동안 확장·페이드한다.
LineRenderer의 `useWorldSpace`를 사용하고 반사 순간 중심 좌표를 저장하므로 이후 Player 이동을 따라가지 않는다.
링 오브젝트는 Player의 자식으로 유지하며, 연속 반사 시 이전 코루틴을 중단하고 새로 재생한다.
게임 상태 변경·비활성화 시 코루틴을 중단하고 숨긴다. 컴포넌트 파괴 시 링 오브젝트도 정리한다.

| PlayerReflectFeedback 필드 | 역할 |
| --- | --- |
| Ring Material | 전용 URP/Particles/Unlit 투명 재질. sharedMaterial로 사용한다. |
| Local Offset | Player 기준 발생 위치. 기본 `(0, 0.08, 0)`으로 바닥과의 겹침을 줄인다. |
| Ring Color | Ring 색상과 최대 알파. |
| Duration | 입자 수명. 기본 0.24초. |
| Radius | 기준 반지름(월드 단위). 기본 0.45. |
| Line Width | 링 선의 두께. 기본 0.04. 초기화 시 적용한다. |
| Expansion | 수명 종료 시 크기 배율. 시작 배율은 0.55, 종료 기본값은 1.35. |

설정은 Play 전에 조절한다. 실행 중 Player 컴포넌트 메뉴의
`Preview Reflect Ring (Play Mode)`로 미리 볼 수 있다.

Player FBX의 재질 이름은 `c430ddca-ac7d-4704-a0d6-72363db3b54c_basecolor`이며,
대응 재질은 URP/Lit이지만 `_EMISSION` 키워드가 활성화되어 있지 않다.
현재 URP 소스의 `SampleEmission`은 이 키워드가 없으면 0을 반환한다.
MaterialPropertyBlock으로 발광 색상만 바꿔서는 효과가 나지 않으므로, 이번에는 Player 원본 재질을
변경하거나 복제하지 않고 링 연출만 적용했다.

LineRenderer 오브젝트는 초기화 때 한 번 생성하고 재사용한다. 재생 중에는 48개 정점과 색상을 갱신한다.
링 전용 재질은 `sharedMaterial`로 연결하며 Player 재질과 별개다.
간단한 코루틴 구현을 선택했으므로 반사마다 IEnumerator 및 Coroutine 핸들 할당이 발생한다.
매 프레임 새 배열이나 대기 객체를 생성하지는 않는다. 실제 CPU·GC 수치는 아직 Profiler로 측정하지 않았다.

플레이 시 바닥 위 표시, 점프 중 위치, 이동 직후 반사, 빠른 연속 반사, 성공/실패 후 숨김,
Retry 후 재생을 확인한다. 실제 화면 및 Windows 빌드에서의 시각 검증은 별도로 필요하다.
