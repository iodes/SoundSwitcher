## Context

현재 SoundSwitcher에서 장치 프로파일을 전환하려면 트레이 아이콘의 컨텍스트 메뉴를 사용해야 합니다. 사용자가 메인 윈도우의 장치 탭에서 프로파일 카드 자체를 직접 좌클릭하여 해당 프로파일을 빠르게 활성화(전환)할 수 있도록 상호작용 개선이 필요합니다.

## Goals / Non-Goals

**Goals:**
- 프로파일 카드의 빈 영역(일반 영역)을 좌클릭하여 해당 프로파일 활성화(전환) 수행
- 아이콘 변경(아이콘 영역 클릭), 연결 장치 변경(콤보박스 클릭), 카드 재정렬(좌측 드래그 핸들) 등 기존 내부 요소의 조작과 충돌하지 않도록 처리

**Non-Goals:**
- 키보드 단축키나 트레이 이외의 전역 활성화 수단 추가는 제외

## Decisions

**1. 클릭 이벤트 감지 및 처리 방식**
- **결정:** `DevicesPage.xaml`의 각 프로파일 카드를 감싸는 최상위 `Border`에 `MouseLeftButtonUp` 이벤트 핸들러를 연결합니다.
- **이유:** `MouseLeftButtonDown`보다 `MouseLeftButtonUp`이 사용자 상호작용(드래그 등)을 방해할 확률이 적습니다. 코드 비하인드에서 처리하여 `e.OriginalSource` 등을 확인하고 내부 컨트롤 조작을 효과적으로 필터링할 수 있습니다.
- **대안:** `InputBinding`을 통한 Command 연결도 가능하지만, 콤보박스나 드래그 핸들 등 하위 요소 클릭을 걸러내는 데 한계가 있어 라우티드 이벤트 핸들링이 적합합니다.

**2. 하위 요소 클릭 시 프로파일 전환(버블링) 차단**
- **결정:** `MouseLeftButtonUp` 이벤트 핸들러 내부에서 `e.OriginalSource`를 확인하여 콤보박스(ComboBox), 드래그 핸들(IsReorderGrip 부착 요소), 아이콘 변경 영역(Grid/Border)에서 발생한 이벤트일 경우 무시(또는 해당 컨트롤에서 `e.Handled = true` 선언)하도록 구현합니다.
- **이유:** 사용자가 장치를 변경하거나 순서를 바꿀 때 의도치 않게 프로파일이 활성화되는 오작동을 방지합니다.

**3. 활성화 로직 호출 경로**
- **결정:** `DeviceProfileViewModel`에 프로파일을 수동으로 적용할 수 있도록 `ActivateProfileCommand` (또는 `ApplyCommand`)를 추가합니다. 이 명령이 실행되면 기존 장치 변경 시와 동일하게 `DeviceApplyRequested` 이벤트를 발생시켜 `MainViewModel`에서 실제 오디오 서비스를 통해 장치를 전환하도록 합니다.

## Risks / Trade-offs

- [Risk] 드래그 그립이나 아이콘 조작 후 마우스 버튼을 놓을 때 클릭으로 인식되어 프로파일 전환이 발생할 우려가 있음.
  → **Mitigation:** 이벤트 핸들러 내에서 `VisualTreeHelper` 등을 이용해 클릭한 요소의 조상 중 `IsReorderGrip`이 속한 영역이거나 아이콘 영역인지 확인하여, 조건에 맞을 경우 실행을 건너뛰도록 처리합니다.
