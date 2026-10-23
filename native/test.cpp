#include "travel.cpp"
#include <cassert>
#include <iostream>
#include <vector>
static int nativeCalls,forwarded,closed;static std::wstring received;static uint32_t receivedCommand;static const void* receivedTarget;static int terminal;
static uint32_t __cdecl FakeQueue(uint32_t command,const void* target,const NativeWide* text,int,int){nativeCalls++;receivedCommand=command;receivedTarget=target;received.assign(text->first,text->last);return 1;}
static void __fastcall FakeClose(void*,void*);
static void __fastcall FakeButton(void* self,void*,void* context){if(context==*(void**)((BYTE*)self+0xa8)){std::wstring s=L"naboo Theed naboo Keren 1";NativeWide n{s.data(),s.data()+s.size(),s.data()+s.size()+1};QueueHook(1,nullptr,&n,0,0);}else if(context==*(void**)((BYTE*)self+0xd8)){std::wstring s=L"123456 2 456 789 naboo Keren";NativeWide n{s.data(),s.data()+s.size(),s.data()+s.size()+1};FakeQueue(2,&terminal,&n,0,0);FakeClose(self,nullptr);}}
static void __fastcall FakeClose(void*,void*){closed++;}
static bool __fastcall FakeMessage(void*,void*,void*,const void*){forwarded++;return true;}
static const char* testAccount="";static void* focusedControl;
static const char* __cdecl FakeAccount(){return testAccount;}
static void __fastcall FakeFocus(void* self,void*){focusedControl=self;}
static std::string fakeToolbarKey;static std::vector<std::pair<std::string,int>> fakeToolbarSettings;static long fakeSlotSize[2],fakeBackgroundSize[2];static void* fakeSlots;static int fakeToolbarUpdates;
static void* __fastcall FakeStringCtor(void* self,void*,const char*){fakeToolbarKey.clear();return self;}
static void __fastcall FakeStringAssign(void*,void*,const char* first,const char* last){fakeToolbarKey.assign(first,last);}
static void __fastcall FakeStringDtor(void*,void*){}
static void __cdecl FakeSaveInteger(void*,const void* key,int value){fakeToolbarSettings.emplace_back(key==numPanesKey?"numPanes":fakeToolbarKey,value);}
static void __fastcall FakeWidgetSetSize(void* self,void*,const long* size){long* target=self==fakeSlots?fakeSlotSize:fakeBackgroundSize;target[0]=size[0];target[1]=size[1];}
static void __fastcall FakeToolbarUpdate(void*,void*){fakeToolbarUpdates++;}
#include "JunkDealerDoubleClickTests.h"
int main(){
 nativeQueue=FakeQueue;nativeButton=(Button)FakeButton;nativeClose=(void(__thiscall*)(void*))FakeClose;trampoline=(void*)FakeMessage;
 BYTE self[0x200]{};int destination,purchase;void* elements[]={&destination};void* vector[]={elements,elements+1,elements+1};*(void***)(self+0xe0)=vector;*(void**)(self+0xa8)=&purchase;
 int single=7,doubled=11;
 assert(MessageHook(self,nullptr,&destination,&single));assert(forwarded==1&&nativeCalls==0);
 assert(!MessageHook(self,nullptr,&destination,&doubled));assert(received==L"private_direct naboo Theed naboo Keren direct"&&nativeCalls==1&&closed==1);
 MessageHook(self,nullptr,&destination,&doubled);assert(nativeCalls==1);
 assert(MessageHook(self,nullptr,&purchase,&doubled));assert(forwarded==2&&nativeCalls==1);
 std::wstring normal=L"a b c d 1";NativeWide n{normal.data(),normal.data()+normal.size(),normal.data()+normal.size()+1};QueueHook(1,nullptr,&n,0,0);assert(received==normal);
 int travel;*(void**)(self+0xd8)=&travel;*(uint32_t*)(self+0x118)=123456;
 lastRequest=0;int callsBefore=nativeCalls,closesBefore=closed;
 assert(!MessageHook(self,nullptr,&destination,&doubled));
 assert(nativeCalls==callsBefore+1&&closed==closesBefore+1);
 assert(receivedCommand==2&&receivedTarget==&terminal&&received==L"123456 2 456 789 naboo Keren");
 assert(!directRequest);MessageHook(self,nullptr,&destination,&doubled);assert(nativeCalls==callsBefore+1);
 // A high-half-only valid ship ID also selects Travel. A missing action preserves native input.
 *(uint32_t*)(self+0x118)=0;*(uint32_t*)(self+0x11c)=1;lastRequest=0;
 MessageHook(self,nullptr,&destination,&doubled);assert(receivedCommand==2&&nativeCalls==callsBefore+2);
 *(void**)(self+0xd8)=nullptr;lastRequest=0;assert(MessageHook(self,nullptr,&destination,&doubled));
 assert(nativeCalls==callsBefore+2);*(uint32_t*)(self+0x11c)=0;
 std::cout<<"PASS: personal ship double-click uses native Travel once, retains ship/terminal/passengers, and handles missing actions safely.\n";
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
 BYTE* destroyFake=(BYTE*)VirtualAlloc(nullptr,itemDestroyConfirmRva+64,MEM_COMMIT|MEM_RESERVE,PAGE_READWRITE);assert(destroyFake);
 const BYTE destroySignature[]={0x8a,0x45,0x0c,0x84,0xc0,0x0f,0x85,0xb3,0,0,0,0xe8,0xe0,0x64,0x03,0,0x84,0xc0,0x0f,0x84,0xa6,0,0,0};
 memcpy(destroyFake+itemDestroyConfirmRva,destroySignature,sizeof(destroySignature));
 assert(InstallInstantItemDestroy(destroyFake));
 const BYTE destroyConfirmed[]={0xe9,0xb4,0,0,0,0x90};
 assert(!memcmp(destroyFake+itemDestroyConfirmRva,destroySignature,5));
 assert(!memcmp(destroyFake+itemDestroyConfirmRva+5,destroyConfirmed,6));
 assert(!memcmp(destroyFake+itemDestroyConfirmRva+11,destroySignature+11,sizeof(destroySignature)-11));
 assert(InstallInstantItemDestroy(destroyFake));
 assert(itemDestroyConfirmRva+5+5+*(int32_t*)(destroyFake+itemDestroyConfirmRva+6)==0x4da94e);
 memcpy(destroyFake+itemDestroyConfirmRva,destroySignature,sizeof(destroySignature));destroyFake[itemDestroyConfirmRva+12]^=1;
 BYTE destroyBefore[sizeof(destroySignature)];memcpy(destroyBefore,destroyFake+itemDestroyConfirmRva,sizeof(destroyBefore));
 assert(!InstallInstantItemDestroy(destroyFake));assert(!memcmp(destroyBefore,destroyFake+itemDestroyConfirmRva,sizeof(destroyBefore)));
 VirtualFree(destroyFake,0,MEM_RELEASE);
 std::cout<<"PASS: inventory destroy uses the native confirmed path, preserves adjacent instructions, targets the original request path, is idempotent and rejects mismatched signatures.\n";
 BYTE toolbar[0xb0]{};ToolbarVector panes[8]{};BYTE rowStorage[8][36*0x48]{};ToolbarVector outer{(BYTE*)panes,(BYTE*)(panes+8),(BYTE*)(panes+8)};*(ToolbarVector**)(toolbar+0xa0)=&outer;
 for(int i=0;i<8;i++)panes[i]={(BYTE*)rowStorage[i],(BYTE*)rowStorage[i]+24*0x48,(BYTE*)rowStorage[i]+36*0x48};
 assert(InspectToolbarShape(toolbar)==ToolbarShape::NeedsExpansion);
 for(int i=0;i<8;i++)panes[i].end=panes[i].begin+36*0x48;
 assert(InspectToolbarShape(toolbar)==ToolbarShape::Complete);
 panes[3].end++;assert(InspectToolbarShape(toolbar)==ToolbarShape::Unreadable);panes[3].end--;
 nativeSettingsSaveInteger=FakeSaveInteger;nativeStringCtor=(GameStringCtor)FakeStringCtor;nativeStringAssign=(GameStringAssign)FakeStringAssign;nativeStringDtor=(GameStringDtor)FakeStringDtor;
 int panesMarker;numPanesKey=&panesMarker;fakeToolbarSettings.clear();assert(SaveToolbarShape(toolbar));assert(fakeToolbarSettings.size()==9);
 assert(fakeToolbarSettings[0]==std::make_pair(std::string("numPanes"),8));
 for(int i=0;i<8;i++)assert(fakeToolbarSettings[i+1]==std::make_pair(std::string("numItems_")+char('0'+i),36));
 BYTE slots[0xb0]{},backgrounds[0xb0]{};*(void**)(toolbar+0x10)=slots;*(void**)(toolbar+0x98)=backgrounds;*(long*)(slots+0x34)=300;*(long*)(backgrounds+0x34)=500;
 fakeSlots=slots;fakeToolbarUpdates=0;nativeWidgetSetSize=(WidgetSetSize)FakeWidgetSetSize;nativeToolbarUpdate=(ToolbarProcedure)FakeToolbarUpdate;ResizeThirdToolbar(toolbar);
 assert(*(long*)(slots+0xa8)==116&&fakeSlotSize[0]==412&&fakeSlotSize[1]==116&&fakeBackgroundSize[0]==500&&fakeBackgroundSize[1]==98&&fakeToolbarUpdates==1);
 std::cout<<"PASS: third toolbar detects 24/36-slot panes, writes eight 36-slot settings, and expands both native UI rows without command-action registration.\n";
 TestJunkDoubleClick();
 auto create=Proc("DirectInput8Create");assert(create!=nullptr); // unsupported test host must leave system input available
 std::cout<<"PASS: native single/double-click routing, destination membership, direct marker, debounce, ordinary ticket preservation, and unsupported-host input forwarding.\n";
}
