static __declspec(naked) const uint32_t* TestRadarBridge(const void*){__asm{
 push ebp
 mov ebp,esp
 sub esp,10h
 mov eax,dword ptr [ebp+8]
 mov dword ptr [ebp-10h],eax
 push 4
 call CitizenRadarColorBridge
 add esp,4
 mov esp,ebp
 pop ebp
 ret
}}
static __declspec(naked) uint32_t TestVisibilityBridge(const void*){__asm{
 push ebp
 mov ebp,esp
 sub esp,10h
 mov eax,dword ptr [ebp+8]
 mov dword ptr [ebp-10h],eax
 call CitizenRadarVisibilityBridge
 mov eax,edx
 mov esp,ebp
 pop ebp
 ret
}}
static uint32_t testCitizenFlags;static int forwardedMapType;
static uint32_t __fastcall FakeMapFlags(const void*,void*){return testCitizenFlags;}
static const uint32_t whiteMap=0xffffffffu,redMap=0xff0000ffu;
static const uint32_t* __cdecl FakeTypeColor(int type){forwardedMapType=type;return &whiteMap;}
static const uint32_t* __cdecl FakeObjectColor(const void*,bool ignore){return ignore?&whiteMap:&redMap;}
static void TestCitizenMapColor(){
 BYTE object[0x390]{};void* table[0xa4/4]{};table[0xa0/4]=(void*)&FakeMapFlags;*(void***)object=table;
 nativeMapTypeColor=FakeTypeColor;nativeMapObjectColor=FakeObjectColor;
 testCitizenFlags=citizenMapMarker;assert(TestRadarBridge(object)==&citizenOrange);assert(TestVisibilityBridge(object)==0);assert(*CitizenRadarColor(4,object)==citizenOrange);
 assert(CitizenRadarColor(13,object)==&whiteMap&&forwardedMapType==13);
 assert(CitizenRadarColor(2,object)==&whiteMap&&forwardedMapType==2);
 assert(*CitizenOverheadColor(object,true)==citizenOrange);assert(CitizenOverheadColor(object,false)==&redMap);
 *(uint32_t*)(object+0x38c)=0x100;assert(CitizenRadarVisibility(object)==0);
 testCitizenFlags=0;assert(CitizenRadarColor(4,object)==&whiteMap);assert(CitizenOverheadColor(object,true)==&whiteMap);assert(CitizenRadarVisibility(object)==1);
 assert(!HasCitizenMapMarker(nullptr));assert(CitizenRadarVisibility(nullptr)==1);
 BYTE* base=(BYTE*)VirtualAlloc(nullptr,0x1600000,MEM_COMMIT|MEM_RESERVE,PAGE_READWRITE);assert(base);
 for(const auto& c:citizenMapChecks)memcpy(base+c.rva,c.bytes,c.count);
 base[citizenMapChecks[4].rva]^=1;assert(!InstallCitizenMapColor(base));base[citizenMapChecks[4].rva]^=1;
 assert(InstallCitizenMapColor(base));assert(InstallCitizenMapColor(base));
 assert(base[0xbfe0f7]==0xe8&&base[0x2de7d1]==0xe8&&base[0xbfe0bb]==0xe8);
 assert(base[0xbfe0c7]==0xf6&&base[0xbfe0f6]==0x53&&base[0xbfe0fc]==0x8b);
 VirtualFree(base,0,MEM_RELEASE);citizenInstalledBase=nullptr;
 std::cout<<"PASS: citizen-only orange radar/map colors, native corpse/group/target handling, visibility, frame bridges, signature rejection and idempotent installation.\n";
}
