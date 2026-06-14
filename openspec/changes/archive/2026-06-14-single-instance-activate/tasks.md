## 1. 애플리케이션 시작 로직 및 Mutex 설정

- [x] 1.1 `App.xaml.cs`의 `App` 클래스에 `Mutex` 필드를 추가합니다.
- [x] 1.2 `App_OnStartup`에서 `new Mutex(true, "SoundSwitcher.App.Mutex", out bool createdNew)`를 사용하여 Mutex를 초기화합니다.
- [x] 1.3 `!createdNew` 조건문을 추가하여 중복 실행 여부를 분기 처리합니다.

## 2. EventWaitHandle 기반 단순 IPC 구현

- [x] 2.1 기존 인스턴스(첫 번째 인스턴스)를 위해 `EventWaitHandle`을 생성(`EventResetMode.AutoReset`)합니다.
- [x] 2.2 첫 번째 인스턴스에서 백그라운드 `Task`를 실행하고 `EventWaitHandle.WaitOne()`으로 신호를 무한 대기합니다.
- [x] 2.3 `WaitOne()` 신호가 발생하면 `Current.Dispatcher.Invoke(() => ShowWithActivate())`를 호출하도록 리스너 로직을 구성합니다.
- [x] 2.4 두 번째 인스턴스(`!createdNew` 분기)에서 `EventWaitHandle.TryOpenExisting`으로 기존 핸들을 가져옵니다.
- [x] 2.5 두 번째 인스턴스에서 `Set()`을 호출하여 활성화 신호를 보낸 직후 `Current.Shutdown()`으로 종료합니다.

## 3. 명령줄 인자 처리 로직

- [x] 3.1 첫 번째 인스턴스가 시작될 때 `Environment.GetCommandLineArgs()`를 파싱합니다.
- [x] 3.2 인자 배열이 `Contains("/Activate", StringComparer.OrdinalIgnoreCase)`를 만족하는지 검사합니다.
- [x] 3.3 조건 만족 시 `ShowWithActivate()` 메서드를 호출하여 즉시 창을 띄웁니다.
