## Context

현재 `DevicesPage.xaml`의 `Profile List` 부분에 정의된 우클릭 컨텍스트 메뉴(ContextMenu)에는 텍스트만 표시되고 있어 시각적 구분이 명확하지 않습니다. 각 메뉴 항목(예: 프로파일 이름 변경, 삭제, 복제, 기본값 설정 등)에 적절한 아이콘을 추가하면, 사용자가 빠르게 기능을 식별할 수 있어 사용성 향상에 도움이 됩니다.

## Goals / Non-Goals

**Goals:**
- `DevicesPage.xaml` 내의 프로파일 컨텍스트 메뉴 항목에 적합한 아이콘(SymbolIcon 또는 폰트 아이콘 등) 추가
- 메뉴의 시각적 일관성 유지 (크기, 여백 조정)

**Non-Goals:**
- 아이콘 추가 이외의 컨텍스트 메뉴 동작 로직 변경
- 다른 화면의 컨텍스트 메뉴 수정 (이번 변경 범위에 포함되지 않는 경우)

## Decisions

- **아이콘 리소스:** Fluent Design 가이드라인에 맞춰 WPF 내장 혹은 프로젝트에서 사용 중인 폰트 아이콘(`Segoe Fluent Icons` 등)을 사용합니다. 이를 통해 앱의 다른 요소들과 시각적 일관성을 유지할 수 있습니다.
- **아이콘 위치:** `MenuItem.Icon` 속성을 사용하여 텍스트 좌측에 아이콘을 배치합니다.

## Risks / Trade-offs

- **[Risk]** 일부 윈도우 버전에 폰트 아이콘(Segoe Fluent Icons)이 누락되어 있을 가능성
  - **Mitigation:** Fallback 폰트를 설정하거나, 프로젝트 내 공용 리소스 딕셔너리에 정의된 아이콘 폰트 계열을 사용하도록 지정합니다.
