## Why

현재 핫키를 통해 프로파일을 전환할 때만 알림이 표시되고 메인 메뉴에서 수동으로 클릭하여 전환할 때는 알림이 표시되지 않아 사용자 경험에 일관성이 부족합니다. 또한 프로파일 변경 알림을 원하지 않는 사용자를 위해 알림 표시 여부를 설정할 수 있는 옵션이 필요합니다.

## What Changes

- 프로파일 변경 알림 로직을 공통화하여 핫키 및 메인 메뉴 UI를 통한 전환 시 모두 일관되게 동작하도록 변경합니다.
- 메인 메뉴에서 프로파일 카드를 클릭하여 전환할 때 공통 알림 로직을 호출합니다.
- 설정 메뉴에 "프로파일 변경 알림 표시" 옵션(토글)을 추가합니다.
- 알림 설정 상태에 따라 알림 표시 여부를 결정하도록 조건부 처리를 추가합니다.

## Capabilities

### New Capabilities
<!-- Capabilities being introduced. Replace <name> with kebab-case identifier (e.g., user-auth, data-export, api-rate-limiting). Each creates specs/<name>/spec.md -->
(없음)

### Modified Capabilities
<!-- Existing capabilities whose REQUIREMENTS are changing (not just implementation).
     Only list here if spec-level behavior changes. Each needs a delta spec file.
     Use existing spec names from openspec/specs/. Leave empty if no requirement changes. -->
- `profile-management`: 프로파일 전환 시 알림 표시 공통화 및 알림 On/Off 설정 추가

## Impact

- 알림 표시 로직이 특정 이벤트 핸들러에서 공통 기능으로 통합됩니다.
- 사용자 설정 데이터 모델에 새로운 알림 설정 값이 추가되며 UI가 업데이트됩니다.
- 프로파일 선택 이벤트 처리 로직이 수정됩니다.
