# CẨM NANG KỸ THUẬT: EVENT-DRIVEN ARCHITECTURE VÀ FINITE STATE MACHINE TRONG LẬP TRÌNH GAME

> **Tài liệu tham chiếu kỹ thuật khách quan dành cho Game Engineer**  
> *Chủ đề: Bản chất khoa học máy tính, cơ chế hoạt động, so sánh đa chiều và chuẩn mực kiến trúc.*

---

## MỤC LỤC
1. [Cơ sở Lý thuyết: Khái niệm & Bài toán Cốt lõi](#1-cơ-sở-lý-thuyết-khái-niệm--bài-toán-cốt-lõi)
2. [Bản chất Kiến trúc Hướng Sự Kiện (Event-Driven Architecture - EDA)](#2-bản-chất-kiến-trúc-hướng-sự-kiện-event-driven-architecture---eda)
3. [Bản chất Máy Trạng Thái Hữu Hạn (Finite State Machine - FSM)](#3-bản-chất-máy-trạng-thái-hữu-hạn-finite-state-machine---fsm)
4. [Sự Kết hợp: Event-Driven Finite State Machine (ED-FSM)](#4-sự-kết-hợp-event-driven-finite-state-machine-ed-fsm)
5. [Đánh giá Kỹ thuật & Bảng So sánh Khách quan](#5-đánh-giá-kỹ-thuật--bảng-so-sánh-khách-quan)
6. [Các Cạm bẫy Kỹ thuật (Anti-Patterns) & Chuẩn mực Thực thi (Best Practices)](#6-các-cạm-bẫy-kỹ-thuật-anti-patterns--chuẩn-mực-thực-thi-best-practices)

---

## 1. CƠ SỞ LÝ THUYẾT: KHÁI NIỆM & BÀI TOÁN CỐT LÕI

Trong kỹ thuật phần mềm nói chung và phát triển game nói riêng, hai thách thức lớn nhất luôn là:
1. **Kiểm soát dòng dữ liệu (Control Flow):** Làm sao để các hệ thống giao tiếp mà không tạo ra sự phụ thuộc chéo (Coupling)?
2. **Kiểm soát trạng thái thực thể (State Management):** Làm sao để đảm bảo thực thể luôn ở trạng thái hợp lệ, triệt tiêu các điều kiện tranh chấp (Race conditions) và sự xung đột logic?

Để giải quyết bài toán này, các kỹ sư thường đối chiếu hai trường phái tiếp cận:

```
                  ┌───────────────────────────────┐
                  │    GAME LOOP (Mỗi khung hình) │
                  └──────────────┬────────────────┘
                                 │
         ┌───────────────────────┴───────────────────────┐
         ▼                                               ▼
┌──────────────────────────────┐        ┌──────────────────────────────┐
│  MÔ HÌNH THĂM DÒ (POLLING)   │        │ MÔ HÌNH HƯỚNG SỰ KIỆN (EDA)  │
├──────────────────────────────┤        ├──────────────────────────────┤
│ - Chủ động kiểm tra liên tục │        │ - Ngồi im nhàn rỗi (Passive) │
│ - "Có biến gì mới chưa?"     │        │ - "Chỉ hành động khi có còi" │
│ - Phụ thuộc vào tần số Tick  │        │ - Độc lập với tần số Tick    │
└──────────────────────────────┘        └──────────────────────────────┘
```

---

## 2. BẢN CHẤT KIẾN TRÚC HƯỚNG SỰ KIỆN (EVENT-DRIVEN ARCHITECTURE - EDA)

### 2.1. Định nghĩa & Thành phần Cốt lõi
**Event-Driven Architecture (EDA)** là mô hình thiết kế phần mềm trong đó dòng điều khiển của chương trình được quyết định bởi việc phát sinh và tiếp nhận các **Sự kiện (Events)**.

Một kiến trúc EDA chuẩn gồm 3 thành phần chính:
1. **Event Producer (Bên phát sự kiện):** Nơi phát hiện biến động (ví dụ: Driver bàn phím, Collider va chạm vật lý, Timer đếm giờ). Producer phát thông điệp và **hoàn toàn không biết ai sẽ nhận nó**.
2. **Event Channel / Bus (Kênh trung chuyển):** Hạ tầng chuyển giao thông điệp (trong C# thường là `event Action`, `delegate`, hoặc `MessageBus`).
3. **Event Consumer / Subscriber (Bên tiêu thụ sự kiện):** Lắng nghe kênh trung chuyển và kích hoạt hành vi tương ứng khi sự kiện xuất hiện.

```
[ Event Producer ] ──(Bắn Event)──> [ Event Channel / Broker ] ──(Chuyển giao)──> [ Event Consumer ]
```

### 2.2. Các biến thể phổ biến trong Game Engine
* **Observer Pattern (Khớp nối trực tiếp):** Subject giữ danh sách Observer và gọi hàm khi có biến động.
* **C# Native Events / Delegates:** Cơ chế nội tại của ngôn ngữ, tốc độ cao, gọi hàm trực tiếp theo con trỏ hàm (Function pointer), không sinh rác bộ nhớ nếu quản lý đúng.
* **Event Bus (Hàng đợi sự kiện toàn cục):** Một tổng đài tĩnh (`EventBus.Publish<T>()`). Cho phép hai module cách xa nhau ngàn dặm giao tiếp mà không cần giữ reference của nhau.

---

## 3. BẢN CHẤT MÁY TRẠNG THÁI HỮU HẠN (FINITE STATE MACHINE - FSM)

### 3.1. Cơ sở Toán học
Theo lý thuyết ô-tô-mát (Automata Theory), một **Finite State Machine** được định nghĩa là một bộ 5 thành phần:
$$M = (S, \Sigma, \delta, s_0, F)$$

Trong đó:
* $S$: Tập hợp hữu hạn các trạng thái khả dĩ (Finite set of states).
* $\Sigma$: Bảng chữ cái đầu vào / Tập các tín hiệu kích hoạt (Input alphabet / Events).
* $\delta$: Hàm chuyển đổi trạng thái: $\delta: S \times \Sigma \to S$.
* $s_0$: Trạng thái bắt đầu ($s_0 \in S$).
* $F$: Tập trạng thái kết thúc ($F \subseteq S$).

> **Ý nghĩa cốt tử:** Một thực thể tại một thời điểm $t$ **chỉ được phép tồn tại ở đúng một trạng thái $s \in S$**. Nó không thể vừa $A$ vừa $B$.

### 3.2. Ba cách cài đặt FSM trong lập trình

#### Cách 1: Enum kết hợp Switch-Case (Cơ bản)
* **Nguyên lý:** Dùng một biến `Enum CurrentState` và một switch-case khổng lồ trong hàm Update.
* **Ưu điểm:** Viết nhanh, trực quan với các hệ thống cực nhỏ (2–3 trạng thái).
* **Nhược điểm:** Vi phạm nghiêm trọng nguyên lý Open/Closed (SOLID). Càng mở rộng, file code càng phình to đến mức không thể bảo trì.

#### Cách 2: Bảng chuyển đổi trạng thái (Transition Table / State-Matrix)
* **Nguyên lý:** Tách bảng chuyển trạng thái thành dữ liệu thuần túy (Ma trận $S \times \Sigma$).
* **Ưu điểm:** Thời gian tra cứu chuyển trạng thái đạt $O(1)$. Logic chuyển đổi có thể đọc từ file JSON/ScriptableObject mà không cần sửa code.
* **Nhược điểm:** Khó gắn các logic nội bộ phức tạp diễn ra xuyên suốt vòng đời của trạng thái.

#### Cách 3: Gang of Four (GoF) State Pattern (Hướng đối tượng)
* **Nguyên lý:** Mỗi trạng thái được đóng gói thành một Class riêng biệt kế thừa từ một interface hoặc abstract class chung (`IState` / `BaseState`).
* **Vòng đời chuẩn của một State Class:**
  * `Enter()`: Khởi tạo dữ liệu, kích hoạt hoạt ảnh, cấp phát tài nguyên khi bước vào trạng thái.
  * `Exit()`: Dọn dẹp tài nguyên, tắt hiệu ứng khi rời khỏi trạng thái.
  * `Tick() / Update()`: Thực thi logic lặp lại mỗi khung hình.
* **Ưu điểm:** Phân tách trách nhiệm hoàn hảo (Single Responsibility). Mỗi lập trình viên có thể code một State mà không sợ chạm vào file của người khác.

---

## 4. SỰ KẾT HỢP: EVENT-DRIVEN FINITE STATE MACHINE (ED-FSM)

Khi đặt EDA và FSM cạnh nhau, chúng tạo ra một mô hình lai hoàn chỉnh: **Máy trạng thái được kích hoạt và chuyển đổi thuần túy thông qua Sự kiện**.

### 4.1. Kiến trúc phân lớp chuẩn mực

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. INPUT HARDWARE & OS LEVEL (Tầng Phần Cứng)                   │
└───────────────────────────────┬─────────────────────────────────┘
                                │ (Gửi tín hiệu Interrupt)
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│ 2. INPUT ABSTRACTION / EVENT PRODUCER (Bộ Chuyển Đổi Sự Kiện)   │
│    - Bắt tín hiệu phần cứng.                                    │
│    - Phát tán C# Event: JumpPressed, AttackReleased...          │
└───────────────────────────────┬─────────────────────────────────┘
                                │ (Publish Event)
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│ 3. CONTEXT / STATE MACHINE DISPATCHER (Trọng Tài Điều Phối)     │
│    - Nắm giữ con trỏ `CurrentState`.                            │
│    - Lắng nghe Event từ tầng 2.                                 │
│    - Chuyển tiếp Event vào `CurrentState.HandleEvent()`.        │
└───────────────────────────────┬─────────────────────────────────┘
                                │ (Invoke Transition)
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│ 4. CONCRETE STATES (Tầng Hành Vi Độc Lập)                       │
│    - State A: Tiêu thụ sự kiện -> Kích hoạt ChangeState(State B)│
│    - State B: Bỏ qua sự kiện nếu không hợp lệ.                  │
└─────────────────────────────────────────────────────────────────┘
```

### 4.2. Cơ chế Input Buffering (Bộ đệm lệnh) trên nền tảng Event
Trong game hành động, người chơi thường bấm nút sớm hơn vài frame trước khi nhân vật kết thúc hoạt ảnh hiện tại. EDA giải quyết việc này thông qua **Hàng đợi Sự kiện có Thời Gian Sống (Timestamped Event Buffer)**:

1. Khi Event được phát ra, nó được đẩy vào một hàng đợi nhỏ kèm mốc thời gian: `EventBuffer.Push(EventType.Attack, Time.time)`.
2. State hiện tại khi chạm tới "Cửa sổ hủy đòn" (Cancel Window / Recovery Phase) sẽ kiểm tra hàng đợi:
   $$\text{CurrentTime} - \text{EventTimestamp} \le \text{BufferThreshold}$$
3. Nếu hợp lệ, State tự động chuyển đổi sang State kế tiếp mà không yêu cầu người chơi phải bấm lại đúng vào frame đó.

---

## 5. ĐÁNH GIÁ KỸ THUẬT & BẢNG SO SÁNH KHÁCH QUAN

Dưới góc độ Kỹ nghệ Phần mềm, không có kiến trúc nào là "viên đạn bạc" (No Silver Bullet). Mọi kiến trúc đều là sự đánh đổi (Trade-off).

### 5.1. Bảng so sánh các mô hình điều khiển hành vi phổ biến

| Tiêu chí | Polling Monolith (Script 1 cục) | **Event-Driven FSM (Mô hình này)** | Behavior Tree (Cây hành vi) | ECS / Data-Oriented (Mô hình Dữ liệu) |
| :--- | :--- | :--- | :--- | :--- |
| **Độ trễ phản hồi (Latency)** | Cao (Chờ vòng lặp `Update` kế tiếp) | **Cực thấp (Phản hồi tức thì qua Callbacks)** | Trung bình (Duyệt cây từ Root mỗi chu kỳ) | Thấp (Xử lý theo mảng bộ nhớ) |
| **Kiểm soát tính xung đột** | Kém (Dễ tranh chấp cờ boolean) | **Tuyệt đối (Mỗi thời điểm chỉ 1 State)** | Tốt (Duyệt theo độ ưu tiên của Node) | Trung bình (Cần hệ thống Command Buffer) |
| **Khả năng mở rộng (Scale)** | Kém ($O(N^2)$ điều kiện if/else) | **Tốt cho Controller; Kém cho AI phức tạp** | **Cực tốt cho AI phức tạp** | **Tối thượng cho số lượng thực thể lớn** |
| **Độ phức tạp cài đặt** | Thấp (Viết nhanh, dễ hiểu) | **Trung bình (Đòi hỏi hiểu OOP & Design Patterns)** | Cao (Cần Engine/Tool hỗ trợ vẽ cây) | Rất cao (Tư duy phi cấu trúc hướng đối tượng) |
| **Khả năng Debug luồng chạy** | Dễ (Đặt breakpoint xem dòng tuần tự) | **Cần Tool phụ trợ (Khó theo dõi chuỗi Callbacks)** | Dễ (Trực quan hóa Node nào đang chạy) | Khó (Dữ liệu phân mảnh trên mảng bộ nhớ) |

### 5.2. Khi nào NÊN sử dụng Event-Driven FSM?
* Thực thể có **tính loại trừ lẫn nhau cao** về mặt vật lý/hoạt ảnh (Không thể vừa nhảy vừa ngủ, không thể vừa choáng vừa chém).
* Hệ thống yêu cầu **độ trễ bằng 0** đối với đầu vào của người dùng (Action Platformer, Fighting Game, Character Action).
* Trạng thái và quy tắc chuyển đổi rõ ràng, hữu hạn (dưới 15–20 trạng thái độc lập).

### 5.3. Khi nào KHÔNG NÊN sử dụng Event-Driven FSM?
* **AI có quá nhiều biến số môi trường:** Với kẻ địch có khả năng dò đường, lẩn trốn, tìm vũ khí, phối hợp nhóm $\rightarrow$ FSM sẽ bị rơi vào **Bùng nổ trạng thái (State Explosion)**. Lúc này **Behavior Tree (BT)** hoặc **Goal-Oriented Action Planning (GOAP)** là lựa chọn vượt trội.
* **Số lượng thực thể khổng lồ:** Mô phỏng 10.000 đơn vị lính trong game chiến thuật $\rightarrow$ Chi phí gọi hàm ảo (Virtual Method Call overhead) và việc phân mảnh bộ nhớ của OOP State Pattern sẽ bóp nghẹt CPU. Lúc này bắt buộc dùng **ECS (Entity Component System)**.

---

## 6. CÁC CẠM BẪY KỸ THUẬT & CHUẨN MỰC THỰC THI (BEST PRACTICES)

### 6.1. Cạm bẫy 1: Rò rỉ Bộ Nhớ do Event (Dangling Event References)
* **Vấn đề:** Trong C#, khi Object $B$ đăng ký sự kiện của Object $A$ (`A.OnEvent += B.HandleEvent`), Object $A$ sẽ ngầm giữ một con trỏ tham chiếu mạnh (Strong Reference) tới $B$. Nếu $B$ bị hủy (Destroyed) nhưng không hủy đăng ký (`-=`), Garbage Collector sẽ coi $B$ vẫn còn sống $\rightarrow$ **Memory Leak**.
* **Chuẩn mực:** 
  * Bắt buộc tuân thủ nguyên tắc đối xứng: Đăng ký ở đâu thì hủy ở đó (`OnEnable` $\leftrightarrow$ `OnDisable`, `Awake` $\leftrightarrow$ `OnDestroy`).
  * Hoặc sử dụng cấu trúc **Weak Reference Event Manager**.

### 6.2. Cạm bẫy 2: Cấp phát bộ nhớ rác trong vòng lặp Game (Garbage Collection Spikes)
* **Vấn đề:** Mỗi lần chuyển trạng thái lại gọi `new ConcreteState()`. Thao tác này liên tục tạo rác trên Heap, ép bộ dọn rác (GC) phải chạy $\rightarrow$ Gây hiện tượng giật cục (Frame drops / Micro-stutters).
* **Chuẩn mực:** 
  * Áp dụng **Object Pool** hoặc **State Caching**: Khởi tạo toàn bộ các State một lần duy nhất khi nạp Object, lưu trữ vào mảng/biến thành viên và tái sử dụng.

### 6.3. Cạm bẫy 3: Phân mảnh Logic chuyển đổi (Transition Leakage)
* **Vấn đề:** Ai là người quyết định việc chuyển State? Bản thân State đó hay State Machine cha?
  * Nếu State con tự gọi `stateMachine.ChangeState(new NextState)`: Các State con bị dính chặt vào nhau (State A phải biết sự tồn tại của State B).
  * Nếu State Machine cha quản lý toàn bộ: State Machine cha lại biến thành một switch-case khổng lồ.
* **Chuẩn mực (Giải pháp phân cấp):**
  * Các State con chỉ phát ra tín hiệu: *"Tôi đã xong việc"* (`OnStateFinished`).
  * State Machine cha hoặc Bảng Transition Table sẽ đứng ra quyết định State tiếp theo là ai dựa trên cấu hình.

---

## 7. MÃ NGUỒN MẪU CHUẨN MỰC "SÁCH GIÁO KHOA" (TEXTBOOK REFERENCE IMPLEMENTATION IN C#)

Dưới đây là một bộ mã nguồn hoàn chỉnh, độc lập và chuẩn mực theo phong cách Sách Giáo Khoa (Textbook-Grade Implementation). Toàn bộ hệ thống được viết bằng C# trong môi trường Unity, tuân thủ nghiêm ngặt các nguyên lý SOLID, Observer Pattern và State Pattern.

### 7.1. Cấu trúc Thư mục Module
```
📁 Architecture_ED_FSM/
├── 📄 IState.cs                  (Giao diện vòng đời State)
├── 📄 StateMachine.cs             (Bộ điều phối trạng thái tái sử dụng)
├── 📄 IInputEvents.cs            (Hợp đồng sự kiện đầu vào)
├── 📄 CharacterInputBroker.cs     (Tầng Adapter chuyển phần cứng thành C# Event)
├── 📄 CharacterContext.cs         (Thực thể trung tâm / Context)
└── 📁 States/
    ├── 📄 CharacterBaseState.cs   (Lớp trừu tượng trung gian)
    ├── 📄 IdleState.cs            (Trạng thái đứng yên)
    ├── 📄 MoveState.cs            (Trạng thái di chuyển)
    ├── 📄 JumpState.cs            (Trạng thái nhảy & phản hồi nhả nút)
    └── 📄 AttackState.cs          (Trạng thái tấn công & khóa hành động)
```

---

### 7.2. Tầng 1: Giao diện Trạng thái & Bộ Điều phối (`IState.cs` & `StateMachine.cs`)

#### 📄 `IState.cs`
```csharp
namespace Architecture.FSM
{
    /// <summary>
    /// Hợp đồng vòng đời chuẩn của một trạng thái trong hệ thống.
    /// </summary>
    public interface IState
    {
        /// <summary>Gọi 1 lần duy nhất khi thực thể bước vào trạng thái.</summary>
        void Enter();

        /// <summary>Gọi 1 lần duy nhất khi thực thể rời khỏi trạng thái để dọn dẹp.</summary>
        void Exit();

        /// <summary>Chạy trong vòng lặp Update() - xử lý logic, timer, điều kiện chuyển đổi.</summary>
        void LogicUpdate();

        /// <summary>Chạy trong vòng lặp FixedUpdate() - xử lý tương tác vật lý (Physics/Rigidbody).</summary>
        void PhysicsUpdate();
    }
}
```

#### 📄 `StateMachine.cs`
```csharp
using System;

namespace Architecture.FSM
{
    /// <summary>
    /// Bộ máy trạng thái độc lập (Engine-Agnostic), có thể tái sử dụng cho bất kỳ thực thể nào.
    /// </summary>
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        /// <summary>Sự kiện bắn ra khi trạng thái thay đổi (phục vụ Debug, UI hoặc Logging).</summary>
        public event Action<IState> OnStateChanged;

        /// <summary>
        /// Khởi động máy trạng thái với trạng thái ban đầu.
        /// </summary>
        public void Initialize(IState startingState)
        {
            if (startingState == null)
                throw new ArgumentNullException(nameof(startingState), "Trạng thái khởi tạo không được null!");

            CurrentState = startingState;
            CurrentState.Enter();
            OnStateChanged?.Invoke(CurrentState);
        }

        /// <summary>
        /// Chuyển đổi từ trạng thái hiện tại sang trạng thái mới theo đúng chu trình: Exit -> Swap -> Enter.
        /// </summary>
        public void ChangeState(IState newState)
        {
            if (newState == null || newState == CurrentState)
                return;

            CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
            OnStateChanged?.Invoke(CurrentState);
        }

        public void LogicUpdate() => CurrentState?.LogicUpdate();
        public void PhysicsUpdate() => CurrentState?.PhysicsUpdate();
    }
}
```

---

### 7.3. Tầng 2: Hệ Thống Sự Kiện Đầu Vào (`IInputEvents.cs` & `CharacterInputBroker.cs`)

#### 📄 `IInputEvents.cs`
```csharp
using System;
using UnityEngine;

namespace Architecture.Input
{
    /// <summary>
    /// Giao diện trừu tượng hóa toàn bộ sự kiện đầu vào.
    /// Cho phép thay thế bộ đọc bàn phím bằng AI Controller hoặc Network Replay mà không sửa code nhân vật.
    /// </summary>
    public interface IInputEvents
    {
        event Action<Vector2> OnMoveInput;
        event Action OnJumpPressed;
        event Action OnJumpReleased;
        event Action OnAttackPressed;
    }
}
```

#### 📄 `CharacterInputBroker.cs`
```csharp
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Architecture.Input
{
    /// <summary>
    /// Đóng vai trò là Adapter: Chuyển các tín hiệu phần cứng từ Unity New Input System
    /// thành các C# Actions thuần chủng (Pure C# Events).
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class CharacterInputBroker : MonoBehaviour, IInputEvents
    {
        public event Action<Vector2> OnMoveInput;
        public event Action OnJumpPressed;
        public event Action OnJumpReleased;
        public event Action OnAttackPressed;

        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction attackAction;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            var actionMap = playerInput.actions;

            moveAction = actionMap["Move"];
            jumpAction = actionMap["Jump"];
            attackAction = actionMap["Attack"];
        }

        private void OnEnable()
        {
            // Bắt trực tiếp sự kiện phần cứng ở tầng C# Delegate (Zero polling)
            moveAction.performed += HandleMove;
            moveAction.canceled += HandleMove;

            jumpAction.performed += HandleJumpPressed;
            jumpAction.canceled += HandleJumpReleased;

            attackAction.performed += HandleAttackPressed;
        }

        private void OnDisable()
        {
            // QUY TẮC SỐNG CÒN: Hủy đăng ký đối xứng để triệt tiêu Memory Leak
            moveAction.performed -= HandleMove;
            moveAction.canceled -= HandleMove;

            jumpAction.performed -= HandleJumpPressed;
            jumpAction.canceled -= HandleJumpReleased;

            attackAction.performed -= HandleAttackPressed;
        }

        private void HandleMove(InputAction.CallbackContext ctx) => OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());
        private void HandleJumpPressed(InputAction.CallbackContext ctx) => OnJumpPressed?.Invoke();
        private void HandleJumpReleased(InputAction.CallbackContext ctx) => OnJumpReleased?.Invoke();
        private void HandleAttackPressed(InputAction.CallbackContext ctx) => OnAttackPressed?.Invoke();
    }
}
```

---

### 7.4. Tầng 3: Lớp Cơ Sở Cho Trạng Thái Nhân Vật (`CharacterBaseState.cs`)

```csharp
using Architecture.FSM;
using UnityEngine;

namespace Architecture.States
{
    /// <summary>
    /// Lớp cha của toàn bộ các State nhân vật. Nắm giữ tham chiếu tới Context và các móc nhận sự kiện.
    /// </summary>
    public abstract class CharacterBaseState : IState
    {
        protected CharacterContext context;
        protected StateMachine stateMachine;

        public CharacterBaseState(CharacterContext context, StateMachine stateMachine)
        {
            this.context = context;
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void LogicUpdate() { }
        public virtual void PhysicsUpdate() { }

        // Các móc tiếp nhận sự kiện (Event Hooks).
        // Mặc định để trống; State con nào quan tâm đến sự kiện nào thì Override hàm đó.
        public virtual void HandleJumpPressed() { }
        public virtual void HandleJumpReleased() { }
        public virtual void HandleAttackPressed() { }
    }
}
```

---

### 7.5. Tầng 4: Các Trạng Thái Cụ Thể (Concrete States)

#### 📄 `IdleState.cs` & `MoveState.cs` (Trạng thái trên đất)
```csharp
using Architecture.FSM;
using UnityEngine;

namespace Architecture.States
{
    public class IdleState : CharacterBaseState
    {
        public IdleState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }

        public override void Enter()
        {
            context.Animator.SetBool("isMoving", false);
            // Phanh đứng im khi ở trạng thái Idle
            context.Rigidbody.linearVelocity = new Vector2(0f, context.Rigidbody.linearVelocityY);
        }

        public override void LogicUpdate()
        {
            // Kiểm tra chuyển sang MoveState nếu có input di chuyển ngang
            if (Mathf.Abs(context.CurrentInputVector.x) > 0.05f)
            {
                stateMachine.ChangeState(context.MoveState);
            }
        }

        public override void HandleJumpPressed()
        {
            // Sự kiện bấm nhảy -> Chuyển ngay sang JumpState
            stateMachine.ChangeState(context.JumpState);
        }

        public override void HandleAttackPressed()
        {
            stateMachine.ChangeState(context.AttackState);
        }
    }

    public class MoveState : CharacterBaseState
    {
        public MoveState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }

        public override void Enter()
        {
            context.Animator.SetBool("isMoving", true);
        }

        public override void LogicUpdate()
        {
            // Nếu nhả cần/phím di chuyển -> Quay về IdleState
            if (Mathf.Abs(context.CurrentInputVector.x) <= 0.05f)
            {
                stateMachine.ChangeState(context.IdleState);
            }
        }

        public override void PhysicsUpdate()
        {
            // Cập nhật vận tốc vật lý trong chu kỳ FixedUpdate
            float targetX = context.CurrentInputVector.x * context.MoveSpeed;
            context.Rigidbody.linearVelocity = new Vector2(targetX, context.Rigidbody.linearVelocityY);
        }

        public override void HandleJumpPressed()
        {
            stateMachine.ChangeState(context.JumpState);
        }

        public override void HandleAttackPressed()
        {
            stateMachine.ChangeState(context.AttackState);
        }
    }
}
```

#### 📄 `JumpState.cs` (Minh họa xử lý Nhả phím để Thay đổi Độ cao Nhảy)
```csharp
using Architecture.FSM;
using UnityEngine;

namespace Architecture.States
{
    public class JumpState : CharacterBaseState
    {
        public JumpState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }

        public override void Enter()
        {
            // 1. Cấp lực đẩy tức thì khi bước vào trạng thái
            context.Rigidbody.linearVelocity = new Vector2(context.Rigidbody.linearVelocityX, context.JumpForce);
            context.Rigidbody.gravityScale = context.AscendingGravity;
            context.Animator.SetTrigger("JumpTrigger");
        }

        public override void LogicUpdate()
        {
            // Khi vận tốc Y bắt đầu giảm về <= 0, nhân vật đạt đỉnh nhảy -> Rơi xuống
            if (context.Rigidbody.linearVelocityY <= 0f)
            {
                // Tăng trọng lực khi rơi để chuyển động dứt khoát
                context.Rigidbody.gravityScale = context.FallingGravity;

                // Nếu chạm đất -> Quay về Idle
                if (context.CheckGrounded())
                {
                    stateMachine.ChangeState(context.IdleState);
                }
            }
        }

        public override void PhysicsUpdate()
        {
            // Cho phép điều hướng trên không
            float targetX = context.CurrentInputVector.x * context.MoveSpeed;
            context.Rigidbody.linearVelocity = new Vector2(targetX, context.Rigidbody.linearVelocityY);
        }

        // BẮT SỰ KIỆN NHẢ NÚT (VARIABLE JUMP HEIGHT):
        public override void HandleJumpReleased()
        {
            // Nếu người chơi nhả phím khi nhân vật đang bay lên:
            // Cắt 90% vận tốc Y ngay lập tức -> Triệt tiêu đà nhảy, ép nhân vật chuyển sang rơi.
            if (context.Rigidbody.linearVelocityY > 0f)
            {
                context.Rigidbody.linearVelocity = new Vector2(context.Rigidbody.linearVelocityX, context.Rigidbody.linearVelocityY * 0.1f);
                context.Rigidbody.gravityScale = context.FallingGravity;
            }
        }

        public override void Exit()
        {
            // Trả lại trọng lực mặc định khi thoát khỏi State
            context.Rigidbody.gravityScale = context.AscendingGravity;
        }
    }
}
```

#### 📄 `AttackState.cs` (Minh họa tính Khóa Hành Động / Mutual Exclusion)
```csharp
using Architecture.FSM;
using UnityEngine;

namespace Architecture.States
{
    public class AttackState : CharacterBaseState
    {
        private float attackTimer;
        private const float ATTACK_DURATION = 0.35f;

        public AttackState(CharacterContext context, StateMachine stateMachine) : base(context, stateMachine) { }

        public override void Enter()
        {
            attackTimer = ATTACK_DURATION;
            context.Animator.SetTrigger("AttackTrigger");

            // Khóa hoàn toàn di chuyển ngang khi đang vung vũ khí
            context.Rigidbody.linearVelocity = new Vector2(0f, context.Rigidbody.linearVelocityY);
        }

        public override void LogicUpdate()
        {
            attackTimer -= Time.deltaTime;

            // Khi hoạt cảnh chém kết thúc -> Tự động quay về Idle
            if (attackTimer <= 0f)
            {
                stateMachine.ChangeState(context.IdleState);
            }
        }

        // ĐẶC ĐIỂM SÁCH GIÁO KHOA CỦA STATE PATTERN:
        // Trong AttackState, chúng ta KHÔNG override HandleJumpPressed() hay HandleAttackPressed().
        // Do đó, dù người chơi có bấm nát phím Nhảy hay Chém, không có dòng code nào chạy cả!
        // Hành vi tấn công được bảo vệ nguyên vẹn 100% mà KHÔNG CẦN bất kỳ biến bool isAttacking nào!
    }
}
```

---

### 7.6. Tầng 5: Thực Thể Trung Tâm (`CharacterContext.cs`)

```csharp
using Architecture.FSM;
using Architecture.Input;
using Architecture.States;
using UnityEngine;

namespace Architecture
{
    /// <summary>
    /// Thực thể Context: Đóng vai trò là Trọng tài điều phối.
    /// Nắm giữ các Component vật lý, khởi tạo State Machine, và nối dây Event từ Input vào State hiện tại.
    /// </summary>
        [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
        public class CharacterContext : MonoBehaviour
        {
        [Header("Components")]
        public Rigidbody2D Rigidbody;
        public Animator Animator;
        [SerializeField] private CharacterInputBroker inputBroker;

        [Header("Movement Configuration")]
        public float MoveSpeed = 6f;
        public float JumpForce = 14f;
        public float AscendingGravity = 3.5f;
        public float FallingGravity = 8f;

        [Header("Ground Detection")]
        public Transform GroundCheckPoint;
        public Vector2 GroundCheckDimensions = new Vector2(0.6f, 0.1f);
        public LayerMask GroundLayerMask;

        // Dữ liệu nội tại
        public Vector2 CurrentInputVector { get; private set; }
        public bool IsFacingRight { get; private set; } = true;

        // Cỗ máy FSM
        public StateMachine StateMachine { get; private set; }

        // BỘ NHỚ ĐỆM TRẠNG THÁI (State Caching - Tối ưu hóa triệt tiêu GC)
        public IdleState IdleState { get; private set; }
        public MoveState MoveState { get; private set; }
        public JumpState JumpState { get; private set; }
        public AttackState AttackState { get; private set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();

            // 1. Khởi tạo State Machine
            StateMachine = new StateMachine();

            // 2. Cấp phát bộ nhớ cho các State ĐÚNG 1 LẦN DUY NHẤT trong Awake (Zero GC Spikes)
            IdleState = new IdleState(this, StateMachine);
            MoveState = new MoveState(this, StateMachine);
            JumpState = new JumpState(this, StateMachine);
            AttackState = new AttackState(this, StateMachine);
        }

        private void Start()
        {
            // Bắt đầu vòng đời nhân vật ở trạng thái Idle
            StateMachine.Initialize(IdleState);
        }

        private void OnEnable()
        {
            // 3. ĐĂNG KÝ SỰ KIỆN: Chuyển tiếp Event từ Broker vào State hiện tại
            inputBroker.OnMoveInput += HandleMoveInputReceived;
            inputBroker.OnJumpPressed += ForwardJumpPressedToCurrentState;
            inputBroker.OnJumpReleased += ForwardJumpReleasedToCurrentState;
            inputBroker.OnAttackPressed += ForwardAttackPressedToCurrentState;
        }

        private void OnDisable()
        {
            // 4. HỦY ĐĂNG KÝ: Bảo vệ bộ nhớ
            inputBroker.OnMoveInput -= HandleMoveInputReceived;
            inputBroker.OnJumpPressed -= ForwardJumpPressedToCurrentState;
            inputBroker.OnJumpReleased -= ForwardJumpReleasedToCurrentState;
            inputBroker.OnAttackPressed -= ForwardAttackPressedToCurrentState;
        }

        private void Update()
        {
            // Ủy thác toàn bộ logic cho State hiện tại
            StateMachine.LogicUpdate();
        }

        private void FixedUpdate()
        {
            // Ủy thác toàn bộ vật lý cho State hiện tại
            StateMachine.PhysicsUpdate();
        }

        // ================================================================
        // CÁC HÀM CẦU NỐI SỰ KIỆN (EVENT DISPATCHERS)
        // ================================================================
        private void HandleMoveInputReceived(Vector2 input)
        {
            CurrentInputVector = input;
            OrientFacingDirection(input.x);
        }

        private void ForwardJumpPressedToCurrentState()
        {
            if (StateMachine.CurrentState is CharacterBaseState baseState)
                baseState.HandleJumpPressed();
        }

        private void ForwardJumpReleasedToCurrentState()
        {
            if (StateMachine.CurrentState is CharacterBaseState baseState)
                baseState.HandleJumpReleased();
        }

        private void ForwardAttackPressedToCurrentState()
        {
            if (StateMachine.CurrentState is CharacterBaseState baseState)
                baseState.HandleAttackPressed();
        }

        // ================================================================
        // CÁC HÀM TIỆN ÍCH DÙNG CHUNG (SHARED UTILITIES)
        // ================================================================
        public bool CheckGrounded()
        {
            return Physics2D.OverlapBox(GroundCheckPoint.position, GroundCheckDimensions, 0f, GroundLayerMask);
        }

        private void OrientFacingDirection(float horizontalInput)
        {
            if ((horizontalInput > 0f && !IsFacingRight) || (horizontalInput < 0f && IsFacingRight))
            {
                IsFacingRight = !IsFacingRight;
                Vector3 currentScale = transform.localScale;
                currentScale.x *= -1f;
                transform.localScale = currentScale;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (GroundCheckPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(GroundCheckPoint.position, GroundCheckDimensions);
            }
        }
    }
}
```

---

### 7.7. Tóm Tắt 4 Trụ Cột Kỹ Thuật Đạt Chuẩn Sách Giáo Khoa Trong Đoạn Code Trên

1. **Phân tách trách nhiệm tuyệt đối (SRP):**
   * `CharacterInputBroker`: Chỉ quan tâm đến phần cứng.
   * `CharacterContext`: Chỉ quan tâm đến Component và cấp phát bộ nhớ.
   * `StateMachine`: Chỉ quan tâm đến việc đổi con trỏ State.
   * `ConcreteStates`: Chỉ quan tâm đến đúng hành vi của chính nó.
2. **Không phân bổ bộ nhớ động trong vòng lặp game (Zero GC Allocation):**
   * Toàn bộ 4 State được `new` đúng 1 lần trong `Awake()`. Khi chạy game, việc chuyển đổi trạng thái thực chất chỉ là tráo đổi địa chỉ con trỏ `CurrentState` ($O(1)$ CPU time, $0$ byte rác trên Heap).
3. **Phản hồi sự kiện tức thì (Instantaneous Event Response):**
   * Cú nhả phím `OnJumpReleased` không cần chờ `Update()` duyệt điều kiện. Ngay khi sự kiện phần cứng phát ra, nó xuyên thẳng vào `JumpState.HandleJumpReleased()` để bẻ gãy 90% vận tốc Y ngay lập tức.
4. **Triệt tiêu lỗi điều kiện (Bug-Free Mutual Exclusion):**
   * Khi đang ở `AttackState`, không có bất kỳ dòng code nào xử lý việc Nhảy hay Chém tiếp. Mọi input lạ đều tự động bị triệt tiêu mà không cần viết các câu lệnh `if (!isAttacking)` lồng nhau.

