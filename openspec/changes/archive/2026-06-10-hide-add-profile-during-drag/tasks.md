## 1. 뷰모델 업데이트 (State Management)

- [x] 1.1 `DevicesViewModel.cs`에 드래그 상태를 나타내는 `IsReordering` (또는 `IsDragging`) `bool` 속성 추가 (INotifyPropertyChanged 지원)

## 2. 상태 동기화 및 바인딩 (UI Behavior)

- [x] 2.1 드래그 앤 드롭 시작 시 `DevicesViewModel`의 `IsReordering`을 `true`로 설정하는 로직 추가 (`LiveReorderBehavior.cs` 혹은 관련 이벤트 핸들러)
- [x] 2.2 드래그 앤 드롭 종료(Drop, Cancel 등) 시 `DevicesViewModel`의 `IsReordering`을 `false`로 복구하는 로직 추가 (비정상 종료 시에도 안전하게 복구되도록 방어 코드 작성)

## 3. UI 렌더링 업데이트 (XAML)

- [x] 3.1 `DevicesPage.xaml` 하단의 '새 프로파일 추가' `Button`의 `Visibility` 속성을 뷰모델의 `IsReordering` 속성에 바인딩 (기본 제공되거나 새로 작성된 `BooleanToVisibilityConverter` 혹은 DataTrigger 사용)

## 4. 검증

- [x] 4.1 앱 실행 후 프로파일 핸들을 잡고 드래그 앤 드롭을 시도하여 '새 프로파일 추가' 버튼이 즉시 사라지는지 확인
- [x] 4.2 드래그를 정상적으로 드롭하여 위치를 변경하거나 도중에 취소했을 때 '새 프로파일 추가' 버튼이 다시 나타나는지 확인
