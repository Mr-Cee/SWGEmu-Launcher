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
static bool __fastcall MessageHook(void* self,void*,void* context,const void* message){if(Readable(message,4)&&*(const int*)message==11&&Destination(self,context)){DWORD now=GetTickCount();if(lastRequest&&now-lastRequest<1000)return false;lastRequest=now;auto button=nativeButton;button(self,context);void* purchase=*(void**)((BYTE*)self+0xa8);if(!purchase)return false;directRequest=true;button(self,purchase);directRequest=false;nativeClose((BYTE*)self+4);return false;}return ((Message)trampoline)(self,context,message);}
static bool Patch(void* p,const void* data,size_t n){DWORD old;if(!VirtualProtect(p,n,PAGE_EXECUTE_READWRITE,&old))return false;memcpy(p,data,n);FlushInstructionCache(GetCurrentProcess(),p,n);DWORD unused;return VirtualProtect(p,n,old,&unused)!=FALSE;}
static BOOL CALLBACK Initialize(PINIT_ONCE,PVOID,PVOID*){wchar_t path[MAX_PATH];UINT n=GetSystemDirectoryW(path,MAX_PATH);if(!n||n+13>=MAX_PATH)return TRUE;wcscat_s(path,L"\\dinput8.dll");systemInput=LoadLibraryW(path);image=(BYTE*)GetModuleHandleW(nullptr);auto dos=(IMAGE_DOS_HEADER*)image;if(dos->e_magic!=IMAGE_DOS_SIGNATURE)return TRUE;auto nt=(IMAGE_NT_HEADERS32*)(image+dos->e_lfanew);const BYTE msg[]={0x55,0x8b,0xec,0x8b,0x45,0x08},button[]={0x55,0x8b,0xec,0x6a,0xff},call[]={0xe8,0xa0,0x9a,0x6e,0xff};if(nt->FileHeader.Machine!=IMAGE_FILE_MACHINE_I386||nt->OptionalHeader.SizeOfImage<0x15c0000||memcmp(image+msgRva,msg,6)||memcmp(image+buttonRva,button,5)||memcmp(image+callRva,call,5)){Log("Unsupported client signatures. Travel extension disabled; normal input preserved.");return TRUE;}BYTE* t=(BYTE*)VirtualAlloc(nullptr,16,MEM_COMMIT|MEM_RESERVE,PAGE_READWRITE);if(!t)return TRUE;memcpy(t,msg,6);t[6]=0xe9;*(int32_t*)(t+7)=(int32_t)((image+msgRva+6)-(t+11));DWORD old;if(!VirtualProtect(t,16,PAGE_EXECUTE_READ,&old)){VirtualFree(t,0,MEM_RELEASE);return TRUE;}trampoline=t;nativeButton=(Button)(image+buttonRva);nativeQueue=(Queue)(image+queueRva);nativeClose=(void(__thiscall*)(void*))(image+closeRva);BYTE c[5]={0xe8};*(int32_t*)(c+1)=(int32_t)((BYTE*)&QueueHook-(image+callRva+5));BYTE j[6]={0xe9,0,0,0,0,0x90};*(int32_t*)(j+1)=(int32_t)((BYTE*)&MessageHook-(image+msgRva+5));if(!Patch(image+callRva,c,5))return TRUE;if(!Patch(image+msgRva,j,6)){Patch(image+callRva,call,5);return TRUE;}Log("Private travel extension 1.0: map double-click installed. Server validates the one-way fare.");return TRUE;}
static FARPROC Proc(const char* n){InitOnceExecuteOnce(&once,Initialize,nullptr,nullptr);return systemInput?GetProcAddress(systemInput,n):nullptr;}
extern "C" HRESULT WINAPI DirectInput8Create(HINSTANCE h,DWORD v,REFIID i,LPVOID* o,LPUNKNOWN u){auto p=Proc("DirectInput8Create");return p?((HRESULT(WINAPI*)(HINSTANCE,DWORD,REFIID,LPVOID*,LPUNKNOWN))p)(h,v,i,o,u):E_FAIL;}
extern "C" HRESULT WINAPI DllCanUnloadNow(){auto p=Proc("DllCanUnloadNow");return p?((HRESULT(WINAPI*)())p)():S_FALSE;}
extern "C" HRESULT WINAPI DllGetClassObject(REFCLSID c,REFIID i,LPVOID* o){auto p=Proc("DllGetClassObject");return p?((HRESULT(WINAPI*)(REFCLSID,REFIID,LPVOID*))p)(c,i,o):E_FAIL;}
extern "C" HRESULT WINAPI DllRegisterServer(){auto p=Proc("DllRegisterServer");return p?((HRESULT(WINAPI*)())p)():E_FAIL;}
extern "C" HRESULT WINAPI DllUnregisterServer(){auto p=Proc("DllUnregisterServer");return p?((HRESULT(WINAPI*)())p)():E_FAIL;}
BOOL WINAPI DllMain(HINSTANCE h,DWORD r,LPVOID){if(r==DLL_PROCESS_ATTACH)DisableThreadLibraryCalls(h);return TRUE;}
