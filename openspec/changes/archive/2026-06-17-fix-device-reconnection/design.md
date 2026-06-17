## Context

현재 SoundSwitcher 앱은 오디오 장치 목록을 `DeviceState.Active` 상태인 장치만 필터링하여 보여주고 있습니다. 또한 장치가 연결 해제(Unplugged)되거나 비활성화(Disabled)되었다가 다시 활성화(Active)될 때 UI의 장치 목록(ComboBox)에 제대로 반영되지 않는 버그가 있습니다. 사용자는 연결이 끊긴 장치라도 프로파일에 어떤 장치가 설정되어 있는지 확인하고 관리할 수 있어야 합니다.

## Goals / Non-Goals

**Goals:**
- 연결이 해제되었거나(Unplugged), 비활성화된(Disabled) 디바이스도 장치 목록에 포함하여 표시합니다.
- 상태가 Active가 아닌 장치들은 UI(ComboBox 항목 등)에서 반투명하게(Opacity 적용) 표시하여 시각적으로 구분합니다.
- 장치 재연결 시(상태 변경 시) 콤보박스 목록 갱신 버그를 수정하여 새 상태가 정상적으로 UI에 반영되게 합니다.
- 장치 전환 시 대상 프로파일의 장치가 사용할 수 없는 상태(Active가 아님)라면, 전환을 취소하거나 유효한 다음 프로파일로 스킵하는 검증 로직을 추가합니다.

**Non-Goals:**
- 완전히 시스템에서 제거된(Uninstall) 장치의 캐시 보존 및 관리. (레지스트리/시스템에서 제거된 장치는 제외)

## Decisions

1. **디바이스 열거(Enumeration) 플래그 변경**
   - 현재 `DeviceState.Active`만 가져오는 로직을 `DeviceState.Active | DeviceState.Unplugged | DeviceState.Disabled`를 포함하도록 수정합니다.
   - `AudioDeviceModel` (또는 해당 모델)에 `IsActive` (또는 `State`) 프로퍼티를 추가하여, View에서 해당 디바이스가 현재 활성화된 상태인지 판단할 수 있게 합니다.

2. **콤보박스 아이템 UI (DataTemplate) 변경**
   - WPF의 `ComboBox.ItemTemplate`을 수정하여, 디바이스의 활성화 상태에 따라 투명도(Opacity)를 조절하는 DataTrigger 혹은 컨버터를 추가합니다. (예: `IsActive == false` 일 때 `Opacity="0.5"`)

3. **장치 상태 갱신 이벤트 처리(버그 수정)**
   - 장치의 `OnDeviceStateChanged`, `OnDeviceAdded`, `OnDeviceRemoved` 콜백이 호출될 때 기존 리스트를 갱신하는 로직을 확인합니다.
   - `ObservableCollection` 갱신 시 기존 아이템의 인스턴스가 교체되면서 SelectedItem 바인딩이 풀리거나, 프로파일의 장치 ID와 매칭이 어긋나는 문제가 있는지 점검하고, ID 기반으로 동기화하도록 수정합니다. 혹은 디바이스 상태 속성만 업데이트 하도록 수정합니다.

4. **프로파일 전환 검증 로직 구현**
   - 트레이 아이콘이나 핫키 등을 통해 프로파일을 순환/전환할 때, 다음 프로파일의 장치들(재생/녹음)의 `IsActive` 상태를 검사합니다.
   - 장치가 연결되지 않았거나 사용할 수 없는 상태(`IsActive == false`)라면, 해당 프로파일을 건너뛰고 그 다음 유효한 프로파일로 전환(Skip)하도록 로직을 구현합니다. 만약 유효한 프로파일이 하나도 없다면 전환을 취소하고 알림(Notification)으로 안내하거나 아무 동작도 하지 않습니다.

## Risks / Trade-offs

- **[Risk]** 사용하지 않는 장치(비활성화된 모니터 스피커 등)가 너무 많이 리스트에 표시되어 목록이 지저분해질 수 있음.
  → **Mitigation**: 반투명 처리 등을 통해 시각적 우선순위를 낮추고, 목록 렌더링 시 Active 장치를 상단에 정렬하거나 명확히 구분되게 할 수 있습니다. 우선은 반투명 처리로 구분합니다.
- **[Risk]** 콤보박스 아이템 교체 시 바인딩 문제로 SelectedItem이 null이 되는 이슈.
  → **Mitigation**: 리스트 갱신 시 기존 선택된 장치의 ID를 캐싱하고 갱신 후 재할당(또는 SelectedValuePath="Id" 활용)하여 선택 상태를 유지합니다.
