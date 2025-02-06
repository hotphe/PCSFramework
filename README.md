# PCSFramework
Unity 개발 시 사용하는 개인 Framework 소스코드 입니다.

해당 코드는 독학과 실무 경험을 바탕으로 작성한 것으로, 학원 교육이나 강사의 도움 없이 개발되었습니다.

### Common
Framework에서 공통적으로 사용되는 코드입니다.<br/>
일부 코드의 경우 외부에서 독단으로 사용 가능합니다. (추후 분리 예정)<br/>
각종 Inspector를 표시해 줄 Attribute와 Insepctor에서 수정 및 편집이 가능한 SerializedDictionary가 포함되어있습니다.

### Crypto
암호화 코드입니다. 데이터를 암호화 및 복호화 할때 사용합니다.

### NewDI
[Reflex](https://github.com/gustavopsantos/Reflex) 를 기반으로 작성한 DI Framework 입니다.<br/>
Name 태그 기능과 AllowRebind 기능을 추가하였습니다.

### Observable
[UniRx](https://github.com/neuecc/UniRx) 를 기반으로 작성한 Observable 코드입니다.<br/>
가볍게 Subscribe 및 Notify만을 추가 하였습니다.<br/>
Unirx 및 R3에서 지원하지 않는 Serializable Observable List를 제공합니다.<br/>
런타임 중 Inspector에서 List에 변화를 줄 경우 Subscribe된 (Add, Remove, Insert, ValueChange) 가 작동하도록 구현되어 있습니다.
이를 통해 기능 테스트를 보다 수월하게 진행할 수 있습니다.

### SaveData
PlayerPrefs 기반으로 작동하는 세이브 시스템입니다.<br/>
옵션과 같은 게임 내 하나만 존재해야 하는 것의 저장에 적합합니다.

### SceneManagement
SceneConfig에 입력된 데이터를 바탕으로 게임 시작 시 필수 씬을 로드하고, 특정 씬 로드시 같이 로드될 Additive씬을 불러옵니다.<br> 현재 씬의 PresenterBase의 Initialize 함수를 호출합니다.<br/>
MVP 패턴 사용시 현재 씬의 PresenterBase의 Initialize 함수를 호출합니다.<br/>
씬 전환 시 연출 효과용 코드 및 프리팹이 포함되어있습니다.

### Sound
효과음 및 배경음에 사용됩니다. ObjectPool 기반으로 효과음 출력 시 해당 효과음을 추가한 object를 생성 및 반환하는 구조로 되어있습니다.<br/>
SoundConfig 에 SerializedDictionary를 이용해 각 사운드별 Key Value를 설정할 수 있습니다.

### UI
다양한 모바일 해상도에 필요한 적응형 UI, 레터박스, 터치 블로커(입력 블로킹), 화면 크기 변경 감지 등 UI와 관련된 코드입니다.


### 사용한 라이브러리 
- [UniTask](https://github.com/Cysharp/UniTask)
- [MessagePack](https://github.com/MessagePack-CSharp/MessagePack-CSharp) (Network 사용 시)
- [Eflatun.SceneReference](https://github.com/starikcetin/Eflatun.SceneReference)
- [Addressable](https://docs.unity3d.com/Packages/com.unity.addressables@2.2/manual/index.html)
