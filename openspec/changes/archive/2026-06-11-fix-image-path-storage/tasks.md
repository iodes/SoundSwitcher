## 1. 캐시 서비스(Cache Service) 업데이트

- [x] 1.1 `IconCacheService.CacheIcon`이 파일의 절대 경로 대신 생성된 파일명만 반환하도록 수정
- [x] 1.2 저장된 파일명을 캐시 디렉터리의 실제 절대 경로로 변환해 주는 `IconCacheService.GetIconFullPath(string filename)` 메서드 추가 (파일 유실 시 `null`을 반환해 기본 이미지 폴백 유도)
- [x] 1.3 `IconCacheService.DeleteIcon` 메서드 내부에서 파일 삭제를 시도하기 전에 `GetIconFullPath`를 사용해 절대 경로를 구성하도록 수정

## 2. 뷰모델 및 앱 로직 업데이트

- [x] 2.1 `DeviceProfileViewModel.IconPath`를 사용할 때(이미지 로딩이나 존재 여부 확인 시) `IconCacheService.GetIconFullPath`를 거치도록 수정
- [x] 2.2 `DeviceProfileViewModel`의 "기본 아이콘으로 초기화" 동작 시, 파일명이 올바르게 `IconCacheService.DeleteIcon`으로 전달되어 캐시 파일이 지워지는지 검증
- [x] 2.3 `MainViewModel`의 "프로파일 삭제" 로직에서 프로파일 삭제 시 파일명이 `IconCacheService.DeleteIcon`으로 제대로 전달되어 캐시가 지워지는지 검증
- [x] 2.4 `App.xaml.cs`의 트레이 아이콘 로직에서 `BitmapImage`를 로드하기 전 `IconPath`를 절대 경로로 올바르게 해석하도록 수정
