## Context

SoundSwitcher 애플리케이션의 장치 탭은 오디오 프로파일 카드들을 스택 형태로 나열하며, 사용자는 각 프로파일 카드의 그립을 잡아 드래그 앤 드롭으로 순서를 변경할 수 있습니다.
현재 구현에서는 프로파일 목록 하단에 '새 프로파일 추가' 버튼이 상시 표시되어 있습니다. 그러나 드래그 중에도 이 버튼이 노출되어 있으면 사용자가 드롭 영역을 판단할 때 시각적으로 혼동을 줄 수 있습니다. 따라서 드래그 앤 드롭이 진행되는 동안에는 추가 버튼을 숨기고 동작이 완료되면 다시 표시해야 합니다.

## Goals / Non-Goals

**Goals:**
- 프로파일 드래그가 시작될 때 하단의 '새 프로파일 추가' 버튼을 숨깁니다.
- 드래그 동작이 종료될 때(드롭 완료, 취소 등) 버튼을 다시 표시합니다.
- MVVM 패턴에 맞춰 뷰와 뷰모델 간 상태 동기화를 구현합니다.

**Non-Goals:**
- 기존 드래그 앤 드롭 구현체(Gong WPF DragDrop, Custom Behavior 등)의 교체.
- 프로파일 추가 로직 자체의 변경.

## Decisions

**1. 드래그 상태 관리 (State Management)**
- **Decision:** `DevicesViewModel`에 `IsReordering`(또는 `IsDragging`)이라는 `bool` 속성을 추가하고, 드래그 시작 시 `true`, 종료 시 `false`로 상태를 업데이트합니다.
- **Rationale:** 뷰모델을 통해 상태를 노출하면 XAML 바인딩을 통해 관련 UI(버튼 등)를 일관되게 제어할 수 있으며, 향후 다른 UI 요소에도 쉽게 확장 적용할 수 있습니다.
- **Alternatives Considered:** XAML 단에서 Code-behind 이벤트 핸들러만으로 버튼의 `Visibility`를 조작하는 방식. 구현은 빠를 수 있으나 MVVM 아키텍처 규칙과 응집도 측면에서 뷰모델을 활용하는 것이 유리합니다.

**2. UI 바인딩 및 애니메이션/숨김 처리**
- **Decision:** `DevicesPage.xaml`에 위치한 '새 프로파일 추가' `Button`의 `Visibility` 속성을 `IsReordering` 속성과 바인딩하여 제어합니다 (필요 시 `BooleanToVisibilityConverter`의 Invert 활용 혹은 DataTrigger 사용).
- **Rationale:** 구조가 단순하고 코드 변경을 최소화할 수 있습니다.

## Risks / Trade-offs

- **[Risk] 비정상적인 드래그 종료 시 버튼이 다시 나타나지 않음** → **Mitigation:** 마우스 이탈, 예외 발생, ESC 키를 통한 취소 등 발생 가능한 모든 드래그 종료/취소 이벤트 훅(Drop, DragCancelled 등)에서 상태 변수를 안전하게 `false`로 리셋하도록 보장합니다.
