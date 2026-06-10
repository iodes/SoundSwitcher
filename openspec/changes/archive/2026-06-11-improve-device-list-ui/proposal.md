## Why

현재 프로파일 설정 UI에서 장치 목록이 정렬되지 않아 사용자가 원하는 장치를 찾기 어렵고, 긴 장치 이름이 콤보박스에서 잘려 보이거나 말줄임 처리가 되지 않아 가독성이 떨어집니다. 또한 콤보박스를 펼쳤을 때 장치 아이콘이 보이지 않아 시각적인 장치 식별이 어렵습니다. 이러한 UI/UX 문제를 개선하여 사용자 편의성을 높이고자 합니다.

## What Changes

- 장치 콤보박스의 장치 목록을 알파벳순(ABC 순)으로 정렬합니다.
- 장치 콤보박스에서 선택된 항목의 이름이 길 경우 '...'으로 말줄임(TextTrimming) 처리되도록 수정합니다.
- 장치 콤보박스를 클릭하여 펼친 드롭다운 목록(Popup) 내의 각 장치 항목에 장치 아이콘이 표시되도록 개선합니다.

## Capabilities

### New Capabilities
<!-- Capabilities being introduced. Replace <name> with kebab-case identifier (e.g., user-auth, data-export, api-rate-limiting). Each creates specs/<name>/spec.md -->

### Modified Capabilities
<!-- Existing capabilities whose REQUIREMENTS are changing (not just implementation).
     Only list here if spec-level behavior changes. Each needs a delta spec file.
     Use existing spec names from openspec/specs/. Leave empty if no requirement changes. -->
- `profile-management`: 프로파일 카드 내 장치 콤보박스 UI 표시 방식 및 정렬 요구사항 변경

## Impact

- `SoundSwitcher` UI 프로젝트 내의 장치 콤보박스 관련 XAML 뷰 및 데이터 바인딩, 정렬 로직 (ViewModel 또는 View 코드)
- 기존 프로파일 관리 화면의 사용자 경험 향상
