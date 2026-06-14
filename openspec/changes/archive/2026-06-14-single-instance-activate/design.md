## 컨텍스트 (Context)

현재 SoundSwitcher에는 단일 인스턴스를 강제하는 메커니즘이 없어, 사용자가 앱을 여러 번 실행할 수 있습니다. 기존 요구사항에서는 Mutex만을 사용하여 두 번째 인스턴스를 단순 종료시키는 방향이었으나, 사용자 경험 향상을 위해 두 번째 실행 시 기존에 실행 중인 인스턴스의 윈도우가 화면에 나타나도록(IPC 활용) 로직을 개선해야 합니다.

## 목표 및 제외 대상 (Goals / Non-Goals)

**목표 (Goals):**
- Mutex를 사용하여 SoundSwitcher가 동시에 여러 개 실행되는 것을 방지합니다.
- `System.Threading.EventWaitHandle`을 활용하여 매우 단순한 IPC(프로세스 간 통신)를 구축합니다.
- 두 번째 인스턴스가 실행되려고 할 때, 기존 인스턴스에 신호를 보내어 기존 앱의 윈도우를 활성화(Activate)합니다.
- 프로그램 시작 시 명령줄 인자를 읽고, `/Activate`가 포함되어 있으면 메인 창을 화면 최상단으로 활성화합니다.

**제외 대상 (Non-Goals):**
- Named Pipes나 gRPC 등 무거운 데이터 전송 목적의 복잡한 IPC 도입. 단순히 "활성화(Signal)" 상태만 전달하면 되므로 EventWaitHandle로 충분합니다.

## 결정 사항 (Decisions)

- **단일 인스턴스 Mutex**: 애플리케이션 시작 시(`App_OnStartup`) `System.Threading.Mutex`를 도입합니다.
- **EventWaitHandle (단순 IPC)**: 
  - **수신부 (첫 번째 인스턴스)**: Mutex를 선점한 인스턴스는 `EventWaitHandle`을 생성하고 별도의 Task(백그라운드 스레드)에서 `WaitOne()`을 통해 신호를 대기합니다. 신호가 수신되면 `Dispatcher.Invoke`를 통해 UI 스레드에서 `ShowWithActivate()`를 실행합니다.
  - **송신부 (두 번째 인스턴스)**: Mutex 획득에 실패한 인스턴스는 기존에 생성된 `EventWaitHandle`을 열어 `Set()`을 호출함으로써 활성화 신호를 보내고, 즉시 `Shutdown()`을 호출해 종료합니다.
- **명령줄 인자 처리**: 첫 번째 인스턴스가 시작할 때 `Environment.GetCommandLineArgs()`에 `/Activate`가 포함되어 있다면 시작과 동시에 창을 표시합니다.

## 위험 요소 및 트레이드오프 (Risks / Trade-offs)

- **트레이드오프 (Trade-off)**: 백그라운드 스레드 하나가 앱이 살아있는 동안 무한히 `WaitOne()` 상태로 대기하게 됩니다. 하지만 리소스 점유가 거의 없는 가벼운 작업이므로 데스크톱 앱에서는 무시할 수 있는 수준입니다.
- **위험 요소 (Risk)**: 여러 사용자가 동일 머신에서 실행할 때 Mutex 및 EventWaitHandle 이름의 충돌이 발생할 수 있습니다.
- **완화 방법 (Mitigation)**: Mutex 및 EventWaitHandle의 이름에 전역(Global) 네임스페이스를 사용하거나, 현재 세션(Local)에 한정할지 결정해야 합니다. 기본적으로 로컬 사용자 세션 레벨로 한정하는 것이 안전합니다 (이름에 `Global\` 접두사를 붙이지 않음).
