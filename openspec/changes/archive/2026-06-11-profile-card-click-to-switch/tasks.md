## 1. ViewModel 로직 추가

- [x] 1.1 `DeviceProfileViewModel`에 프로파일 수동 적용을 위한 `ApplyCommand` (ICommand) 추가 및 초기화
- [x] 1.2 `ApplyCommand` 내부 로직에 `DeviceApplyRequested?.Invoke(this)` 호출 추가

## 2. View 수정 및 이벤트 바인딩

- [x] 2.1 `DevicesPage.xaml` 파일의 `ProfileBorder` (최상위 DataTemplate 요소)에 `MouseLeftButtonUp="ProfileBorder_MouseLeftButtonUp"` 이벤트 바인딩 추가

## 3. View 코드 비하인드 이벤트 핸들링

- [x] 3.1 `DevicesPage.xaml.cs`에 `ProfileBorder_MouseLeftButtonUp` 이벤트 핸들러 메서드 작성
- [x] 3.2 `e.OriginalSource`와 `VisualTreeHelper`를 사용하여 콤보박스, 드래그 그립(`IsReorderGrip="True"`), 아이콘 영역 등에서 발생한 이벤트인지 확인 후 필터링
- [x] 3.3 조건에 맞는 순수한 카드 여백 클릭 시, DataContext를 `DeviceProfileViewModel`로 캐스팅하여 `ApplyCommand` 실행
