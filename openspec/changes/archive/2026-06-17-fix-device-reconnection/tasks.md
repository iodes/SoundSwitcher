## 1. 장치 모델 및 열거(Enumeration) 로직 수정

- [x] 1.1 `AudioDeviceModel` (혹은 관련 모델 클래스)에 `IsActive` 프로퍼티(또는 `State`) 추가
- [x] 1.2 `AudioDeviceService.cs`에서 장치 열거 함수 호출 시 필터 플래그를 `DeviceState.Active`에서 `DeviceState.Active | DeviceState.Unplugged | DeviceState.Disabled` 등으로 확장
- [x] 1.3 장치 상태 변경 콜백(`OnDeviceStateChanged` 등)에서 리스트를 갱신할 때 새로 열거된 장치들의 `IsActive` 상태가 올바르게 모델에 반영되도록 로직 수정

## 2. 장치 목록 갱신 및 바인딩 안정화
- [x] 2.1 View/ViewModel에서 장치 목록 갱신 이벤트 수신 시, 리스트 인스턴스를 통째로 교체하지 않고 교체하더라도 기존에 선택된 SelectedItem(또는 SelectedValue)을 유지하도록 동기화 방식 개선 (불연속적 목록 갱신 시 선택 해제되는 버그 수정)
- [x] 2.2 연결된 장치와 연결 끊긴 장치를 식별 가능하도록 갱신 시 `IsActive` 값 올바르게 세팅 검증
## 3. UI 시각적 피드백 구현

- [x] 3.1 콤보박스의 DataTemplate(또는 ItemTemplate) 내에 DataTrigger를 추가하여, Converter를 통해 바인딩된 `IsActive`가 `false`일 때 투명도(`Opacity`)를 0.5로 설정
- [x] 3.2 필요할 경우 비활성화/연결 끊긴 장치를 리스트 하단으로 정렬하는 로직 추가 검토(우선순위 낮음)

## 4. 프로파일 전환 검증 및 스킵 로직

- [x] 4.1 트레이 아이콘이나 프로파일 순환 전환에 해당하는 로직(예: `ProfileService` 또는 관련 커맨드)에서 전환 전 해당 프로파일의 `IsActive` 상태를 검사하도록 수정
- [x] 4.2 대상 프로파일의 장치가 사용할 수 없는 경우 그 다음 유효한 프로파일을 찾아 전환하는 순환 반복 순차 스킵 로직 구현
- [x] 4.3 모든 프로파일이 비활성 상태일 경우의 무한 루프 방지 및 안전 처리 (예: 전환 취소)

## 5. 테스트
- [x] 5.1 장치 물리적 연결 해제 시 목록에 남아있고 반투명해지는지 테스트
- [x] 5.2 장치 물리적 재연결 시 즉각적으로 목록에 불투명하게 활성화되는지 테스트
- [x] 5.3 중간 프로파일의 장치가 해제된 상태에서 순환 전환 시 해당 프로파일을 올바르게 스킵하는지 테스트
