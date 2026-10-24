#define WIN32_LEAN_AND_MEAN
#define DIRECTINPUT_VERSION 0x0800
#include <windows.h>
#include <dinput.h>
#include <stdint.h>
#include <string>
#include <cstring>
// Original, version-specific client extension. No third-party extension code.
static BYTE* image;
static HMODULE systemInput;
static INIT_ONCE once=INIT_ONCE_STATIC_INIT;
static void* trampoline;
static thread_local bool directRequest=false;
static DWORD lastRequest;
struct NativeWide{const wchar_t* first;const wchar_t* last;const wchar_t* capacity;};
using Button=void(__thiscall*)(void*,void*);
using Message=bool(__thiscall*)(void*,void*,const void*);
using Queue=uint32_t(__cdecl*)(uint32_t,const void*,const NativeWide*,int,int);
static Button nativeButton;
static Queue nativeQueue;
static void(__thiscall* nativeClose)(void*);
constexpr uintptr_t msgRva=0x985060,buttonRva=0x9848d0,callRva=0x984b4b,queueRva=0x6e5f0,closeRva=0x5c17a0;
static void Log(const char* text){char path[MAX_PATH];GetModuleFileNameA(nullptr,path,MAX_PATH);char* s=strrchr(path,'\\');if(!s)return;strcpy_s(s+1,MAX_PATH-(s+1-path),"private_travel.log");HANDLE f=CreateFileA(path,FILE_APPEND_DATA,FILE_SHARE_READ|FILE_SHARE_WRITE,nullptr,OPEN_ALWAYS,FILE_ATTRIBUTE_NORMAL,nullptr);if(f!=INVALID_HANDLE_VALUE){DWORD n;WriteFile(f,text,(DWORD)strlen(text),&n,nullptr);WriteFile(f,"\r\n",2,&n,nullptr);CloseHandle(f);}}
static bool Readable(const void* p,size_t n){MEMORY_BASIC_INFORMATION m{};return p&&VirtualQuery(p,&m,sizeof(m))&&m.State==MEM_COMMIT&&!(m.Protect&(PAGE_GUARD|PAGE_NOACCESS))&&(uintptr_t)p+n>=(uintptr_t)p&&(uintptr_t)p+n<=(uintptr_t)m.BaseAddress+m.RegionSize;}
static bool Destination(void* self,void* context){if(!Readable(self,0xe4))return false;auto v=*reinterpret_cast<void***>((BYTE*)self+0xe0);if(!Readable(v,12))return false;auto begin=(void**)v[0],end=(void**)v[1];if((uintptr_t)end<(uintptr_t)begin||(uintptr_t)end-(uintptr_t)begin>4096||!Readable(begin,(uintptr_t)end-(uintptr_t)begin))return false;for(auto p=begin;p!=end;++p)if(*p==context)return true;return false;}
static uint32_t __cdecl QueueHook(uint32_t command,const void* target,const NativeWide* args,int a,int b){auto original=nativeQueue;if(!directRequest)return original(command,target,args,a,b);if(!Readable(args,sizeof(*args))||args->last<args->first||args->last-args->first>1024||!Readable(args->first,(args->last-args->first)*2))return 0;std::wstring text(args->first,args->last);auto split=text.find_last_of(L' ');if(split==std::wstring::npos)return 0;text.resize(split+1);text+=L"direct";text.insert(0,L"private_direct ");NativeWide replacement{text.data(),text.data()+text.size(),text.data()+text.size()+1};Log("Map double-click: paid direct-travel request sent.");return original(command,target,&replacement,a,b);}
// The supported native Travel handler uses the selected ship ID at 0x118 and
// its own button at 0xd8. Ticket mode uses Purchase at 0xa8 instead.
static bool __fastcall MessageHook(void* self,void*,void* context,const void* message){
 if(Readable(message,4)&&*(const int*)message==11&&Destination(self,context)){
  if(!Readable(self,0x120))return ((Message)trampoline)(self,context,message);
  const bool ship=*(const uint32_t*)((BYTE*)self+0x118)||*(const uint32_t*)((BYTE*)self+0x11c);
  void* action=*(void**)((BYTE*)self+(ship?0xd8:0xa8));
  if(!action)return ((Message)trampoline)(self,context,message);
  DWORD now=GetTickCount();if(lastRequest&&now-lastRequest<1000)return false;lastRequest=now;
  auto button=nativeButton;button(self,context);
  if(ship){
   // Native launchIntoSpace builds the selected ship, terminal and passenger list
   // and closes the map itself. Never rewrite it as a ticket purchase.
   Log("Map double-click: personal ship Travel action sent.");button(self,action);
  }else{
   directRequest=true;button(self,action);directRequest=false;nativeClose((BYTE*)self+4);
  }
  return false;
 }
 return ((Message)trampoline)(self,context,message);
}
static bool Patch(void* p,const void* data,size_t n){DWORD old;if(!VirtualProtect(p,n,PAGE_EXECUTE_READWRITE,&old))return false;memcpy(p,data,n);FlushInstructionCache(GetCurrentProcess(),p,n);DWORD unused;return VirtualProtect(p,n,old,&unused)!=FALSE;}
// Skip only CuiMissionManager::acceptMission's false-confirmed branch.
// The native confirmed path still sends the normal server-validated request.
constexpr uintptr_t missionConfirmRva=0x5797c1;
static bool InstallInstantMissionAccept(BYTE* base){
 const BYTE expected[]={0x8a,0x45,0x0c,0x84,0xc0,0x0f,0x84,0xe6,0x00,0x00,0x00,0xe8,0xff,0xb9,0xaa,0xff};
 const BYTE skip[]={0x90,0x90,0x90,0x90,0x90,0x90};
 BYTE* site=base+missionConfirmRva;
 if(!Readable(site,sizeof(expected))||memcmp(site,expected,5)||memcmp(site+11,expected+11,5))return false;
 if(!memcmp(site+5,skip,sizeof(skip)))return true;
 if(memcmp(site+5,expected+5,6))return false;
 return Patch(site+5,skip,sizeof(skip));
}
// Login activation normally focuses username even when ConfigClientGame already filled it.
constexpr uintptr_t loginFocusRva=0x88d291,loginAccountRva=0x102e30,loginSetFocusRva=0xd06be0;
using LoginAccount=const char*(__cdecl*)();
using LoginFocus=void(__thiscall*)(void*);
static LoginAccount nativeLoginAccount;
static LoginFocus nativeLoginFocus;
static void __cdecl FocusLogin(void* self){
 if(!Readable(self,0xa8)||!nativeLoginAccount||!nativeLoginFocus)return;
 const char* name=nativeLoginAccount();
 bool saved=Readable(name,1)&&*name!='\0';
 void* control=*(void**)((BYTE*)self+(saved?0xa4:0xa0));
 if(Readable(control,sizeof(void*)))nativeLoginFocus(control);
}
// ESI is the login mediator at this exact, signature-checked call site.
static __declspec(naked) void LoginFocusHook(){__asm{
 push esi
 call FocusLogin
 add esp,4
 ret
 }}
static bool InstallLoginPasswordFocus(BYTE* base){
 const BYTE expected[]={0x8b,0x8e,0xa0,0,0,0,0xe8,0x44,0x99,0x47,0};
 const BYTE account[]={0xa1,0xec,0x11,0x91,0x01,0xc3};
 const BYTE focus[]={0x56,0x8b,0xf1,0x8b,0x06,0xff,0x90,0xa4,0,0,0};
 BYTE* site=base+loginFocusRva;
 if(!Readable(site,sizeof(expected))||!Readable(base+loginAccountRva,sizeof(account))||!Readable(base+loginSetFocusRva,sizeof(focus))||
    memcmp(site,expected,6)||memcmp(base+loginAccountRva,account,sizeof(account))||memcmp(base+loginSetFocusRva,focus,sizeof(focus)))return false;
 BYTE call[5]={0xe8};*(int32_t*)(call+1)=(int32_t)((BYTE*)&LoginFocusHook-(site+11));
 if(!memcmp(site+6,call,sizeof(call)))return true;
 if(memcmp(site+6,expected+6,5))return false;
 nativeLoginAccount=(LoginAccount)(base+loginAccountRva);
 nativeLoginFocus=(LoginFocus)(base+loginSetFocusRva);
 return Patch(site+6,call,sizeof(call));
}
// Forced client placement keeps the terrain-sampled preview height instead of
// the uncomputed LotManager height (zero when its early check was bypassed).
constexpr uintptr_t placementHeightRva=0x2cc1a5,placementClickRva=0x8a9e6e;
static bool InstallRelaxedPlacement(BYTE* base){
 const BYTE expected[]={0x8a,0x45,0xe4,0x84,0xc0,0x74,0x05,0x8b,0x4d,0xcc,0x89,0x0b,0x8b,0xb6,0x74,0x02,0,0};
 const BYTE clickExpected[]={0xe8,0x2d,0x23,0xea,0xff,0x84,0xc0,0x74,0x58,0xa0,0x58,0x88,0x90,0x01};
 const BYTE accept[]={0x90,0x90};
 BYTE* site=base+placementHeightRva;BYTE* click=base+placementClickRva;
 if(!Readable(site,sizeof(expected))||!Readable(click,sizeof(clickExpected))||memcmp(site,expected,5)||memcmp(site+6,expected+6,sizeof(expected)-6)||
    memcmp(click,clickExpected,7)||memcmp(click+9,clickExpected+9,sizeof(clickExpected)-9))return false;
 bool heightDone=site[5]==0xeb,clickDone=!memcmp(click+7,accept,2);
 if((!heightDone&&site[5]!=expected[5])||(!clickDone&&memcmp(click+7,clickExpected+7,2)))return false;
 const BYTE keepTerrainHeight=0xeb;
 if(!heightDone&&!Patch(site+5,&keepTerrainHeight,1))return false;
 // Retain the native footprint-null guard and coordinate/request construction;
 // bypass only the redundant client LotManager result. Server validates the deed.
 if(!clickDone&&!Patch(click+7,accept,2)){
  if(!heightDone)Patch(site+5,expected+5,1);
  return false;
 }
 return true;
}
// CuiInventoryManager::destroyObject: take the existing confirmed path.
// Object/player guards and the serverDestroyObject request remain native.
constexpr uintptr_t itemDestroyConfirmRva=0x4da890;
static bool InstallInstantItemDestroy(BYTE* base){
 const BYTE expected[]={0x8a,0x45,0x0c,0x84,0xc0,0x0f,0x85,0xb3,0,0,0,0xe8,0xe0,0x64,0x03,0,0x84,0xc0,0x0f,0x84,0xa6,0,0,0};
 const BYTE confirmed[]={0xe9,0xb4,0,0,0,0x90};
 BYTE* site=base+itemDestroyConfirmRva;
 if(!Readable(site,sizeof(expected))||memcmp(site,expected,5)||memcmp(site+11,expected+11,sizeof(expected)-11))return false;
 if(!memcmp(site+5,confirmed,sizeof(confirmed)))return true;
 if(memcmp(site+5,expected+5,sizeof(confirmed)))return false;
 return Patch(site+5,confirmed,sizeof(confirmed));
}

// The stock pre-CU toolbar already supports arbitrary slot indices for mouse
// input, drag/drop, execution, and settings.  Its saved pane shape defaults to
// 24 entries, though, and the double-row UI is only tall enough for those 24.
// Keep the native implementation and expand its eight panes to 36 entries.
// Slots 24-35 intentionally are not registered as toolbarSlot## actions, so the
// added row is mouse-only while still accepting abilities and macros.
constexpr uintptr_t toolbarLoadRva=0xb690b0,toolbarSaveRva=0xb68900,toolbarUpdateRva=0xb69890;
constexpr uintptr_t settingsSaveIntegerRva=0x5cd140,stringCtorRva=0x6e20,stringAssignRva=0x15be0,stringDtorRva=0x16b0;
constexpr uintptr_t widgetSetSizeRva=0xd061e0,numPanesKeyRva=0x1580448;
constexpr size_t toolbarLoadPatchLength=6,toolbarSavePatchLength=10;
struct ToolbarVector{BYTE* begin;BYTE* end;BYTE* capacity;};
enum class ToolbarShape{Complete,NeedsExpansion,Unreadable};
struct Detour{BYTE* target;BYTE original[16];size_t length;void* trampoline;};
using ToolbarProcedure=void(__thiscall*)(void*);
using SettingsSaveInteger=void(__cdecl*)(void*,const void*,int);
using GameStringCtor=void*(__thiscall*)(void*,const char*);
using GameStringAssign=void(__thiscall*)(void*,const char*,const char*);
using GameStringDtor=void(__thiscall*)(void*);
using WidgetSetSize=void(__thiscall*)(void*,const long*);
static ToolbarProcedure nativeToolbarLoad,nativeToolbarSave;
static SettingsSaveInteger nativeSettingsSaveInteger;
static GameStringCtor nativeStringCtor;
static GameStringAssign nativeStringAssign;
static GameStringDtor nativeStringDtor;
static WidgetSetSize nativeWidgetSetSize;
static ToolbarProcedure nativeToolbarUpdate;
static const void* numPanesKey;
static thread_local bool toolbarHealing;
static Detour toolbarLoadDetour{},toolbarSaveDetour{};

static ToolbarShape InspectToolbarShape(void* self){
 if(!Readable(self,0xa4))return ToolbarShape::Unreadable;
 auto panes=*reinterpret_cast<ToolbarVector**>((BYTE*)self+0xa0);
 if(!Readable(panes,sizeof(*panes))||panes->end<panes->begin)return ToolbarShape::Unreadable;
 uintptr_t bytes=(uintptr_t)panes->end-(uintptr_t)panes->begin;
 if(bytes%sizeof(ToolbarVector))return ToolbarShape::Unreadable;
 size_t count=bytes/sizeof(ToolbarVector);
 if(count<8)return ToolbarShape::NeedsExpansion;
 if(!Readable(panes->begin,8*sizeof(ToolbarVector)))return ToolbarShape::Unreadable;
 auto rows=reinterpret_cast<ToolbarVector*>(panes->begin);
 for(size_t i=0;i<8;i++){
  if(rows[i].end<rows[i].begin)return ToolbarShape::Unreadable;
  uintptr_t rowBytes=(uintptr_t)rows[i].end-(uintptr_t)rows[i].begin;
  if(rowBytes%0x48)return ToolbarShape::Unreadable;
  if(rowBytes/0x48<36)return ToolbarShape::NeedsExpansion;
 }
 return ToolbarShape::Complete;
}

static bool SaveToolbarShape(void* self){
 if(!self||!nativeSettingsSaveInteger||!nativeStringCtor||!nativeStringAssign||!nativeStringDtor||!numPanesKey)return false;
 nativeSettingsSaveInteger((BYTE*)self+4,numPanesKey,8);
 for(int pane=0;pane<8;pane++){
  char name[16];int length=wsprintfA(name,"numItems_%d",pane);
  if(length<=0||length>=(int)sizeof(name))return false;
  alignas(16) BYTE gameString[32]{};
  const char empty=0;
  nativeStringCtor(gameString,&empty);
  nativeStringAssign(gameString,name,name+length);
  nativeSettingsSaveInteger((BYTE*)self+4,gameString,36);
  nativeStringDtor(gameString);
 }
 return true;
}

static void ResizeThirdToolbar(void* self){
 if(!Readable(self,0x9c)||!nativeWidgetSetSize||!nativeToolbarUpdate)return;
 void* slots=*reinterpret_cast<void**>((BYTE*)self+0x10);
 void* backgrounds=*reinterpret_cast<void**>((BYTE*)self+0x98);
 if(!Readable(slots,0xac)||!Readable(backgrounds,0x3c))return;
 *reinterpret_cast<long*>((BYTE*)slots+0xa8)=116;
 long slotSize[2]={*reinterpret_cast<long*>((BYTE*)slots+0x34),116};
 long backgroundSize[2]={*reinterpret_cast<long*>((BYTE*)backgrounds+0x34),98};
 if(slotSize[0]<412)slotSize[0]=412;
 if(backgroundSize[0]<450)backgroundSize[0]=450;
 nativeWidgetSetSize(slots,slotSize);
 nativeWidgetSetSize(backgrounds,backgroundSize);
 nativeToolbarUpdate(self);
}

static void __fastcall ToolbarLoadHook(void* self,void*){
 nativeToolbarLoad(self);
 if(toolbarHealing)return;
 auto shape=InspectToolbarShape(self);
 if(shape==ToolbarShape::Complete){ResizeThirdToolbar(self);return;}
 if(shape==ToolbarShape::Unreadable){Log("Third toolbar: toolbar settings could not be inspected; stock layout retained.");return;}
 toolbarHealing=true;
 bool saved=SaveToolbarShape(self);
 if(saved)nativeToolbarLoad(self);
 toolbarHealing=false;
 if(saved&&InspectToolbarShape(self)==ToolbarShape::Complete){ResizeThirdToolbar(self);Log("Third toolbar: eight panes expanded to 36 clickable slots.");}
 else Log("Third toolbar: settings expansion failed; stock layout retained.");
}
static void __fastcall ToolbarSaveHook(void* self,void*){
 if(InspectToolbarShape(self)==ToolbarShape::NeedsExpansion){Log("Third toolbar: prevented a truncated toolbar settings save.");return;}
 nativeToolbarSave(self);
}
static bool CreateDetour(Detour& d,BYTE* target,const BYTE* expected,size_t length,void* hook){
 if(length<5||length>sizeof(d.original)||!Readable(target,length)||memcmp(target,expected,length))return false;
 BYTE* t=(BYTE*)VirtualAlloc(nullptr,length+5,MEM_COMMIT|MEM_RESERVE,PAGE_READWRITE);if(!t)return false;
 memcpy(t,target,length);t[length]=0xe9;*(int32_t*)(t+length+1)=(int32_t)((target+length)-(t+length+5));
 DWORD old;if(!VirtualProtect(t,length+5,PAGE_EXECUTE_READ,&old)){VirtualFree(t,0,MEM_RELEASE);return false;}
 BYTE replacement[16];memset(replacement,0x90,length);replacement[0]=0xe9;*(int32_t*)(replacement+1)=(int32_t)((BYTE*)hook-(target+5));
 memcpy(d.original,target,length);d.target=target;d.length=length;d.trampoline=t;
 if(Patch(target,replacement,length))return true;
 VirtualFree(t,0,MEM_RELEASE);d={};return false;
}
static void RemoveDetour(Detour& d){if(d.target&&d.length)Patch(d.target,d.original,d.length);if(d.trampoline)VirtualFree(d.trampoline,0,MEM_RELEASE);d={};}
static bool InstallThirdToolbar(BYTE* base){
 const BYTE loadExpected[]={0x53,0x8b,0xdc,0x83,0xec,0x08};
 const BYTE saveExpected[]={0x55,0x8b,0xec,0x6a,0xff,0x68,0x97,0x8b,0x56,0x01};
 const BYTE updateExpected[]={0x55,0x8b,0xec,0x6a,0xff,0x68};
 const BYTE saveIntegerExpected[]={0x55,0x8b,0xec,0x8b,0x45,0x08};
 const BYTE ctorExpected[]={0x55,0x8b,0xec,0x8b,0x45,0x08};
 const BYTE assignExpected[]={0x55,0x8b,0xec,0x53,0x56,0x57};
 const BYTE dtorExpected[]={0x55,0x8b,0xec,0x6a,0xff,0x68};
 const BYTE sizeExpected[]={0x55,0x8b,0xec,0x83,0xec,0x10};
 if(!Readable(base+numPanesKeyRva,24)||memcmp(base+toolbarUpdateRva,updateExpected,sizeof(updateExpected))||
    memcmp(base+settingsSaveIntegerRva,saveIntegerExpected,sizeof(saveIntegerExpected))||memcmp(base+stringCtorRva,ctorExpected,sizeof(ctorExpected))||
    memcmp(base+stringAssignRva,assignExpected,sizeof(assignExpected))||memcmp(base+stringDtorRva,dtorExpected,sizeof(dtorExpected))||
    memcmp(base+widgetSetSizeRva,sizeExpected,sizeof(sizeExpected)))return false;
 nativeSettingsSaveInteger=(SettingsSaveInteger)(base+settingsSaveIntegerRva);nativeStringCtor=(GameStringCtor)(base+stringCtorRva);
 nativeStringAssign=(GameStringAssign)(base+stringAssignRva);nativeStringDtor=(GameStringDtor)(base+stringDtorRva);
 nativeWidgetSetSize=(WidgetSetSize)(base+widgetSetSizeRva);nativeToolbarUpdate=(ToolbarProcedure)(base+toolbarUpdateRva);numPanesKey=base+numPanesKeyRva;
 if(!CreateDetour(toolbarLoadDetour,base+toolbarLoadRva,loadExpected,sizeof(loadExpected),(void*)&ToolbarLoadHook))return false;
 nativeToolbarLoad=(ToolbarProcedure)toolbarLoadDetour.trampoline;
 if(!CreateDetour(toolbarSaveDetour,base+toolbarSaveRva,saveExpected,sizeof(saveExpected),(void*)&ToolbarSaveHook)){RemoveDetour(toolbarLoadDetour);nativeToolbarLoad=nullptr;return false;}
 nativeToolbarSave=(ToolbarProcedure)toolbarSaveDetour.trampoline;
 return true;
}
#include "JunkDealerDoubleClick.h"
#include "CitizenMapColor.h"
static BOOL CALLBACK Initialize(PINIT_ONCE,PVOID,PVOID*){wchar_t path[MAX_PATH];UINT n=GetSystemDirectoryW(path,MAX_PATH);if(!n||n+13>=MAX_PATH)return TRUE;wcscat_s(path,L"\\dinput8.dll");systemInput=LoadLibraryW(path);image=(BYTE*)GetModuleHandleW(nullptr);auto dos=(IMAGE_DOS_HEADER*)image;if(dos->e_magic!=IMAGE_DOS_SIGNATURE)return TRUE;auto nt=(IMAGE_NT_HEADERS32*)(image+dos->e_lfanew);const BYTE msg[]={0x55,0x8b,0xec,0x8b,0x45,0x08},button[]={0x55,0x8b,0xec,0x6a,0xff},call[]={0xe8,0xa0,0x9a,0x6e,0xff};if(nt->FileHeader.Machine!=IMAGE_FILE_MACHINE_I386||nt->OptionalHeader.SizeOfImage!=0x15bf000||memcmp(image+msgRva,msg,6)||memcmp(image+buttonRva,button,5)||memcmp(image+callRva,call,5)||memcmp(image+0x984bc3,"\x3b\xbe\xd8\x00\x00\x00",6)||memcmp(image+0x984bcf,"\x8b\x96\x18\x01\x00\x00\x8b\x86\x1c\x01\x00\x00",12)){Log("Unsupported client signatures. Travel extension disabled; normal input preserved.");return TRUE;}BYTE* t=(BYTE*)VirtualAlloc(nullptr,16,MEM_COMMIT|MEM_RESERVE,PAGE_READWRITE);if(!t)return TRUE;memcpy(t,msg,6);t[6]=0xe9;*(int32_t*)(t+7)=(int32_t)((image+msgRva+6)-(t+11));DWORD old;if(!VirtualProtect(t,16,PAGE_EXECUTE_READ,&old)){VirtualFree(t,0,MEM_RELEASE);return TRUE;}trampoline=t;nativeButton=(Button)(image+buttonRva);nativeQueue=(Queue)(image+queueRva);nativeClose=(void(__thiscall*)(void*))(image+closeRva);BYTE c[5]={0xe8};*(int32_t*)(c+1)=(int32_t)((BYTE*)&QueueHook-(image+callRva+5));BYTE j[6]={0xe9,0,0,0,0,0x90};*(int32_t*)(j+1)=(int32_t)((BYTE*)&MessageHook-(image+msgRva+5));if(!Patch(image+callRva,c,5))return TRUE;if(!Patch(image+msgRva,j,6)){Patch(image+callRva,call,5);return TRUE;}if(InstallInstantMissionAccept(image))Log("Mission accept confirmation disabled; normal server validation retained.");else Log("Mission accept signature unsupported; native confirmation retained.");if(InstallLoginPasswordFocus(image))Log("Login focus: password for a saved account, username otherwise.");else Log("Login focus signatures unsupported; native focus retained.");if(InstallRelaxedPlacement(image))Log("Structure placement preview uses terrain height; server validates the final site.");else Log("Structure placement height signature unsupported; native height retained.");if(InstallInstantItemDestroy(image))Log("Inventory destruction confirmation disabled; server item protection retained.");else Log("Item destruction signature unsupported; native confirmation retained.");if(InstallJunkDoubleClick(image))Log("Junk dealer double-click handler installed for marked sale windows.");else Log("Junk dealer signatures unsupported; normal list handling retained.");if(InstallThirdToolbar(image))Log("Third toolbar runtime installed: 36 native mouse-clickable slots per pane.");else Log("Third toolbar signatures unsupported; stock two-row toolbar retained.");if(InstallCitizenMapColor(image))Log("Citizen radar and overhead map orange markers installed.");else Log("Citizen map signatures unsupported; native colors retained.");Log("Private client extension 1.9: map travel and interface updates installed.");return TRUE;}
static FARPROC Proc(const char* n){InitOnceExecuteOnce(&once,Initialize,nullptr,nullptr);return systemInput?GetProcAddress(systemInput,n):nullptr;}
extern "C" HRESULT WINAPI DirectInput8Create(HINSTANCE h,DWORD v,REFIID i,LPVOID* o,LPUNKNOWN u){auto p=Proc("DirectInput8Create");return p?((HRESULT(WINAPI*)(HINSTANCE,DWORD,REFIID,LPVOID*,LPUNKNOWN))p)(h,v,i,o,u):E_FAIL;}
extern "C" HRESULT WINAPI DllCanUnloadNow(){auto p=Proc("DllCanUnloadNow");return p?((HRESULT(WINAPI*)())p)():S_FALSE;}
extern "C" HRESULT WINAPI DllGetClassObject(REFCLSID c,REFIID i,LPVOID* o){auto p=Proc("DllGetClassObject");return p?((HRESULT(WINAPI*)(REFCLSID,REFIID,LPVOID*))p)(c,i,o):E_FAIL;}
extern "C" HRESULT WINAPI DllRegisterServer(){auto p=Proc("DllRegisterServer");return p?((HRESULT(WINAPI*)())p)():E_FAIL;}
extern "C" HRESULT WINAPI DllUnregisterServer(){auto p=Proc("DllUnregisterServer");return p?((HRESULT(WINAPI*)())p)():E_FAIL;}
BOOL WINAPI DllMain(HINSTANCE h,DWORD r,LPVOID){if(r==DLL_PROCESS_ATTACH)DisableThreadLibraryCalls(h);return TRUE;}
