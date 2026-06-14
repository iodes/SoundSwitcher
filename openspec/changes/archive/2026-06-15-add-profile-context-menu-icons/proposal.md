## Why

프로파일 관리 메뉴에서 우클릭 시 나타나는 컨텍스트 메뉴 항목들에 아이콘이 없어 시각적인 직관성이 떨어집니다. 각 기능에 알맞은 아이콘을 추가하여 사용자가 원하는 작업을 더 빠르고 명확하게 식별할 수 있도록 사용자 경험(UX)을 개선하고자 합니다.

## What Changes

- 프로파일 관리 메뉴의 우클릭 컨텍스트 메뉴(Context Menu) 항목들에 적절한 아이콘 추가 (예: 수정, 삭제 등)
- 윈도우 기본 아이콘(SymbolIcon) 또는 앱 내 리소스를 활용하여 일관성 있는 디자인 적용

## Capabilities

### New Capabilities

### Modified Capabilities
- `profile-management`: 프로파일 관리 컨텍스트 메뉴의 시각적 요소(아이콘) 추가

## Impact

- 프로파일 관리 화면의 UI를 정의하는 XAML 파일
- 관련된 뷰 모델 또는 코드 비하인드 (필요시)
