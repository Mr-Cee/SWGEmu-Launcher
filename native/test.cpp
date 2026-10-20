#include "travel.cpp"
#include <cassert>
#include <iostream>
static int nativeCalls,forwarded,closed;static std::wstring received;
static uint32_t __cdecl FakeQueue(uint32_t,const void*,const NativeWide* text,int,int){nativeCalls++;received.assign(text->first,text->last);return 1;}
static void __fastcall FakeButton(void* self,void*,void* context){if(context==*(void**)((BYTE*)self+0xa8)){std::wstring s=L"naboo Theed naboo Keren 1";NativeWide n{s.data(),s.data()+s.size(),s.data()+s.size()+1};QueueHook(1,nullptr,&n,0,0);}}
static void __fastcall FakeClose(void*,void*){closed++;}
static bool __fastcall FakeMessage(void*,void*,void*,const void*){forwarded++;return true;}
int main(){
 nativeQueue=FakeQueue;nativeButton=(Button)FakeButton;nativeClose=(void(__thiscall*)(void*))FakeClose;trampoline=(void*)FakeMessage;
 BYTE self[0x200]{};int destination,purchase;void* elements[]={&destination};void* vector[]={elements,elements+1,elements+1};*(void***)(self+0xe0)=vector;*(void**)(self+0xa8)=&purchase;
 int single=7,doubled=11;
 assert(MessageHook(self,nullptr,&destination,&single));assert(forwarded==1&&nativeCalls==0);
 assert(!MessageHook(self,nullptr,&destination,&doubled));assert(received==L"private_direct naboo Theed naboo Keren direct"&&nativeCalls==1&&closed==1);
 MessageHook(self,nullptr,&destination,&doubled);assert(nativeCalls==1);
 assert(MessageHook(self,nullptr,&purchase,&doubled));assert(forwarded==2&&nativeCalls==1);
 std::wstring normal=L"a b c d 1";NativeWide n{normal.data(),normal.data()+normal.size(),normal.data()+normal.size()+1};QueueHook(1,nullptr,&n,0,0);assert(received==normal);
 auto create=Proc("DirectInput8Create");assert(create!=nullptr); // unsupported test host must leave system input available
 std::cout<<"PASS: native single/double-click routing, destination membership, direct marker, debounce, ordinary ticket preservation, and unsupported-host input forwarding.\n";
}
