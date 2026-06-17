## 1. 뷰모델 업데이트 (ProfileViewModel)

- [ ] 1.1 출력 장치 속성 창을 여는 Command(`OpenOutputDevicePropertiesCommand`) 추가
- [ ] 1.2 입력 장치 속성 창을 여는 Command(`OpenInputDevicePropertiesCommand`) 추가
- [ ] 1.3 Windows API(`mmsys.cpl` 혹은 COM 인터페이스)를 사용하여 특정 디바이스 ID에 대한 속성 창을 띄우는 로직 구현

## 2. 컨텍스트 메뉴 UI 재구성 (ProfileCard 뷰)

- [ ] 2.1 기존 컨텍스트 메뉴 항목들을 기능별로 그룹화하고 `Separator` 추가 (기본 설정 / 아이콘 / 속성 / 삭제)
- [ ] 2.2 '출력 장치 속성' 메뉴 아이템(MenuItem) 추가 및 `OpenOutputDevicePropertiesCommand` 바인딩
- [ ] 2.3 '입력 장치 속성' 메뉴 아이템(MenuItem) 추가 및 `OpenInputDevicePropertiesCommand` 바인딩
- [ ] 2.4 각 속성 메뉴 아이템에 적절한 아이콘 추가

## 3. 테스트 및 검증

- [ ] 3.1 출력 장치가 있는 경우 '출력 장치 속성' 클릭 시 해당 장치의 네이티브 속성 창이 열리는지 확인
- [ ] 3.2 입력 장치가 있는 경우 '입력 장치 속성' 클릭 시 해당 장치의 네이티브 속성 창이 열리는지 확인
- [ ] 3.3 장치가 할당되지 않은 프로파일에서 해당 속성 메뉴가 비활성화되는지 확인
