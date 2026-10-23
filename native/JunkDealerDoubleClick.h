// Supported pre-CU UIList has no double-click branch. Keep every other list native.
using ListProcess=bool(__thiscall*)(void*,const void*);
using ListHit=bool(__thiscall*)(void*,const void*,long*);
using ListCount=long(__thiscall*)(void*);
using ListSelect=void(__thiscall*)(void*,long);
using FindDefault=void*(__thiscall*)(void*,bool);
using PressButton=void(__thiscall*)(void*);
static ListProcess originalListProcess;
static ListHit nativeListHit;
static ListCount nativeListCount;
static ListSelect nativeListSelect;
static FindDefault nativeFindDefault;
static PressButton nativePressButton;
static void* lastJunkPage;
static DWORD lastJunkTick;
constexpr uintptr_t listProcessRva=0xd3a610,listSlotRva=0x11fb378;
static bool WidgetName(void* widget,const char* expected){
 if(!Readable(widget,0x18))return false;
 const char* begin=*(const char**)((BYTE*)widget+8);
 const char* end=*(const char**)((BYTE*)widget+12);
 size_t length=strlen(expected);
 return (uintptr_t)end>=(uintptr_t)begin&&(uintptr_t)end-(uintptr_t)begin==length&&
     Readable(begin,length)&&!_strnicmp(begin,expected,length);
}
static void* WidgetParent(void* widget){return Readable(widget,0x18)?*(void**)((BYTE*)widget+0x14):nullptr;}
static bool __fastcall JunkListProcessHook(void* self,void*,const void* message){
 if(!Readable(message,0x1c)||*(const int*)message!=11||!WidgetName(self,"lstList"))
  return originalListProcess(self,message);
 void* listPage=WidgetParent(self);void* page=WidgetParent(listPage);
 if(!WidgetName(listPage,"List")||!WidgetName(page,"privateJunkDealerSell")||!Readable(self,0x150))
  return originalListProcess(self,message);
 void* style=*(void**)((BYTE*)self+0x144);
 if(!Readable(style,0x74))return originalListProcess(self,message);
 long count=nativeListCount(self),row=-1;
 long long stride=(long long)*(long*)((BYTE*)style+0x48)+*(long*)((BYTE*)style+0x50);
 long long y=(long long)*(const long*)((const BYTE*)message+0x18)+*(long*)((BYTE*)self+0x40)-*(long*)((BYTE*)style+0x70);
 // Native hit-testing clamps blank space to the final item: reject it explicitly.
 if(count<=0||count>10000||stride<=0||y<0||y/stride>=count||
    !nativeListHit(self,(const BYTE*)message+0x14,&row)||row<0||row>=count||row!=y/stride)
  return originalListProcess(self,message);
 void* button=nativeFindDefault(page,true);
 if(!WidgetName(button,"btnOk"))return originalListProcess(self,message);
 DWORD now=GetTickCount();
 if(lastJunkPage==page&&lastJunkTick&&now-lastJunkTick<250)return true;
 nativeListSelect(self,row); // same selection/notifications as a normal click
 lastJunkPage=page;lastJunkTick=now;
 nativePressButton(button); // existing single-sale callback and server checks
 Log("Junk dealer double-click: selected item Sell action submitted.");
 return true;
}
struct JunkCodeCheck {uintptr_t rva;const char* bytes;size_t size;};
static const JunkCodeCheck junkCodeChecks[]={
 {listProcessRva,"\x83\xec\x1c\x53\x55\x8b\x6c\x24\x28\x56\x57\x55\x8b\xf1",14},
 {0xd3a500,"\x51\x53\x8b\xd9\x8b\x93\x44\x01\x00\x00\x85\xd2",12},
 {0xd3ace0,"\x8b\x89\x20\x01\x00\x00\x85\xc9\x74\x05\x8b\x01\xff\x60\x54",15},
 {0xd3a280,"\x51\x53\x55\x8b\x6c\x24\x10\x85\xed\x8b\xd9",11},
 {0xcffb20,"\x55\x8b\xec\x51\x53\x8b\x5d\x08\x84\xdb\x56\x8b\xf1",13},
 {0xd16200,"\x56\x8b\xf1\xf6\x46\x7c\x04\x74\x46\x33\xc0",11},
 {0xcf2e50,"\x55\x8b\xec\x83\xec\x18\x8b\x45\x08\x53\x56\x8d\x71\x08",14}
};
static bool InstallJunkDoubleClick(BYTE* base){
 for(const auto& check:junkCodeChecks)
  if(!Readable(base+check.rva,check.size)||memcmp(base+check.rva,check.bytes,check.size))return false;
 auto slot=(void**)(base+listSlotRva);
 if(!Readable(slot,sizeof(*slot)))return false;
 void* hook=(void*)&JunkListProcessHook;
 if(*slot==hook)return true;
 if(*slot!=base+listProcessRva)return false;
 originalListProcess=(ListProcess)(base+listProcessRva);
 nativeListHit=(ListHit)(base+0xd3a500);nativeListCount=(ListCount)(base+0xd3ace0);
 nativeListSelect=(ListSelect)(base+0xd3a280);nativeFindDefault=(FindDefault)(base+0xcffb20);
 nativePressButton=(PressButton)(base+0xd16200);
 return Patch(slot,&hook,sizeof(hook));
}
