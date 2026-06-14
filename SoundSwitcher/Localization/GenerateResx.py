import os
import xml.etree.ElementTree as ET

def create_resx(filepath, data):
    root = ET.Element("root")
    
    # Add schema
    schema = ET.SubElement(root, "xsd:schema", id="root", xmlns="", 
                           attrib={"xmlns:xsd": "http://www.w3.org/2001/XMLSchema", "xmlns:msdata": "urn:schemas-microsoft-com:xml-msdata"})
    element = ET.SubElement(schema, "xsd:element", name="root", attrib={"msdata:IsDataSet": "true"})
    complexType = ET.SubElement(element, "xsd:complexType")
    choice = ET.SubElement(complexType, "xsd:choice", maxOccurs="unbounded")
    data_element = ET.SubElement(choice, "xsd:element", name="data")
    data_complexType = ET.SubElement(data_element, "xsd:complexType")
    sequence = ET.SubElement(data_complexType, "xsd:sequence")
    ET.SubElement(sequence, "xsd:element", name="value", type="xsd:string", minOccurs="0", attrib={"msdata:Ordinal": "1"})
    ET.SubElement(data_complexType, "xsd:attribute", name="name", type="xsd:string")
    
    # Add resheaders
    for name, value in [("resmimetype", "text/microsoft-resx"), ("version", "2.0"), 
                        ("reader", "System.Resources.ResXResourceReader, System.Windows.Forms"),
                        ("writer", "System.Resources.ResXResourceWriter, System.Windows.Forms")]:
        resheader = ET.SubElement(root, "resheader", name=name)
        val = ET.SubElement(resheader, "value")
        val.text = value

    # Add data
    for k, v in data.items():
        d = ET.SubElement(root, "data", name=k)
        val = ET.SubElement(d, "value")
        val.text = v

    tree = ET.ElementTree(root)
    tree.write(filepath, encoding="utf-8", xml_declaration=True)

strings_en = {
    "NavDevices": "Devices",
    "NavSettings": "Settings",
    "NavAbout": "About",
    "ProgramVersion": "Program Version",
    "CheckUpdate": "Check for Updates",
    "CheckUpdateDesc": "Check and install the latest program update.",
    "Homepage": "Official Homepage",
    "HomepageDesc": "Visit the official program homepage.",
    "AutoStartup": "Register as Startup Program",
    "AutoStartupDesc": "Automatically start the program when Windows starts.",
    "SwitchCommunicationDevice": "Switch Default Communication Device",
    "SwitchCommunicationDeviceDesc": "Change the default communication device when switching profiles.",
    "ShowProfileIconInTray": "Show Profile Icon in Tray",
    "ShowProfileIconInTrayDesc": "Display the icon of the active profile in the system tray.",
    "ShowProfileChangeNotification": "Show Profile Change Notification",
    "ShowProfileChangeNotificationDesc": "Show a notification on the screen when the profile is switched.",
    "ChangeIcon": "Change Icon",
    "ResetIcon": "Reset to Default Icon",
    "Delete": "Delete",
    "Output": "Output",
    "Input": "Input",
    "AddProfile": "Add Profile",
    "TrayMenuSettings": "Program Settings",
    "TrayMenuSystemSound": "System Sound Settings",
    "TrayMenuExit": "Exit",
    "TrayNoDevice": "No Device",
    "Language": "Language",
    "LanguageDesc": "Select the display language.",
    "LanguageAuto": "System Default"
}

strings_ko = {
    "NavDevices": "장치",
    "NavSettings": "설정",
    "NavAbout": "정보",
    "ProgramVersion": "프로그램 버전",
    "CheckUpdate": "업데이트 확인",
    "CheckUpdateDesc": "프로그램의 최신 업데이트를 확인 및 설치합니다.",
    "Homepage": "홈페이지",
    "HomepageDesc": "프로그램의 공식 홈페이지를 방문합니다.",
    "AutoStartup": "시작 프로그램 등록",
    "AutoStartupDesc": "Windows 시작 시 프로그램이 자동으로 시작됩니다.",
    "SwitchCommunicationDevice": "기본 통신 장치 전환",
    "SwitchCommunicationDeviceDesc": "프로파일 전환 시 기본 통신 장치도 함께 변경합니다.",
    "ShowProfileIconInTray": "트레이에 프로파일 아이콘 표시",
    "ShowProfileIconInTrayDesc": "활성화된 프로파일의 아이콘을 트레이에 표시합니다.",
    "ShowProfileChangeNotification": "프로파일 변경 알림 표시",
    "ShowProfileChangeNotificationDesc": "프로파일이 전환될 때 화면에 알림을 표시합니다.",
    "ChangeIcon": "아이콘 변경",
    "ResetIcon": "기본 아이콘으로 초기화",
    "Delete": "삭제",
    "Output": "출력",
    "Input": "입력",
    "AddProfile": "프로파일 추가",
    "TrayMenuSettings": "프로그램 설정",
    "TrayMenuSystemSound": "시스템 소리 설정",
    "TrayMenuExit": "종료",
    "TrayNoDevice": "장치 없음",
    "Language": "언어",
    "LanguageDesc": "프로그램의 표시 언어를 설정합니다.",
    "LanguageAuto": "시스템 기본값"
}

strings_ja = {
    "NavDevices": "デバイス",
    "NavSettings": "設定",
    "NavAbout": "情報",
    "ProgramVersion": "プログラムのバージョン",
    "CheckUpdate": "アップデートを確認",
    "CheckUpdateDesc": "最新のプログラムアップデートを確認してインストールします。",
    "Homepage": "ホームページ",
    "HomepageDesc": "公式ホームページにアクセスします。",
    "AutoStartup": "スタートアップに登録",
    "AutoStartupDesc": "Windows起動時にプログラムを自動的に開始します。",
    "SwitchCommunicationDevice": "既定の通信デバイスを切り替え",
    "SwitchCommunicationDeviceDesc": "プロファイル切り替え時に既定の通信デバイスも変更します。",
    "ShowProfileIconInTray": "トレイにプロファイルアイコンを表示",
    "ShowProfileIconInTrayDesc": "アクティブなプロファイルのアイコンをシステムトレイに表示します。",
    "ShowProfileChangeNotification": "プロファイル変更通知を表示",
    "ShowProfileChangeNotificationDesc": "プロファイルが切り替わったときに画面に通知を表示します。",
    "ChangeIcon": "アイコンを変更",
    "ResetIcon": "デフォルトのアイコンにリセット",
    "Delete": "削除",
    "Output": "出力",
    "Input": "入力",
    "AddProfile": "プロファイルを追加",
    "TrayMenuSettings": "プログラム設定",
    "TrayMenuSystemSound": "システムサウンド設定",
    "TrayMenuExit": "終了",
    "TrayNoDevice": "デバイスなし",
    "Language": "言語",
    "LanguageDesc": "表示言語を選択します。",
    "LanguageAuto": "システム デフォルト"
}

strings_zh_cn = {
    "NavDevices": "设备",
    "NavSettings": "设置",
    "NavAbout": "关于",
    "ProgramVersion": "程序版本",
    "CheckUpdate": "检查更新",
    "CheckUpdateDesc": "检查并安装最新版本的程序更新。",
    "Homepage": "官方主页",
    "HomepageDesc": "访问官方主页。",
    "AutoStartup": "开机自动启动",
    "AutoStartupDesc": "在 Windows 启动时自动运行程序。",
    "SwitchCommunicationDevice": "切换默认通信设备",
    "SwitchCommunicationDeviceDesc": "在切换配置文件时，同时更改默认的通信设备。",
    "ShowProfileIconInTray": "在托盘中显示配置文件图标",
    "ShowProfileIconInTrayDesc": "在系统托盘中显示当前激活的配置文件图标。",
    "ShowProfileChangeNotification": "显示配置文件更改通知",
    "ShowProfileChangeNotificationDesc": "切换配置文件时在屏幕上显示通知。",
    "ChangeIcon": "更改图标",
    "ResetIcon": "重置为默认图标",
    "Delete": "删除",
    "Output": "输出",
    "Input": "输入",
    "AddProfile": "添加配置文件",
    "TrayMenuSettings": "程序设置",
    "TrayMenuSystemSound": "系统声音设置",
    "TrayMenuExit": "退出",
    "TrayNoDevice": "没有设备",
    "Language": "语言",
    "LanguageDesc": "选择显示语言。",
    "LanguageAuto": "系统默认"
}

strings_zh_tw = {
    "NavDevices": "裝置",
    "NavSettings": "設定",
    "NavAbout": "關於",
    "ProgramVersion": "程式版本",
    "CheckUpdate": "檢查更新",
    "CheckUpdateDesc": "檢查並安裝最新版本的程式更新。",
    "Homepage": "官方首頁",
    "HomepageDesc": "前往官方首頁。",
    "AutoStartup": "開機自動啟動",
    "AutoStartupDesc": "在 Windows 啟動時自動執行程式。",
    "SwitchCommunicationDevice": "切換預設通訊裝置",
    "SwitchCommunicationDeviceDesc": "切換設定檔時，同時更改預設的通訊裝置。",
    "ShowProfileIconInTray": "在系統匣中顯示設定檔圖示",
    "ShowProfileIconInTrayDesc": "在系統匣中顯示目前啟用的設定檔圖示。",
    "ShowProfileChangeNotification": "顯示設定檔變更通知",
    "ShowProfileChangeNotificationDesc": "切換設定檔時在螢幕上顯示通知。",
    "ChangeIcon": "變更圖示",
    "ResetIcon": "重設為預設圖示",
    "Delete": "刪除",
    "Output": "輸出",
    "Input": "輸入",
    "AddProfile": "新增設定檔",
    "TrayMenuSettings": "程式設定",
    "TrayMenuSystemSound": "系統音效設定",
    "TrayMenuExit": "結束",
    "TrayNoDevice": "沒有裝置",
    "Language": "語言",
    "LanguageDesc": "選擇顯示語言。",
    "LanguageAuto": "系統預設"
}

base_dir = r"C:\Users\Kevin\Documents\Repositories\SoundSwitcher\SoundSwitcher\Localization"
os.makedirs(base_dir, exist_ok=True)

create_resx(os.path.join(base_dir, "Strings.resx"), strings_en)
create_resx(os.path.join(base_dir, "Strings.ko-KR.resx"), strings_ko)
create_resx(os.path.join(base_dir, "Strings.ja-JP.resx"), strings_ja)
create_resx(os.path.join(base_dir, "Strings.zh-CN.resx"), strings_zh_cn)
create_resx(os.path.join(base_dir, "Strings.zh-TW.resx"), strings_zh_tw)

print("Created all resx files")
