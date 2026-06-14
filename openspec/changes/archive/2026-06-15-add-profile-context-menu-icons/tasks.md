## 1. 아이콘 리소스 확인 및 선정

- [x] 1.1 `ChangeIcon` (아이콘 변경), `ResetIcon` (기본 아이콘으로 초기화), `Delete` (삭제) 에 알맞은 `ui:FontIcon` 혹은 `ui:SymbolIcon` 선정

## 2. DevicesPage.xaml 수정

- [x] 2.1 `DevicesPage.xaml` 파일의 `Profile List` 템플릿 내 `ContextMenu` 영역 찾기
- [x] 2.2 `ChangeIconCommand` MenuItem 에 `<MenuItem.Icon>` 추가
- [x] 2.3 `ResetIconCommand` MenuItem 에 `<MenuItem.Icon>` 추가
- [x] 2.4 `DeleteCommand` MenuItem 에 `<MenuItem.Icon>` 추가

## 3. UI 검증

- [x] 3.1 런타임에서 프로파일 카드 우클릭 시 컨텍스트 메뉴에 아이콘이 정상적으로 나타나는지 확인
- [x] 3.2 아이콘과 텍스트의 정렬 및 여백이 적절한지 검토
