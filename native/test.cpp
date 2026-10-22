#include "travel.cpp"
#include <cassert>
#include <iostream>
static int nativeCalls,forwarded,closed;static std::wstring received;
static uint32_t __cdecl FakeQueue(uint32_t,const void*,const NativeWide* text,int,int){nativeCalls++;received.assign(text->first,text->last);return 1;}
static void __fastcall FakeButton(void* self,void*,void* context){if(context==*(void**)((BYTE*)self+0xa8)){std::wstring s=L"naboo Theed naboo Keren 1";NativeWide n{s.data(),s.data()+s.size(),s.data()+s.size()+1};QueueHook(1,nullptr,&n,0,0);}}
static void __fastcall FakeClose(void*,void*){closed++;}
static bool __fastcall FakeMessage(void*,void*,void*,const void*){forwarded++;return true;}
static const char* testAccount="";static void* focusedControl;
static const char* __cdecl FakeAccount(){return testAccount;}
static void __fastcall FakeFocus(void* self,void*){focusedControl=self;}
int main(){
 nativeQueue=FakeQueue;nativeButton=(Button)FakeButton;nativeClose=(void(__thiscall*)(void*))FakeClose;trampoline=(void*)FakeMessage;
 BYTE self[0x200]{};int destination,purchase;void* elements[]={&destination};void* vector[]={elements,elements+1,elements+1};*(void***)(self+0xe0)=vector;*(void**)(self+0xa8)=&purchase;
 int single=7,doubled=11;
 assert(MessageHook(self,nullptr,&destination,&single));assert(forwarded==1&&nativeCalls==0);
 assert(!MessageHook(self,nullptr,&destination,&doubled));assert(received==L"private_direct naboo Theed naboo Keren direct"&&nativeCalls==1&&closed==1);
 MessageHook(self,nullptr,&destination,&doubled);assert(nativeCalls==1);
 assert(MessageHook(self,nullptr,&purchase,&doubled));assert(forwarded==2&&nativeCalls==1);
 std::wstring normal=L"a b c d 1";NativeWide n{normal.data(),normal.data()+normal.size(),normal.data()+normal.size()+1};QueueHook(1,nullptr,&n,0,0);assert(received==normal);
 BYTE* fake=(BYTE*)VirtualAlloc(nullptr,missionConfirmRva+32,MEM_COMMIT|MEM_RESERVE,PAGE_READWRITE);assert(fake);
 const BYTE signature[]={0x8a,0x45,0x0c,0x84,0xc0,0x0f,0x84,0xe6,0x00,0x00,0x00,0xe8,0xff,0xb9,0xaa,0xff};
 memcpy(fake+missionConfirmRva,signature,sizeof(signature));
 assert(InstallInstantMissionAccept(fake));
 assert(!memcmp(fake+missionConfirmRva,signature,5));
 for(int i=5;i<11;i++)assert(fake[missionConfirmRva+i]==0x90);
 assert(!memcmp(fake+missionConfirmRva+11,signature+11,5));
 assert(InstallInstantMissionAccept(fake));
 memcpy(fake+missionConfirmRva,signature,sizeof(signature));fake[missionConfirmRva+7]^=1;
 BYTE before[sizeof(signature)];memcpy(before,fake+missionConfirmRva,sizeof(before));
 assert(!InstallInstantMissionAccept(fake)&&!memcmp(before,fake+missionConfirmRva,sizeof(before)));
 VirtualFree(fake,0,MEM_RELEASE);
 std::cout<<"PASS: mission accept skips only its confirmation branch, preserves adjacent instructions, is idempotent, and rejects mismatched signatures.\n";
 BYTE login[0xb0]{};int username=0,password=0;
 *(void**)(login+0xa0)=&username;*(void**)(login+0xa4)=&password;
 nativeLoginAccount=FakeAccount;nativeLoginFocus=(LoginFocus)FakeFocus;
 testAccount="";FocusLogin(login);assert(focusedControl==&username);
 testAccount="saved-account";FocusLogin(login);assert(focusedControl==&password);
 testAccount=nullptr;FocusLogin(login);assert(focusedControl==&username);
 focusedControl=nullptr;FocusLogin(nullptr);assert(focusedControl==nullptr);
 BYTE* loginImage=(BYTE*)VirtualAlloc(nullptr,loginSetFocusRva+32,MEM_COMMIT|MEM_RESERVE,PAGE_READWRITE);assert(loginImage);
 const BYTE loginSite[]={0x8b,0x8e,0xa0,0,0,0,0xe8,0x44,0x99,0x47,0};
 const BYTE getter[]={0xa1,0xec,0x11,0x91,0x01,0xc3};
 const BYTE setter[]={0x56,0x8b,0xf1,0x8b,0x06,0xff,0x90,0xa4,0,0,0};
 memcpy(loginImage+loginFocusRva,loginSite,sizeof(loginSite));
 memcpy(loginImage+loginAccountRva,getter,sizeof(getter));memcpy(loginImage+loginSetFocusRva,setter,sizeof(setter));
 assert(InstallLoginPasswordFocus(loginImage));assert(!memcmp(loginImage+loginFocusRva,loginSite,6));
 assert(loginImage[loginFocusRva+11]==0);assert(InstallLoginPasswordFocus(loginImage));
 memcpy(loginImage+loginFocusRva,loginSite,sizeof(loginSite));loginImage[loginFocusRva+7]^=1;
 BYTE loginBefore[sizeof(loginSite)];memcpy(loginBefore,loginImage+loginFocusRva,sizeof(loginBefore));
 assert(!InstallLoginPasswordFocus(loginImage));assert(!memcmp(loginBefore,loginImage+loginFocusRva,sizeof(loginBefore)));
 VirtualFree(loginImage,0,MEM_RELEASE);
 std::cout<<"PASS: saved-account password focus, blank-account username focus, missing-pointer safety, exact patch bounds, repeat install and signature rejection.\n";
 BYTE* placement=(BYTE*)VirtualAlloc(nullptr,placementClickRva+64,MEM_COMMIT|MEM_RESERVE,PAGE_READWRITE);assert(placement);
 const BYTE heightSignature[]={0x8a,0x45,0xe4,0x84,0xc0,0x74,0x05,0x8b,0x4d,0xcc,0x89,0x0b,0x8b,0xb6,0x74,0x02,0,0};
 const BYTE clickSignature[]={0xe8,0x2d,0x23,0xea,0xff,0x84,0xc0,0x74,0x58,0xa0,0x58,0x88,0x90,0x01};
 memcpy(placement+placementClickRva,clickSignature,sizeof(clickSignature));
 memcpy(placement+placementHeightRva,heightSignature,sizeof(heightSignature));
 assert(InstallRelaxedPlacement(placement));
 for(size_t i=0;i<sizeof(clickSignature);++i)assert(placement[placementClickRva+i]==((i==7||i==8)?0x90:clickSignature[i]));
 for(size_t i=0;i<sizeof(heightSignature);++i)assert(placement[placementHeightRva+i]==(i==5?0xeb:heightSignature[i]));
 assert(InstallRelaxedPlacement(placement));
 memcpy(placement+placementHeightRva,heightSignature,sizeof(heightSignature));placement[placementHeightRva+8]^=1;
 BYTE unchanged[sizeof(heightSignature)];memcpy(unchanged,placement+placementHeightRva,sizeof(unchanged));
 assert(!InstallRelaxedPlacement(placement));assert(!memcmp(unchanged,placement+placementHeightRva,sizeof(unchanged)));
 VirtualFree(placement,0,MEM_RELEASE);
 std::cout<<"PASS: relaxed placement preserves terrain preview height, patches only guarded preview/click branches, is idempotent, and rejects foreign code.\n";
 auto create=Proc("DirectInput8Create");assert(create!=nullptr); // unsupported test host must leave system input available
 std::cout<<"PASS: native single/double-click routing, destination membership, direct marker, debounce, ordinary ticket preservation, and unsupported-host input forwarding.\n";
}
